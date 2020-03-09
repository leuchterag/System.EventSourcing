using System.EventSourcing.Hosting.Middleware;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Projections
{
    public static class MiddlewareAppBuilderExtensions
    {
        public static IMiddlewareAppBuilder<TContext, TBaseContextType> UseProjections<TContext, TBaseContextType>(this IMiddlewareAppBuilder<TContext, TBaseContextType> app, Action<IProjectionHostBuilder<TContext>> factory)
            where TContext : IContext
        {
            var projBuilder = new ProjectionHostBuilder<TContext>();
            Func<TContext, Func<Task>, Task> middleware = 
                (ctx, next) =>
                {
                    throw new ArgumentNullException("There is no handler for the projection middleware. Did you forget to build the AppBuilder?");
                };

            factory(projBuilder);
            
            app.Use((ctx, next) => middleware(ctx, next));

            app.OnBuild(() => middleware = projBuilder.Build());

            return app;
        }
    }
}