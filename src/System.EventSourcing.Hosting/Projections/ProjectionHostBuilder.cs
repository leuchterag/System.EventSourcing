using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Projections
{
    public class ProjectionHostBuilder<TContext> : IProjectionHostBuilder<TContext>
    {
        public IList<Func<IEnumerable<Type>>> ProjectionTypeSources { get; set; } = new List<Func<IEnumerable<Type>>>();
        
        public Func<IEnumerable<Type>, IEnumerable<ProjectionGroup>> ProjectionExtractor { get; set; } = (t) => Enumerable.Empty<ProjectionGroup>();
        
        public Func<ProjectionSet, Func<TContext, Task>> InvocatorFactory { get; set; }
        
        public Func<TContext, string> KeyExtractor { get; set; }

        public Func<TContext, Func<Task>, Task> Build()
        {
            var projectionTypes = ProjectionTypeSources.SelectMany(x => x());
            var projectionGroups = ProjectionExtractor(projectionTypes);

            var handlers = projectionGroups
                .Select(
                    x =>
                    {
                        var projectionHandlers = x.Projections.Select(y => InvocatorFactory(y)).ToArray();
                        Func<TContext, Task> handler = (TContext ctx) => Task.WhenAll(projectionHandlers.Select(y => y(ctx)));
                        return new {
                            Key = x.EventKey,
                            Handler = handler
                        };
                    }
                );
            
            var handlerDictionary = handlers.ToDictionary(x => x.Key, x => x.Handler);

            return async (ctx, next) =>
            {
                var key = KeyExtractor(ctx);
                if (handlerDictionary.ContainsKey(key))
                {
                    await handlerDictionary[key](ctx);
                }

                await next();
            };
        }
    }
}
