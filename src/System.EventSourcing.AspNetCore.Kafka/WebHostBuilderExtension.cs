using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder UseKafka(this IWebHostBuilder subject, Action<KafkaListenerSettings> setup)
        {
            subject.ConfigureServices(svc =>
            {
                svc.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                svc.AddTransient<IHttpContextFactory, HttpContextFactory>();
                svc.Configure(setup);
                svc.AddSingleton<IServer, KafkaListener>();
            });

            return subject;
        }

        public static IWebHostBuilder UseStartup<TStartup>(
            this IWebHostBuilder hostBuilder,
            Func<IServiceCollection, IServiceProvider> configureServicesDelegate,
            Action<IApplicationBuilder> configureDelegate)
            where TStartup : class, new()
        {
            var startupType = typeof(TStartup);
            var startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;

            return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName)
                              .ConfigureServices(services =>
                              {
                                  if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
                                  {
                                      services.AddSingleton(typeof(IStartup), startupType);
                                  }
                                  else
                                  {
                                      services.AddSingleton(typeof(IStartup), sp =>
                                      {
                                          var hostingEnvironment = sp.GetRequiredService<IHostingEnvironment>();

                                          var typedStartup = StartupLoader.LoadMethods(sp, startupType, hostingEnvironment.EnvironmentName);

                                          var combinedStartups = new StartupMethods(
                                              appBuilder =>
                                              {
                                                  configureDelegate(appBuilder);
                                                  typedStartup.ConfigureDelegate(appBuilder);
                                              },
                                              col =>
                                              {
                                                  return new MultiplexServiceProvider(typedStartup.ConfigureServicesDelegate(col), configureServicesDelegate(col));
                                              });

                                          return new ConventionBasedStartup(combinedStartups);
                                      });
                                  }
                              });
            return hostBuilder;
        }
    }
}
