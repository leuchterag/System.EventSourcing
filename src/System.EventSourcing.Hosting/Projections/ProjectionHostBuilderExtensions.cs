using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.Hosting.Projections
{
    public static class ProjectionHostBuilderExtensions
    {
        public static IProjectionHostBuilder<TContext> FromList<TContext>(this IProjectionHostBuilder<TContext> projBuilder, IEnumerable<Type> projections)
        {
            projBuilder.ProjectionTypeSources.Add(() => projections);
            return projBuilder;
        }

        public static IProjectionHostBuilder<TContext> FromServiceCollection<TContext>(this IProjectionHostBuilder<TContext> projBuilder, IServiceCollection serviceCollection)
        {
            projBuilder.ProjectionTypeSources.Add(
                () =>
                    serviceCollection
                        .Where(x => x
                            .ServiceType
                            .GetInterfaces()
                            .Any(y =>
                                y.IsGenericType &&
                                y.GetGenericTypeDefinition() == typeof(IProjection<>)))
                        .Select(x => x.ImplementationType));
            
            return projBuilder;
        }

        public static IProjectionHostBuilder<TContext> KeyFrom<TContext>(this IProjectionHostBuilder<TContext> projBuilder, Func<TContext, string> keyExtrator)
        {
            projBuilder.KeyExtractor = keyExtrator;
            return projBuilder;
        }
    }
}