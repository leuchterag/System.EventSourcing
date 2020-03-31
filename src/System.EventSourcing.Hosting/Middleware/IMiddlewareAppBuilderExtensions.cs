using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Middleware
{
    public static class IMiddlewareAppBuilderExtensions
    {
        public static void Use<TContext, TBaseContextType>(this IMiddlewareAppBuilder<TContext, TBaseContextType> subject, Func<TContext, Func<Task>, Task> middleware)
            where TContext : IContext
        {
            subject.Use((ctx, next) => middleware(ctx, () => next(ctx)));
        }
    }
}
