using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;

namespace System.EventSourcing.Hosting.Kafka
{
    public static class ServiceBuilderExtensions
    {
        public static IEventSourcingBuilder<IServiceCollection> FromKafka(this IEventSourcingBuilder<IServiceCollection> builder)
        {
            builder.Base.AddSingleton<IMessageHandler<string, byte[]>, EventMultiplexer>();

            return builder;
        }
    }
}
