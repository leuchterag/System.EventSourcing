using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System.Collections.Generic;
using System.EventSourcing.Kafka.Serialization;
using System.Text;

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
                    var msg = await producer.ProduceAsync(topic, evnt.Name, evnt.Content);
                });

            return subject;
        }
    }
}
