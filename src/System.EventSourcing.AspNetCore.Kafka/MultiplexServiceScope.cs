using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public class MultiplexServiceScope : IServiceScope
    {
        readonly IServiceScope primary;
        readonly IServiceScope secondary;

        public MultiplexServiceScope(IServiceScope primary, IServiceScope secondary)
        {
            this.secondary = secondary;
            this.primary = primary;

            ServiceProvider = new MultiplexServiceProvider(primary.ServiceProvider, secondary.ServiceProvider);
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public void Dispose()
        {

        }
    }
}
