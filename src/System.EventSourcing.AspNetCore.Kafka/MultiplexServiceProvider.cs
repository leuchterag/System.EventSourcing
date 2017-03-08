using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public class MultiplexServiceProvider : IServiceProvider
    {
        readonly IServiceProvider primary;
        readonly IServiceProvider secondary;

        public MultiplexServiceProvider(IServiceProvider primary, IServiceProvider secondary)
        {
            this.secondary = secondary;
            this.primary = primary;
        }

        public object GetService(Type serviceType)
        {
            if(typeof(IServiceScopeFactory).GetTypeInfo().IsAssignableFrom(serviceType))
            {
                return new MultiplexServiceScopeFactory((IServiceScopeFactory)primary.GetService(serviceType), (IServiceScopeFactory)secondary.GetService(serviceType));
            }


            var svc = primary.GetService(serviceType);
            if(svc == null)
            {
                return secondary.GetService(serviceType);
            }

            return svc;
        }
    }
}
