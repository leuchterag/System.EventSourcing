using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.EventSourcing.Hosting.Json;
using System.EventSourcing.Hosting;
using System.EventSourcing.Hosting.Kafka;

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
                .AddEventSourcing(es =>
                {
                    es.FromKafka()
                        .UseProjections()
                        .AddProjection<SampleProjection>();
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
