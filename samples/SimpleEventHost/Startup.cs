using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.EventSourcing.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Kafka;

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

            // Add Kafka server and settings
            services.UseKafka(
                x =>
                {
                    x.BootstrapServers = new[] { "localhost:9092" };
                    x.Topics = new[] { "system.events" };
                    x.ConsumerGroup = "services.sample";
                });

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