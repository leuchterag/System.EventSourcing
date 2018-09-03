using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.EventSourcing.Hosting;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddEventSourcing(this IHostBuilder builder, Action<IEventSourcingBuilder<IServiceCollection>> projectionSetup)
        {
            return new EventSourcingHostBuilder
            {
                ServiceBuilderSetup = projectionSetup,
                Base = builder
            }
            .Build();            
        }


        static IHostBuilder Build(this EventSourcingHostBuilder builder)
        {
            builder.Base.ConfigureServices((ctx,services) =>
            {
                var serviceBuilder = new ServiceEventSourcingBuilder
                {
                    Base = services
                };

                builder.ServiceBuilderSetup(serviceBuilder);

                serviceBuilder.Build();
            });

            return builder.Base;
        }
    }
}
