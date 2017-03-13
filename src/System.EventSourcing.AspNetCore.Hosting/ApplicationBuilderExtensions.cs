using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder DenyEventSourcing(this IApplicationBuilder subject)
        {
            subject.Use((x, n) =>
            {
                if (x.Request.Path.HasValue && x.Request.Path.Value.StartsWith("/v1/events", StringComparison.OrdinalIgnoreCase))
                {
                    x.Response.StatusCode = 404;
                    return Task.CompletedTask;
                }

                return n();
            });
            return subject;
        }
    }
}
