using Microsoft.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Kafka;
using System.EventSourcing.Client;
using System.EventSourcing.Client.Serialization;
using System.EventSourcing.Client.Kafka;
using System.EventSourcing.Client.Reflection;

namespace SimpleEventHost
{
    class Program
    {
        static void Main()
        {
            var host = new WebHostBuilder()
                .UseKafka(
                    x =>
                    {
                        x.BootstrapServers = new[] { "localhost:9092" };
                        x.Topics = new[] { "system.events" };
                        x.ConsumerGroup = "services.sample";
                    })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}