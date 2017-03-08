using Microsoft.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Kafka;
using System.EventSourcing.Client.Kafka;

namespace SimpleEventHost
{
    class Program
    {
        static void Main()
        {
            var webhost = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<StartupWeb>()
                .Build();

            var host = new WebHostBuilder()
                .UseKafka(
                    x =>
                    {
                        x.BootstrapServers = new[] { "localhost:9092" };
                        x.Topics = new[] { "system.events" };
                        x.ConsumerGroup = "services.sample";
                    })
                .UseStartup<Startup>(x => { return webhost.Services; }, x => { })
                .Build();
            

            host.Run();

        }
    }
}