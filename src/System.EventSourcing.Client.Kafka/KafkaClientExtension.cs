using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.EventSourcing.Kafka;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Kafka
{
    public static class KafkaClientExtension
    {
        public static EventClient UseKafka(this EventClient subject, string topic, string bootstrapServers)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };

            subject.UseHandle(
                async evnt =>
                {
                    using (var p = new ProducerBuilder<string, string>(config).Build())
                    {
                        try
                        {
                            var kafkaEvent = new KafkaEvent { Tags = evnt.Tags, Content = JObject.Parse(evnt.Content) };
                            var strContent = await Task.Run(() => JsonConvert.SerializeObject(kafkaEvent));
                            await p.ProduceAsync(topic, new Message<string, string> { Key = evnt.Name, Value = strContent });
                        }
                        catch (ProduceException<string, string> e)
                        {
                            Console.WriteLine($"Delivery to Kafka failed: {e.Error.Reason}");
                            throw e;
                        }
                    }
                });

            return subject;
        }
    }
}
