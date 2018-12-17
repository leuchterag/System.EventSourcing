using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.EventSourcing.Hosting;
using System.EventSourcing.Hosting.Kafka;
using System.EventSourcing.Hosting.Reflection;

namespace SimpleEventHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseKafka()
                .ConfigureServices(container =>
                {
                    // Configuration for the kafka consumer
                    container.Configure<KafkaListenerSettings>(config =>
                    {

                        config.BootstrapServers = new[] { "localhost:29092" };
                        config.Topics = new[] { "topic1" };
                        config.ConsumerGroup = "group1";
                        config.DefaultTopicConfig = new Dictionary<string, object>
                        {
                            { "auto.offset.reset", "smallest" }
                        };
                    });
                })
                .AddEventSourcing(es =>
                {
                    es.FromKafka();
                    es.UseReflectionResolution();
                    
                    es.AddProjection<SampleProjection>();
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
