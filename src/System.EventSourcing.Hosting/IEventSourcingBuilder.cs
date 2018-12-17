using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    public interface IEventSourcingBuilder<THostType>
    {
        THostType Base { get; set; }

        IList<Type> Projections { get; set; }

        IList<Action<IEventSourcingBuilder<THostType>>> Setups { get; set; }
    }
}