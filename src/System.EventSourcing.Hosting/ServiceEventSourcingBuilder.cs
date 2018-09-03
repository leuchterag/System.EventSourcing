using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.Hosting
{
    class ServiceEventSourcingBuilder : IEventSourcingBuilder<IServiceCollection>
    {
        public IServiceCollection Base { get; set; }

        public IList<Type> Projections { get; set; } = new List<Type>();
    }
}
