using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.EventSourcing.Kafka;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Kafka
{
    public static class KafkaClientExtension
    {
        /// <summary>
        /// Submitts messages to kafka on a given topic and using the given bootstrap servers.
        /// This method will create a new producer for every message submitted.
        /// </summary>
        /// <param name="subject">The <c>EventClient</c> to extend</param>
        /// <param name="topic">The topic to produce messages to</param>
        /// <param name="bootstrapServers">the boostrap servers</param>
        /// <returns>the modified event client.</returns>
        public static EventClient UseKafka(this EventClient subject, string topic, string bootstrapServers)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            var builder = new ProducerBuilder<string, string>(config);

            subject.UseHandle(
                async evnt =>
                {
                    using (var producer = builder.Build())
                    {
                        try
                        {
                            var kafkaEvent = new KafkaEvent { Tags = evnt.Tags, Content = JObject.Parse(evnt.Content) };
                            var strContent = await Task.Run(() => JsonConvert.SerializeObject(kafkaEvent));
                            await producer.ProduceAsync(topic, new Message<string, string> { Key = evnt.Name, Value = strContent });
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

        /// <summary>
        /// Submitts messages to kafka on a given topic and using the given bootstrap servers.
        /// In contrast to <c>UseKafka(this EventClient subject, string , string)</c>, this extension gives controll over how a producer is created.
        /// This requires the lifetime and deallocation to be managed externally but enables reusing producers for as long as it's needed and thus have much higher throughput for production of messages.
        /// </summary>
        /// <param name="subject">The <c>EventClient</c> to extend</param>
        /// <param name="topic">The topic to produce messages to</param>
        /// <param name="producerFactory">A factory function that returnes a ready to use Kafka producer.</param>
        /// <returns></returns>
        public static EventClient UseKafka(this EventClient subject, string topic, Func<IProducer<string, string>> producerFactory)
        {
            subject.UseHandle(
                async evnt =>
                {
                    try
                    {
                        var producer = producerFactory();
                        var kafkaEvent = new KafkaEvent { Tags = evnt.Tags, Content = JObject.Parse(evnt.Content) };
                        var strContent = await Task.Run(() => JsonConvert.SerializeObject(kafkaEvent));
                        await producer.ProduceAsync(topic, new Message<string, string> { Key = evnt.Name, Value = strContent });
                    }
                    catch (ProduceException<string, string> e)
                    {
                        Console.WriteLine($"Delivery to Kafka failed: {e.Error.Reason}");
                        throw e;
                    }
                });

            return subject;
        }
    }
}
