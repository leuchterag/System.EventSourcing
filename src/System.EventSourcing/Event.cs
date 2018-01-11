using System.Collections.Generic;

namespace System.EventSourcing
{
    public class Event
    {
        public string Name { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public string Content { get; set; }
    }
}
