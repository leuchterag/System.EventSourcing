using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.EventSourcing;
using System.EventSourcing.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Hosting.Authorization;
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
                .AddAuthorization()
                .AddJsonFormatters();

            // Add Kafka server and settings
            services.UseKafka(
                x =>
                {
                    x.BootstrapServers = new[] { "localhost:9092" };
                    x.Topics = new[] { "system.events" };
                    x.ConsumerGroup = "services.sample1";
                    x.DefaultTopicConfig = new Dictionary<string, object>()
                    {
                        { "auto.offset.reset", "smallest" }
                    };
                });

            services.AddSingleton(x => _target.Provider.GetService<IService>());

            services.UseEventSourcing()
                .WithProjection<TestProjection>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            
            //app.UseImpersonationBearer("http://localhost:6001", "api.client", "secret");

            //app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            //{
            //    Authority = "http://localhost:6001",
            //    RequireHttpsMetadata = false,
            //    EnableCaching = false,
            //    ApiName = "api",
            //    ApiSecret = "secret"
            //});

            app.UseMvc();
        }
    }
}