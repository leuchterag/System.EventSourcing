﻿using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using System.Collections.Generic;
using Confluent.Kafka.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.EventSourcing.Kafka.Serialization;
using System.Linq;
using Microsoft.Extensions.Options;

namespace System.EventSourcing.AspNetCore.Kafka
{
    class KafkaListener : IServer
    {
        static readonly IDictionary<string, string> event_action_map = new Dictionary<string, string>()
        {
            { "created", "PUT" },
            { "deleted", "DELETE" },
            { "updated", "POST" },
            { "added", "PUT" }
        };

        Task listener;
        CancellationTokenSource cancellationSrc;
        readonly IServiceScopeFactory _scopeFactory;
        Consumer<string, byte[]> kafka_consumer;
        string _pattern = @"^(?<subject>[a-zA-Z0-9_]*)\.(?<action>[a-zA-Z0-9_]*)$";
        readonly Regex _regex;
        readonly KafkaListenerSettings _config;

        IObservable<Message<string, byte[]>> _eventPipeline;

        public IFeatureCollection Features { get; set; } = new FeatureCollection();

        public KafkaListener(IServiceScopeFactory scopeFactory, IOptions<KafkaListenerSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _regex = new Regex(_pattern);
            _config = settings.Value;
        }

        public void Dispose()
        {
            cancellationSrc.Cancel();
            listener.Wait();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            cancellationSrc = new CancellationTokenSource();

            kafka_consumer = new Consumer<string, byte[]>(_config, new StringDeserializer(Encoding.UTF8), new ByteDeserializer());

            kafka_consumer.OnPartitionsAssigned += (_, list) => { kafka_consumer.Assign(list); };
            kafka_consumer.OnPartitionEOF += (_, end) => Console.WriteLine($"End of Topic {end.Topic} partition {end.Partition}, next offset {end.Offset}");

            kafka_consumer.Subscribe(_config.Topics.ToList().First());

            _eventPipeline = Observable
                .FromEventPattern<Message<string, byte[]>>(
                    h => kafka_consumer.OnMessage += h,
                    h => kafka_consumer.OnMessage -= h)
                .Select(x => x.EventArgs);

            listener = Task.Run(() =>
                _eventPipeline.Select(
                x =>
                {
                    var match = _regex.Match(x.Key);

                    if (match.Success)
                    {
                        var subject = match.Groups["subject"].Value;
                        var action = match.Groups["action"].Value;

                        var requestFeature = new HttpRequestFeature
                        {
                            Method = event_action_map[action],
                            Path = $"/v1/events/{subject}",
                            Body = new MemoryStream(x.Value),
                            Protocol = "http",
                            Scheme = "http",
                            Headers = new HeaderDictionary { { "Content-Type", "application/json" } }
                        };

                        var requestFeatures = new FeatureCollection();
                        requestFeatures.Set<IHttpRequestFeature>(requestFeature);

                        var context = application.CreateContext(requestFeatures);

                        if (context is Context)
                        {
                            var ctx = (Context)Convert.ChangeType(context, typeof(Context));

                            var httpContext = new DefaultHttpContext();

                            requestFeatures.Set<IHttpResponseFeature>(new HttpResponseFeature
                            {
                                Body = new MemoryStream()
                            });
                            requestFeatures.Set<IServiceProvidersFeature>(new RequestServicesFeature(_scopeFactory));

                            httpContext.Initialize(requestFeatures);

                            ctx.HttpContext = httpContext;

                            return (TContext)Convert.ChangeType(ctx, typeof(TContext));
                        }
                    }

                    return default(TContext);
                })
            .Select(
                async x =>
                {
                    await application.ProcessRequestAsync(x);
                })
            .Subscribe(x => { }, ex => { }, () => { }, cancellationSrc.Token));

            listener = Task.Run(() =>
            {
                while (!cancellationSrc.Token.IsCancellationRequested)
                {
                    kafka_consumer.Poll(TimeSpan.FromSeconds(1));
                }
            });
        }
    }
}
