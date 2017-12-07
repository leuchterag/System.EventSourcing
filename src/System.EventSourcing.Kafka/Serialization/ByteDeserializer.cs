using System.Collections.Generic;
using Confluent.Kafka.Serialization;

namespace System.EventSourcing.Kafka.Serialization
{
    public class ByteDeserializer : IDeserializer<byte[]>
    {
        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public byte[] Deserialize(string topic, byte[] data)
        {
            return data;
        }
    }
}
