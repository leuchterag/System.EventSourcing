using System.EventSourcing.Hosting.Middleware;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public static class MiddlewareAppBuilderExtensions
    {
        public static void UseTransformations<TContext, TBaseContextType>(
            this IMiddlewareAppBuilder<TContext, TBaseContextType> subject,
            Action<ITransformationMiddlewareBuilder<TContext>> setup)
            where TContext : IContext
        {
            var builder = new TransformationMiddlewareBuilder<TContext>();

            setup(builder);

            subject.Use(
                (ctx, next) => 
                {
                    var transforms = builder.Handlers.Select(x => x(ctx, next));
                    return Task.WhenAll(transforms);
                });
        }
    }
}
