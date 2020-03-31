using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.Hosting
{
    class ServiceEventSourcingBuilder : IEventSourcingBuilder<IServiceCollection>
    {
        private readonly IList<Action> buildHooks = new List<Action>();

        public IServiceCollection Base { get; set; }

        public IList<Type> Projections { get; set; } = new List<Type>();

        public void Configure(Action onBuildHook)
        {
            buildHooks.Add(onBuildHook);
        }
        
        public void Build() 
        {
            foreach (var hook in buildHooks)
            {
                hook();
            }
        }
    }
}
