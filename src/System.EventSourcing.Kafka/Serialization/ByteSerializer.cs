using Confluent.Kafka.Serialization;

namespace System.EventSourcing.Kafka.Serialization
{
    public class ByteSerializer : ISerializer<byte[]>
    {
        public byte[] Serialize(byte[] data)
        {
            return data;
        }
    }
}
