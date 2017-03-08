using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.EventSourcing.AspNetCore.Hosting;

namespace SimpleEventHost
{
    public class Startup
    {
        readonly IRedirectionTarget _target;

        public Startup(IRedirectionTarget target)
        {
            _target = target;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddJsonFormatters();

            services.AddSingleton(x => _target.Provider.GetService<IService>());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            
            app.UseMvc();
        }
    }
}