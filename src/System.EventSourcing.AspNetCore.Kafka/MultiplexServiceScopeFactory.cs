using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public class MultiplexServiceScopeFactory : IServiceScopeFactory
    {
        readonly IServiceScopeFactory primary;
        readonly IServiceScopeFactory secondary;

        public MultiplexServiceScopeFactory(IServiceScopeFactory primary, IServiceScopeFactory secondary)
        {
            this.secondary = secondary;
            this.primary = primary;
        }

        public IServiceScope CreateScope()
        {
            return new MultiplexServiceScope(primary.CreateScope(), secondary.CreateScope());
        }
    }
}
