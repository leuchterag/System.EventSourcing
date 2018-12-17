using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    public interface IEventContext
    {
        IDictionary<string, string> Tags { get; }
    }
}