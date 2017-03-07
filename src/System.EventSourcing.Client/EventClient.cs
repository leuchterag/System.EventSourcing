using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Client
{
    public class EventClient : IEventClient
    {
        IList<EventMiddleware> Parsers { get; set; } = new List<EventMiddleware>();

        IList<EventHandle> Handlers { get; set; } = new List<EventHandle>();

        public void UseHandle(EventHandle handle)
        {
            Handlers.Add(handle);
        }
        
        public void UseParser(EventMiddleware parser)
        {
            Parsers.Add(parser);
        }

        public async Task Publish<TEvent>(TEvent evnt)
        {
            var newEvent = new Event();
            var originalType = typeof(TEvent);

            foreach (var parser in Parsers)
            {
                await parser(evnt, originalType, newEvent);
            }

            foreach (var handler in Handlers)
            {
                await handler(newEvent);
            }
        }
    }
}
