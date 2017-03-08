using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.EventSourcing.AspNetCore.Hosting;

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

        public static IWebHostBuilder EnableRedirection(this IWebHostBuilder hostBuilder, IServiceProvider provider)
        {
            hostBuilder.ConfigureServices(svcs =>
            {
                svcs.AddSingleton<IRedirectionTarget>(new ServiceRedirectionTarget(provider));
            });
            return hostBuilder;
        }
    }
}
