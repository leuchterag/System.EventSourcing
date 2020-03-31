using System.EventSourcing.Reflection;
using System.Linq;

namespace System.EventSourcing.Hosting.Projections.Reflection
{
    public static class ProjectionHostBuilderExtensions
    {
        public static IProjectionHostBuilder<TContext> AttributeKeyResolution<TContext>(this IProjectionHostBuilder<TContext> projBuilder)
        {
            projBuilder.ProjectionExtractor = (projectionTypes) =>
            {
                return projectionTypes
                    .SelectMany(x => {
                        return x
                            .GetInterfaces()
                            .Where(y => y.IsInterface)
                            .Where(y => y.IsGenericType)
                            .Where(y => y.GetGenericTypeDefinition() == typeof(IProjection<>))
                            .Select(y => new {ProjectionType = x, EventType = y});
                    })
                    .Select(x => // Pull all attributes on all types
                    {
                        var eventType = x.EventType
                            .GetGenericArguments()
                            .First();
                        var attributes = eventType
                            .GetCustomAttributes(typeof(EventAttribute), true).Cast<EventAttribute>();
                        return new { Type = x.ProjectionType, EventType = eventType, Attributes = attributes };
                    })
                    .Where(x => x.Attributes.Any())
                    .SelectMany(x => x.Attributes.Select(y => new { x.Type, x.EventType, Attribute = y})) // fan-out the attributes to support one type having multiple attributes
                    .Select(x => new { x.Type, x.EventType, Name = $"{x.Attribute.Subject}.{x.Attribute.Action}" }) // format the action
                    .GroupBy(x => x.Name)
                    .Select(x => new ProjectionGroup
                    {
                        EventKey = x.Key,
                        Projections = x.Select(p => new ProjectionSet { EventType = p.EventType, ProjectionType = p.Type})
                    }); // Form a group to bundle all projections for the same event type
            };

            return projBuilder;
        }
    }
}