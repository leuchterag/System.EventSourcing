using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;
using Newtonsoft.Json.Linq;
using System.EventSourcing.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Reflection
{
    public static class ReflectionHostBuilderExtensions
    {

        /// <summary>
        /// Uses the Event attribute on the registered projections' event types for name inference and dispatching. 
        /// </summary>
        /// <param name="builder">the builder to apply to</param>
        /// <returns>the extended builder</returns>
        public static IEventSourcingBuilder<IServiceCollection> UseReflectionResolution(this IEventSourcingBuilder<IServiceCollection> builder)
        {
            builder.Setups.Add(BuildReflectionHandler);
            return builder;
        }

        private static void BuildReflectionHandler(IEventSourcingBuilder<IServiceCollection> builder)
        {
            var grouppedProjections = builder
                .Projections
                .SelectMany(x => {
                    return x
                        .GetInterfaces()
                        .Where(y => y.IsInterface)
                        .Where(y => y.IsGenericType)
                        .Where(y => y.GetGenericTypeDefinition() == typeof(IProjection<>))
                        .Select(y => new {ProjectionType = x, EventType = y});
                })
                .Select(x =>  // Pull all attributes on all types
                {
                    var attributes = x.EventType
                        .GetGenericArguments()
                        .First()
                        .GetCustomAttributes(typeof(EventAttribute), true).Cast<EventAttribute>();
                    return new { Type = x.ProjectionType, Attributes = attributes };
                })
                .Where(x => x.Attributes.Any())
                .SelectMany(x => x.Attributes.Select(y => new { x.Type, Attribute = y})) // fan-out the attributes to support one type having multiple attributes
                .Select(x => new { x.Type, Name = $"{x.Attribute.Subject}.{x.Attribute.Action}"}) // format the action
                .Select(x => new { x.Name, Handler = ExtractHandler(x.Type), Type = x.Type }) // and extract the handler for the action
                .GroupBy(x => x.Name); // Form a group to bundle all projections for the same event type

            builder.Base.AddScoped(sp => 
            {
                MessageHandler handler = (name, obj) => {
                    var projectionHandlerTasks = grouppedProjections
                        .Where(x => x.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        .SelectMany( x => x)
                        .Select(x => x.Handler(sp.GetService(x.Type), obj))
                        .ToArray();

                    return Task.WhenAll(projectionHandlerTasks);
                };
                return handler;
            });

            foreach (var projection in builder.Projections)
            {
                builder.Base.AddTransient(projection);
            }
        }

        /// <summary>
        /// Extracts the generic type definition of the given projection and constructs a handler method which can be invoked. 
        /// </summary>
        /// <param name="projection">The projection for which to implement the handler</param>
        /// <returns>Function that takes JObjects returns the task handling the projection</returns>
        private static Func<object, JObject, Task> ExtractHandler(Type projection)
        {
            var projectionType = projection
                .GetInterfaces()
                .First(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IProjection<>))
                .GenericTypeArguments
                .First(); // We can use First() since these preconditions are ensured by EventSourcingHostBuilderExtensions.AddProjection()

            var handlerMethod = projection
                .GetMethod(nameof(IProjection<object>.Handle));

            return (object subject, JObject content) => 
            {
                var unmarshalledEvent = content.ToObject(projectionType);
                return handlerMethod.Invoke(subject, new [] { unmarshalledEvent }) as Task;
            };
        }
    }
}
