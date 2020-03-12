using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    public interface IEventSourcingBuilder<THostType> : IConfigurable
    {
        THostType Base { get; set; }
    }
}