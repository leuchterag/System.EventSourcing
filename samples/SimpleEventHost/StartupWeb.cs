using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SimpleEventHost
{
    public class StartupWeb
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddJsonFormatters();
            services.AddSingleton<IService, DummyService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.Use((x, n) =>
            {
                if (x.Request.Path.HasValue && !x.Request.QueryString.Value.StartsWith("v1/events/", System.StringComparison.OrdinalIgnoreCase))
                {
                    x.Response.StatusCode = 404;
                    return Task.CompletedTask;
                }

                return n();
            });
            
            app.UseMvc();
        }
    }
}