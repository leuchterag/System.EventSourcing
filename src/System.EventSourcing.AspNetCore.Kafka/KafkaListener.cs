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
using Confluent.Kafka.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.EventSourcing.Kafka.Serialization;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.EventSourcing.Kafka;
using System.Web;

namespace System.EventSourcing.AspNetCore.Kafka
{
    class KafkaListener : IServer
    {

        Task listener;
        CancellationTokenSource cancellationSrc;
        readonly IServiceScopeFactory _scopeFactory;
        Consumer<string, byte[]> kafka_consumer;
        string _pattern = @"^(?<subject>[a-zA-Z0-9\.\/_-]*)\.(?<action>[a-zA-Z0-9_]*)$";
        readonly Regex _regex;
        readonly KafkaListenerSettings _config;
        readonly ILogger _logger;

        public IFeatureCollection Features { get; set; } = new FeatureCollection();

        public KafkaListener(IServiceScopeFactory scopeFactory, IOptions<KafkaListenerSettings> settings, ILogger<KafkaListener> logger)
        {
            _scopeFactory = scopeFactory;
            _regex = new Regex(_pattern);
            _config = settings.Value;
            _logger = logger;

            cancellationSrc = new CancellationTokenSource();
        }

        public void Dispose()
        {
            cancellationSrc.Cancel();
            listener.Wait();
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            kafka_consumer = new Consumer<string, byte[]>(_config, new StringDeserializer(Encoding.UTF8), new ByteDeserializer());

            kafka_consumer.OnPartitionsAssigned +=
                (_, parts) =>
                {
                    _logger.LogInformation($"Consumer was assigned to topics: {string.Join(" ,", parts)}");
                    kafka_consumer.Assign(parts);
                };

            kafka_consumer.OnPartitionsRevoked +=
                (_, parts) =>
                {
                    _logger.LogInformation($"Consumer was unassigned from topics: {string.Join(" ,", parts)}");
                    kafka_consumer.Unassign();
                };

            kafka_consumer.OnPartitionEOF += (_, end) => _logger.LogInformation($"End of Topic {end.Topic} partition {end.Partition} reached, next offset {end.Offset}");

            kafka_consumer.OnError +=
                (_, error) =>
                {
                    _logger.LogError($"Listener failed: {error.Code} - {error.Reason}");
                };

            kafka_consumer.OnConsumeError += (_, error) => { };

            kafka_consumer.Subscribe(_config.Topics.ToList());

            listener = Task.Run(async () =>
            {
                while (!cancellationSrc.Token.IsCancellationRequested)
                {
                    // Consume from stream in order
                    kafka_consumer.Consume(out var msg, TimeSpan.FromSeconds(1));

                    if (msg != null)
                    {
                        try
                        {
                            var match = _regex.Match(msg.Key);

                            if (match.Success)
                            {
                                var subject = HttpUtility.UrlEncode(match.Groups["subject"].Value);
                                var action = match.Groups["action"].Value;

                                var evnt = JsonConvert.DeserializeObject<KafkaEvent>(Encoding.UTF8.GetString(msg.Value));

                                var headers = new HeaderDictionary { { "Content-Type", "application/json" } };

                                foreach (var tag in evnt.Tags)
                                {
                                    headers.Add(tag.Key, tag.Value);
                                }

                                var body = Encoding.UTF8.GetBytes(evnt.Content.ToString());

                                var requestFeature = new HttpRequestFeature
                                {
                                    Method = "PUT",
                                    Path = $"/v1/events/{subject}.{action}",
                                    Body = new MemoryStream(body),
                                    Protocol = "http",
                                    Scheme = "http",
                                    Headers = headers
                                };

                                var responseFeature = new HttpResponseFeature
                                {
                                };

                                var requestFeatures = new FeatureCollection();
                                requestFeatures.Set<IHttpRequestFeature>(requestFeature);
                                requestFeatures.Set<IHttpResponseFeature>(responseFeature);

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

                                    var newContext = (TContext)Convert.ChangeType(ctx, typeof(TContext));

                                    try
                                    {
                                        await application.ProcessRequestAsync(newContext);
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogWarning($"failed to process a message: {e.Message} \n {e.StackTrace}");
                                        throw;
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"failed to parse message correctly");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning($"general error during handling of the event occured: {e.Message} \n {e.StackTrace}");
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationSrc.Cancel();

            var awaitForceShutdown = Task.Run(() => cancellationToken.WaitHandle.WaitOne());

            if(await Task.WhenAny(listener, awaitForceShutdown) == awaitForceShutdown)
            {
                _logger.LogWarning("Kafka listener did not terminated in the allotted time and will be forced.");
                return;
            }

            _logger.LogInformation("Kafka listener terminated succesfully");
        }
    }
}
