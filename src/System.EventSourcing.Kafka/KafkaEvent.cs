using Newtonsoft.Json;
using System.Collections.Generic;

namespace System.EventSourcing.Kafka
{
    public class KafkaEvent
    {
        [JsonProperty(PropertyName = "tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty(PropertyName = "cnt")]
        public byte[] Content { get; set; }
    }
}
