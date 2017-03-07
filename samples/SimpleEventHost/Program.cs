using Microsoft.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Kafka;
using System.EventSourcing.Client;
using System.EventSourcing.Client.Serialization;
using System.EventSourcing.Client.Kafka;

namespace SimpleEventHost
{
    class Program
    {
        static void Main()
        {
            var client = new EventClient()
                //.UseReflectionNameResolution()
                .UseJsonSerialization()
                .UseKafka("system.events", "localhost:9092");

            client.Publish(new TEvent { T = "test" }).Wait();

            var host = new WebHostBuilder()
                .UseKafka()
                //.UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}