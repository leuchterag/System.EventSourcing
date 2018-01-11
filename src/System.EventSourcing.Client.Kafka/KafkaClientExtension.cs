using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.EventSourcing.Kafka;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Kafka
{
    public static class KafkaClientExtension
    {
        public static EventClient UseKafka(this EventClient subject, string topic, string bootstrapServers)
        {
            var config = new Dictionary<string, object> { { "bootstrap.servers", bootstrapServers } };
            var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8));

            subject.UseHandle(
                async evnt =>
                {
                    var kafkaEvent = new KafkaEvent { Tags = evnt.Tags, Content = JToken.Parse(evnt.Content)};
                    var strContent = await Task.Run(() => JsonConvert.SerializeObject(kafkaEvent));
                    var message = await producer.ProduceAsync(topic, evnt.Name, strContent);
                });

            return subject;
        }
    }
}
