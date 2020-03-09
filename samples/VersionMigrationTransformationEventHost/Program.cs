using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.EventSourcing.Hosting;
using System.EventSourcing.Hosting.Kafka;
using System.EventSourcing.Hosting.Projections;
using System.EventSourcing.Hosting.Projections.Reflection;
using System.EventSourcing.Hosting.Json;
using System.EventSourcing.Hosting.Middleware;
using System.EventSourcing;
using System.EventSourcing.Hosting.Transformation;
using System.EventSourcing.Hosting.Json.Transformation;
using System.Text.RegularExpressions;

namespace SimpleEventHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseKafka(config =>
                {
                    // Configuration for the kafka consumer
                    config.BootstrapServers = new[] { "localhost:29092" };
                    config.Topics = new[] { "topic1" };
                    config.ConsumerGroup = "group1";
                    config.AutoOffsetReset = "Latest";
                    config.AutoCommitIntervall = 5000;
                    config.IsAutocommitEnabled = true;
                })
                // .UseProjections()
                .AddEventSourcing(es =>
                {
                    es.FromKafka()
                        // .UseReflectionResolution()
                        .Handle<string, JObject, StringJObjectContext>(
                            (k, v) => new StringJObjectContext { Content = v, Key = k },
                            (app) =>
                            {
                                app.UseTransformations(
                                    tran =>
                                    {
                                        tran
                                            .KeysMatchingRegex(@"v1/template.created")
                                            .Transform(
                                                (origin, transformed) =>
                                                {
                                                    var keyRegex = new Regex(@"(?<protocol>\w+):\/\/(?<domain>[\w@][\w.:@]+)(?<path>\/?[\w\.?=%&=\-@\/$,]*)");
                                                    if (keyRegex.IsMatch(origin.Key))
                                                    {
                                                        var match = keyRegex.Match(origin.Key);
                                                        transformed.Content = origin.Content;
                                                        transformed.Key = $"{match.Groups["protocol"]}://{match.Groups["domain"]}/v2{match.Groups["path"]}";
                                                    }
                                                })
                                            .DropOrigin();
                                        
                                        tran.HandleUntransformed();
                                    }
                                );

                                app
                                    .UseProjections(projApp =>
                                    {
                                        projApp.FromServiceCollection(es.Base);
                                        projApp.AttributeKeyResolution();
                                        projApp.KeyFrom(x => x.Key);
                                        projApp.ContentFromJObject();
                                    });
                                    
                            })
                        .AddProjection<SampleProjection>()
                        .AddProjection<SampleProjectionV2>();
                })
                .ConfigureLogging((ILoggingBuilder loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .Build();

            host.Run();
        }
    }
}
