using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.EventSourcing;
using System.EventSourcing.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Hosting.Authorization;
using System.EventSourcing.Client;
using System.EventSourcing.Client.Authorization;
using System.EventSourcing.Client.Kafka;
using System.EventSourcing.Client.Reflection;
using System.EventSourcing.Client.Serialization;
using System.Threading.Tasks;

namespace SimpleEventHost
{
    public class StartupWeb
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();
            services.AddSingleton<IService, DummyService>();
            
            services.AddScoped<IEventClient>(x => new EventClient()
                .UseKafka("demo.events", "localhost:9092")
                .UseReflectionNameResolution()
                .UseJsonSerialization()
                .UseAuthorizationForwarding(x));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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