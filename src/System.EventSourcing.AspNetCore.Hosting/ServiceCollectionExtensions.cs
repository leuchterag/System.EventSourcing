using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseEventSourcing(this IServiceCollection subject)
        {
            subject.AddSingleton<IControllerFactory, EventControllerFactory>();
            subject.AddTransient<EventsController>();
            return subject;
        }

        public static IServiceCollection WithProjection<TProjection>(this IServiceCollection subject)
            where TProjection : AspNetProjection
        {
            subject.AddTransient<AspNetProjection, TProjection>();
            return subject;
        }

    }
}
