using System;
using System.EventSourcing.Client;
using System.EventSourcing.Reflection;
using System.EventSourcing.Client.Kafka;
using System.EventSourcing.Client.Reflection;
using System.EventSourcing.Client.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using Confluent.Kafka;

namespace SimpleProducer
{
    class Program
    {
        static async Task Main()
        {
            var client = new EventClient()
                .UseKafka("topic1", "localhost:29092")
                .UseReflectionNameResolution()
                .UseJsonSerialization();
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                await client.Publish(new SampleEvent { Id = Guid.NewGuid().ToString().ToLowerInvariant() });
            }
            stopwatch.Stop();
            Console.WriteLine($"Produced 100 messages in {stopwatch.ElapsedMilliseconds}ms");

            var config = new ProducerConfig { BootstrapServers = "localhost:29092" };
            var builder = new ProducerBuilder<string, string>(config);
            using (var producer = builder.Build())
            {
                var fastclient = new EventClient()
                    .UseKafka("topic1", () => producer)
                    .UseReflectionNameResolution()
                    .UseJsonSerialization();
                stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < 100; i++)
                {
                    await fastclient.Publish(new SampleEvent { Id = Guid.NewGuid().ToString().ToLowerInvariant() });
                }
                stopwatch.Stop();
                Console.WriteLine($"Fast client produced 100 messages in {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }

    [Event("https://sample.service.com/v1/sample", "created")]
    public class SampleEvent
    {
        public string Id { get; set; }
    }
}
