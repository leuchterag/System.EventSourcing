using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.EventSourcing.Hosting
{
    class EventSourcingHostBuilder : IEventSourcingBuilder<IHostBuilder>
    {
        public Action<IEventSourcingBuilder<IServiceCollection>> ServiceBuilderSetup { get; set; }

        public IHostBuilder Base { get; set; }

        public IList<Action> BuildHooks { get; set; } = new List<Action>();

        public void OnBuild(Action onBuildHook)
        {
            BuildHooks.Add(onBuildHook);
        }

        // public IList<Type> Projections { get; set; } = new List<Type>();

        // public IList<Action<IEventSourcingBuilder<IHostBuilder>>> Setups { get; set; } = new List<Action<IEventSourcingBuilder<IHostBuilder>>>();
    }
}
