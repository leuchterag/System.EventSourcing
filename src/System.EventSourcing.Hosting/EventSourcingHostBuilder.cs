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
    }
}
