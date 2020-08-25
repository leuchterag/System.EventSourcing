using System.EventSourcing.Hosting.Middleware;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                async (ctx, next) => 
                {
                    var logger = ctx.Services.GetService<ILogger<TransformationMiddlewareBuilder<TContext>>>();
                    var transforms = builder.Handlers
                        .Select(async x => await x(ctx))
                        .ToArray();

                    await Task.WhenAll(transforms);

                    var transformedContexts = transforms
                        .Where(x => !x.IsFaulted && !x.IsCanceled && x.IsCompleted)
                        .Select(x => x.Result)
                        .ToArray();
                    var failedTranformations = transforms
                        .Where(x => x.IsFaulted || x.IsCanceled)
                        .Count();
                    if (failedTranformations != 0)
                    {
                        logger.LogWarning("Failed to apply {failed} of {allTranformations}", failedTranformations, transformedContexts.Count());
                    }

                    if (!transformedContexts.Any(x => x.transformApplies)
                        || transformedContexts.Any(x => x.considerOrigin))
                    {
                        await next(ctx);
                    }

                    var transformedContextHandlers = transformedContexts
                        .Where(x => x.transformApplies)
                        .Select(async x => await next(x.transformed))
                        .ToArray();

                    await Task.WhenAll(transformedContextHandlers);
                });
        }
    }
}