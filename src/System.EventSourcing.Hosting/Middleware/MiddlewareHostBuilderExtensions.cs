using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.Hosting.Middleware
{
    public static class MiddlewareHostBuilderExtensions
    {
        public static IEventSourcingBuilder<IServiceCollection> Handle<TKey, TContent, TContext>(
            this IEventSourcingBuilder<IServiceCollection> builder,
            Func<TKey, TContent, TContext> transform,
            Action<IMiddlewareAppBuilder<TContext, IServiceCollection>> middlewareFactory)
            where TContext : IContext
        {
            builder.Configure(
                () => {
                    var appBuilder = new MiddlewareAppBuilder<TContext, IServiceCollection>{ Base = builder};
                    middlewareFactory(appBuilder);

                    var middleware = appBuilder.Build();

                    builder.Base.AddScoped(sp => 
                    {
                        MessageHandler<TKey, TContent> handler = async (key, content) =>
                        {
                            var context = transform(key, content);
                            context.Services = sp;
                            await middleware(context);
                        };
                        return handler;
                    });
                }
            );
            
            return builder;
        }
    }
}
