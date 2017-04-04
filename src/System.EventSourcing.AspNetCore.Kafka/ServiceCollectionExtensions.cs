using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseKafka(this IServiceCollection subject, Action<KafkaListenerSettings> setup)
        {
            subject.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            subject.AddTransient<IHttpContextFactory, HttpContextFactory>();
            subject.Configure(setup);
            subject.AddSingleton<IServer, KafkaListener>();

            return subject;
        }
    }
}
