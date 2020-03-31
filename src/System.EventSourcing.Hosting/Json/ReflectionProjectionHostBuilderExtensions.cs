using System.EventSourcing.Hosting.Middleware;
using System.EventSourcing.Hosting.Projections;
using System.EventSourcing.Hosting.Projections.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace System.EventSourcing.Hosting.Json
{
    public static class ProjectionHostBuilderExtensions
    {
        public static IProjectionHostBuilder<TContext> ContentFromJObject<TContext>(this IProjectionHostBuilder<TContext> projBuilder)
            where TContext : StringJObjectContext
        {
            projBuilder.InvocatorFactory = (projSet) =>
            {
                var handlerMethod = projSet.ProjectionType
                    .GetMethod(nameof(IProjection<object>.Handle));

                return (ctx) => 
                {
                    var unmarshalledEvent = ctx.Content.ToObject(projSet.EventType);
                    var projection = ctx.Services.GetService(projSet.ProjectionType);
                    return handlerMethod.Invoke(projection, new [] { unmarshalledEvent }) as Task;
                };
            };

            return projBuilder;
        }

        public static IEventSourcingBuilder<IServiceCollection> UseProjections(
            this IEventSourcingBuilder<IServiceCollection> builder)
        {
            builder.Handle<string, JObject, StringJObjectContext>(
                (k, v) => new StringJObjectContext { Content = v, Key = k },
                (app) =>
                {
                    app
                        .UseProjections(projApp =>
                        {
                            projApp.FromServiceCollection(app.Base.Base);
                            projApp.AttributeKeyResolution();
                            projApp.KeyFrom(x => x.Key);
                            projApp.ContentFromJObject();
                        });
                });
            return builder;
        }
    }
}