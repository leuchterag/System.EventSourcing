using Microsoft.Extensions.Hosting.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.EventSourcing.Kafka;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Kafka
{
    class EventMultiplexer : IMessageHandler<string, byte[]>
    {
        IEnumerable<IEventProjection> Projections { get; }

        IDictionary<string, IEnumerable<IEventProjection>> GrouppedProjections { get; }

        IDictionary<string, IEnumerable<Func<JObject, Task>>> ProjectionHandlers { get; }

        public EventMultiplexer(IEnumerable<IEventProjection> projections)
        {
            Projections = projections;

            GrouppedProjections = projections
                .GroupBy(x => x.EventDescriptor)
                .ToDictionary(x => x.Key, x => x.Select(y => y));

            ProjectionHandlers = GrouppedProjections
                .ToDictionary(x => x.Key, x => x.Value.Select(handler => ExtractHandler(handler)));
        }

        public Task Handle(string name, byte[] payload)
        {
            var @event = JsonConvert.DeserializeObject<KafkaEvent>(Encoding.UTF8.GetString(payload));

            if (ProjectionHandlers.ContainsKey(name))
            {
                var handlerTasks = ProjectionHandlers[name]
                    .Select(x => x(@event.Content))
                    .ToArray();

                return Task.WhenAll(handlerTasks);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Extracts the generic type definition of the given projection and constructs a handler method which can be 
        /// </summary>
        /// <param name="projection">The projection for which to implement the </param>
        /// <returns>Function that takes JObjects returns the task handling the projection</returns>
        Func<JObject, Task> ExtractHandler(IEventProjection projection)
        {
            var projectionType = projection
                .GetType()
                .GetInterfaces()
                .First(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IProjection<>))
                .GenericTypeArguments
                .First();

            var handlerMethod = projection
                .GetType()
                .GetMethod(nameof(IProjection<object>.Handle));

            return (JObject obj) => 
            {
                var unmarshalledEvent = obj.ToObject(projectionType);
                return handlerMethod.Invoke(projection, new [] { unmarshalledEvent }) as Task;
            };
        }

    }
}