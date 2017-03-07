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
        readonly IDictionary<string, object> _config = new Dictionary<string, object>
        {
           { "group.id", "simple-csharp-consumer" },
           //{ "enable.auto.commit", true },
           //{ "auto.commit.interval.ms", 5000 },
           //{ "statistics.interval.ms", 60000 },
           { "bootstrap.servers", "localhost:9092" },
           //{ "default.topic.config", new Dictionary<string, object>()
           //    {
           //        { "auto.offset.reset", "smallest" }
           //    }
           //}
        };

        IObservable<Message<string, byte[]>> _eventPipeline;

        public IFeatureCollection Features { get; set; } = new FeatureCollection();

        public KafkaListener(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _regex = new Regex(_pattern);

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
            kafka_consumer.OnPartitionEOF += (_, end) => Console.WriteLine($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");
            kafka_consumer.OnMessage += (_, msg) => { };
            kafka_consumer.OnStatistics += (_, json)
                => Console.WriteLine($"Statistics: {json}");
            kafka_consumer.Subscribe("system.events");

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

//using System;
//using Microsoft.AspNetCore.Hosting.Server;
//using Microsoft.AspNetCore.Http.Features;
//using System.Threading.Tasks;
//using System.Reactive.Linq;
//using System.Threading;
//using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
//using Microsoft.AspNetCore.Http;
//using System.IO;
//using Microsoft.AspNetCore.Hosting.Internal;
//using Microsoft.Extensions.DependencyInjection;
//using Confluent.Kafka;
//using System.Collections.Generic;
//using Confluent.Kafka.Serialization;
//using System.Text;
//using System.EventSourcing.AspNetCore.Kafka.Serialization;
//using System.Text.RegularExpressions;

//namespace System.EventSourcing.AspNetCore.Kafka
//{
//    class KafkaListener : IServer
//    {
//        static IDictionary<string, string> event_action_map = new Dictionary<string, string>()
//        {
//            { "created", "PUT" },
//            { "deleted", "DELETE" },
//            { "updated", "POST" }
//        };

//        Task listener;
//        CancellationTokenSource cancellationSrc;
//        IServiceScopeFactory _scopeFactory;

//        public IFeatureCollection Features => new FeatureCollection();

//        public KafkaListener(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//        }

//        public void Dispose()
//        {
//            cancellationSrc.Cancel();
//            listener.Wait();
//        }

//        public void Start<TContext>(IHttpApplication<TContext> application)
//        {
//            cancellationSrc = new CancellationTokenSource();

//            var config = new Dictionary<string, object>
//            {
//                { "group.id", "simple-csharp-consumer" },
//                { "bootstrap.servers", "localhost:9092" }
//            };

//            var consumer = new Consumer<string, byte[]>(config, new StringDeserializer(Encoding.UTF8), new ByteDeserializer());

//            consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset("system.events", 0, 0) });

//            var producer = new Producer<string, byte[]>(config, new StringSerializer(Encoding.UTF8), new ByteSerializer());
//            var deliveryReport = producer.ProduceAsync("system.events", "user.created", Encoding.UTF8.GetBytes("{ \"t\":\"test\"}"));
//            deliveryReport.ContinueWith(task =>
//            {
//                Console.WriteLine($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset}");
//            });

//            listener = Task.Run(
//                async () =>
//                {
//                    var pattern = @"^(?<subject>.*)\.(?<action>.*)$";
//                    var regex = new Regex(pattern);

//                    await Observable.Generate(0,
//                        i => !cancellationSrc.IsCancellationRequested,
//                        i => i + 1,
//                        i => i,
//                        i => TimeSpan.FromMilliseconds(1))
//                        .Select(
//                            x =>
//                            {
//                                Message<string, byte[]> msg;
//                                if (consumer.Consume(out msg, 1000))
//                                {
//                                    return msg;
//                                }

//                                Console.WriteLine("No messages in the queue!");

//                                return null;
//                            })
//                        .Where(x => x != null)
//                        .Select(x =>
//                            {
//                                var context = application.CreateContext(Features);

//                                var match = regex.Match(x.Key);

//                                if (context is Context && match.Success)
//                                {
//                                    var ctx = (Context)Convert.ChangeType(context, typeof(Context));

//                                    var httpContext = new DefaultHttpContext();

//                                    var subject = match.Groups["subject"].Value;
//                                    var action = match.Groups["action"].Value;

//                                    var features = new FeatureCollection();
//                                    features.Set<IHttpRequestFeature>(new HttpRequestFeature
//                                    {
//                                        Method = event_action_map[action],
//                                        Path = $"/v1/events/{subject}",
//                                        Body = new MemoryStream(x.Value),
//                                        Headers = new HeaderDictionary { { "content-type", "application/json" } }
//                                    });
//                                    features.Set<IHttpResponseFeature>(new HttpResponseFeature
//                                    {                                        
//                                        Body = new MemoryStream()
//                                    });
//                                    features.Set<IServiceProvidersFeature>(new RequestServicesFeature(_scopeFactory));
//                                    httpContext.Initialize(features);

//                                    ctx.HttpContext = httpContext;

//                                    return (TContext)Convert.ChangeType(ctx, typeof(TContext));
//                                }

//                                return context;
//                            })
//                        .Do(
//                            async x =>
//                            {
//                                await application.ProcessRequestAsync(x);
//                            })
//                        .LastAsync();
//                });
//        }
//    }
//}