using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;
using System.Linq;

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

            builder.Projections.Add(typeof(TProjection));
            return builder;
        }


        public static IEventSourcingBuilder<IServiceCollection> Build(this IEventSourcingBuilder<IServiceCollection> builder) 
        {
            foreach (var projectionType in builder.Projections)
            {
                builder.Base.AddScoped(typeof(IEventProjection), projectionType);
            }

            return builder;
        }
    }
}
