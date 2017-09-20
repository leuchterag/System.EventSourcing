using Microsoft.AspNetCore.Hosting.Server;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.EventSourcing.Kafka;
using System.Reactive.Subjects;

namespace System.EventSourcing.AspNetCore.Kafka
{
    class KafkaListener : IServer
    {

        Task listener;
        CancellationTokenSource cancellationSrc;
        readonly IServiceScopeFactory _scopeFactory;
        Consumer<string, byte[]> kafka_consumer;
        string _pattern = @"^(?<subject>[a-zA-Z0-9_]*)\.(?<action>[a-zA-Z0-9_]*)$";
        readonly Regex _regex;
        readonly KafkaListenerSettings _config;
        readonly ILogger _logger;

        IObservable<Message<string, byte[]>> _eventPipeline;

        public IFeatureCollection Features { get; set; } = new FeatureCollection();

        public KafkaListener(IServiceScopeFactory scopeFactory, IOptions<KafkaListenerSettings> settings, ILogger<KafkaListener> logger)
        {
            _scopeFactory = scopeFactory;
            _regex = new Regex(_pattern);
            _config = settings.Value;
            _logger = logger;
        }

        public void Dispose()
        {
            cancellationSrc.Cancel();
            listener.Wait();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            bool isReceiving = false;
            cancellationSrc = new CancellationTokenSource();

            kafka_consumer = new Consumer<string, byte[]>(_config, new StringDeserializer(Encoding.UTF8), new ByteDeserializer());

            kafka_consumer.OnPartitionsAssigned += 
                (_, parts) =>
                {
                    _logger.LogInformation($"Consumer was assigned to topics: {string.Join(" ,", parts)}");
                    kafka_consumer.Assign(parts);

                    if(kafka_consumer.Assignment.Any())
                    {
                        isReceiving = true;
                    }
                };

            kafka_consumer.OnPartitionsRevoked += 
                (_, parts) =>
                {
                    _logger.LogInformation($"Consumer was unassigned from topics: {string.Join(" ,", parts)}");
                    kafka_consumer.Unassign();

                    if (kafka_consumer.Assignment.Any())
                    {
                        isReceiving = true;
                    }
                    else
                    {
                        isReceiving = false;
                    }
                };

            kafka_consumer.OnPartitionEOF += (_, end) => _logger.LogInformation($"End of Topic {end.Topic} partition {end.Partition} reached, next offset {end.Offset}");

            kafka_consumer.OnError +=
                (_, error) =>
                {
                    _logger.LogError($"Listener failed: {error.Code} - {error.Reason}");
                };

            kafka_consumer.OnConsumeError += (_, error) => { };

            kafka_consumer.Subscribe(_config.Topics.ToList());

            var _eventPipeline = new Subject<Message<string, byte[]>>();

            var canread = new AutoResetEvent(true);

            listener = Task.Run(() =>
            {
                _eventPipeline
                .Select(
                    x =>
                    {
                        var match = _regex.Match(x.Key);

                        if (match.Success)
                        {
                            var subject = match.Groups["subject"].Value;
                            var action = match.Groups["action"].Value;

                            var evnt = JsonConvert.DeserializeObject<KafkaEvent>(Encoding.UTF8.GetString(x.Value));

                            var headers = new HeaderDictionary { { "Content-Type", "application/json" } };

                            foreach (var tag in evnt.Tags)
                            {
                                headers.Add(tag.Key, tag.Value);
                            }

                            var requestFeature = new HttpRequestFeature
                            {
                                Method = "PUT",
                                Path = $"/v1/events/{subject}.{action}",
                                Body = new MemoryStream(evnt.Content),
                                Protocol = "http",
                                Scheme = "http",
                                Headers = headers
                            };

                            var requestFeatures = new FeatureCollection();
                            requestFeatures.Set<IHttpRequestFeature>(requestFeature);

                            var context = application.CreateContext(requestFeatures);

                            if (context is Context)
                            {
                                var ctx = (Context)Convert.ChangeType(context, typeof(Context));

                                var httpContext = new DefaultHttpContext();

                                requestFeatures.Set<IHttpResponseFeature>(new FakeHttpReponseFeature
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
                            try
                            {
                                await application.ProcessRequestAsync(x);
                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning($"failed to process a message: {e.Message} \n {e.StackTrace}");
                                throw;
                            }
                        })
                    .Subscribe(
                            async x =>
                            {
                                canread.Set();
                                await kafka_consumer.CommitAsync();
                            },
                            async ex =>
                            {
                                _logger.LogInformation($"Encountered error while processing message: {ex.Message} \n {ex.StackTrace}");
                                canread.Set();
                                await kafka_consumer.CommitAsync();
                            },
                            () =>
                            {
                                _logger.LogInformation($"Stream processing of messages from kafka completed.");
                            },
                            cancellationSrc.Token);

                    });

            listener = Task.Run(() =>
            {
                while (!cancellationSrc.Token.IsCancellationRequested)
                {
                    kafka_consumer.Consume(out var msg, TimeSpan.FromSeconds(1));
                    if(msg != null)
                    {
                        canread.WaitOne();
                        _eventPipeline.OnNext(msg);
                    }
                }
            });
        }
    }
}
