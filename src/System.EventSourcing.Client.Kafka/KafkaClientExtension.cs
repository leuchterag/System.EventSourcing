using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.EventSourcing.Kafka;
using System.EventSourcing.Kafka.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Kafka
{
    public static class KafkaClientExtension
    {
        public static EventClient UseKafka(this EventClient subject, string topic, string bootstrapServers)
        {
            var config = new Dictionary<string, object> { { "bootstrap.servers", bootstrapServers } };
            var producer = new Producer<string, byte[]>(config, new StringSerializer(Encoding.UTF8), new ByteSerializer());

            subject.UseHandle(
                async evnt =>
                {
                    var kafkaEvent = new KafkaEvent { Tags = evnt.Tags, Content = evnt.Content };
                    var strContent = await Task.Run(() => JsonConvert.SerializeObject(kafkaEvent));
                    var content = Encoding.UTF8.GetBytes(strContent);
                    await producer.ProduceAsync(topic, evnt.Name, content);
                });

            return subject;
        }
    }
}
