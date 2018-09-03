using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    class RawEvent
    {
        public string Name { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public JObject Payload { get; set; }
    }
}
