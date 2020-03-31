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
            Action<TContext, TContext> transformation)
            where TContext : IContext, new()
        {
            Func<TContext, TContext> transformationFactory = 
                (TContext origin) => 
                {
                    var transformedContext = new TContext();
                    transformedContext.Services = origin.Services;

                    transformation(origin, transformedContext);

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
                async (origin, next) =>
                {
                    await next(origin);
                    
                    if (transformationBuilder.Condition(origin))
                    {
                        var transformed = transformationBuilder.Transformation(origin);

                        await next(transformed);    
                    }
                });

            return transformationBuilder;
        }
        
        public static ITransformationBuilder<TContext> DropOrigin<TContext>(
            this ITransformationBuilder<TContext> transformationBuilder)
            where TContext : IContext, new()
        {
            transformationBuilder.Base.Handlers.Add(async (origin, next) =>
            {
                if (transformationBuilder.Condition(origin))
                {
                    var transformed = transformationBuilder.Transformation(origin);

                    await next(transformed);
                }
            });

            return transformationBuilder;
        }
        
        public static void HandleUntransformed<TContext>(
            this ITransformationMiddlewareBuilder<TContext> middlewareBuilder)
            where TContext : IContext, new()
        {
            middlewareBuilder.Handlers.Add(async (origin, next) =>
            {
                await next(origin);
            });
        }
    }
}