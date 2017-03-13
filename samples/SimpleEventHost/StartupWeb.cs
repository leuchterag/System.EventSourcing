using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.EventSourcing.AspNetCore.Hosting;
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

            app.DenyEventSourcing();
            
            app.UseMvc();
        }
    }
}