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
                        .UseReflectionResolution()
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
