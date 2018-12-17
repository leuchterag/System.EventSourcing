using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    class EventContext : IEventContext
    {
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}