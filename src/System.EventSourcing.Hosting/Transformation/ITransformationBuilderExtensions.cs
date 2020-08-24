using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public static class ITransformationBuilderExtensions
    {
        public static ITransformationBuilder<TContext> KeysMatching<TContext>(
            this ITransformationMiddlewareBuilder<TContext> middlewareBuilder,
            Func<TContext, bool> condition)
            where TContext : IContext
        {
            var builder = new TransformationBuilder<TContext>();
            builder.Base = middlewareBuilder;
            builder.Condition = condition;

            return builder;
        }

        public static ITransformationBuilder<TContext> Transform<TContext>(
            this ITransformationBuilder<TContext> transformationBuilder,
            Func<TContext, TContext, Task> transformation)
            where TContext : IContext, new()
        {
            Func<TContext, Task<TContext>> transformationFactory =
                async (TContext origin) =>
                {
                    var transformedContext = new TContext();
                    transformedContext.Services = origin.Services;

                    await transformation(origin, transformedContext);

                    return transformedContext;
                };

            transformationBuilder.Transformation = transformationFactory;

            return transformationBuilder;
        }

        
        public static ITransformationBuilder<TContext> RetainOrigin<TContext>(
            this ITransformationBuilder<TContext> transformationBuilder)
            where TContext : IContext, new()
        {
            transformationBuilder.Base.Handlers.Add(
                async origin =>
                {
                    if (transformationBuilder.Condition(origin))
                    {
                        var transformed = await transformationBuilder.Transformation(origin);

                        return (true, true, transformed);
                    }
                    
                    return (true, false, default(TContext));
                });

            return transformationBuilder;
        }
        
        public static ITransformationBuilder<TContext> DropOrigin<TContext>(
            this ITransformationBuilder<TContext> transformationBuilder)
            where TContext : IContext, new()
        {
            transformationBuilder.Base.Handlers.Add(async origin =>
            {
                if (transformationBuilder.Condition(origin))
                {
                    var transformed = await transformationBuilder.Transformation(origin);

                    return(false, true, transformed);
                }

                return (false, false, default(TContext));
            });

            return transformationBuilder;
        }
    }
}