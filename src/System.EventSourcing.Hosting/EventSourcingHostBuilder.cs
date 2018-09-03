using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace System.EventSourcing.Hosting
{
    class EventSourcingHostBuilder : IEventSourcingBuilder<IHostBuilder>
    {
        public Action<IEventSourcingBuilder<IServiceCollection>> ServiceBuilderSetup { get; set; }

        public IHostBuilder Base { get; set; }

        public IList<Type> Projections { get; set; } = new List<Type>();
    }
}
