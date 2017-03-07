using Confluent.Kafka.Serialization;

namespace System.EventSourcing.Kafka.Serialization
{
    public class ByteDeserializer : IDeserializer<byte[]>
    {
        public byte[] Deserialize(byte[] data)
        {
            return data;
        }
    }
}
