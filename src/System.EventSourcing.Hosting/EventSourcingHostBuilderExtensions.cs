using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;
using Newtonsoft.Json.Linq;
using System.EventSourcing.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting
{
    public static class EventSourcingHostBuilderExtensions
    {
        public static IEventSourcingBuilder<IServiceCollection> AddProjection<TProjection>(this IEventSourcingBuilder<IServiceCollection> builder)
        {
            var isProjection = typeof(TProjection)
                .GetInterfaces()
                .Any(x =>
                  x.IsGenericType &&
                  x.GetGenericTypeDefinition() == typeof(IProjection<>));

            if (!isProjection)
            {
                throw new ArgumentException($"Type {typeof(TProjection).Name} cannot be registered because it does not implement IProjection<TEvent>");
            }

            builder.Base.AddTransient(typeof(TProjection));
            return builder;
        }
    }
}
