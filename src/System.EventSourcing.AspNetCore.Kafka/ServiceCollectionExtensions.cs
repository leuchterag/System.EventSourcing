using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.EventSourcing.Kafka;
using System.IO;
using System.Text;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public delegate HttpRequestFeature RequestFactory(Message<string, byte[]> msg, string subject, string action);

    public static class ServiceCollectionExtensions
    {
        private static HttpRequestFeature DefaultRequestFactory(Message<string, byte[]> msg, string subject, string action)
        {
            var evnt = JsonConvert.DeserializeObject<KafkaEvent>(Encoding.UTF8.GetString(msg.Value));

            var headers = new HeaderDictionary { { "Content-Type", "application/json" } };

            foreach (var tag in evnt.Tags)
            {
                headers.Add(tag.Key, tag.Value);
            }

            var body = Encoding.UTF8.GetBytes(evnt.Content.ToString());

            var requestFeature = new HttpRequestFeature
            {
                Method = "PUT",
                Path = $"/v1/events/{subject}.{action}",
                Body = new MemoryStream(body),
                Protocol = "http",
                Scheme = "http",
                Headers = headers
            };
            return requestFeature;
        }

        public static IServiceCollection UseKafka(this IServiceCollection subject, Action<KafkaListenerSettings> setup)
        {
            return subject.UseKafka(setup, DefaultRequestFactory);
        }

        public static IServiceCollection UseKafka(this IServiceCollection subject, Action<KafkaListenerSettings> setup, RequestFactory customRequestFactory)
        {
            subject.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            subject.AddTransient<IHttpContextFactory, HttpContextFactory>();
            subject.Configure(setup);
            subject.AddSingleton<IServer, KafkaListener>(di =>
            {
                var listener = new KafkaListener(
                    di.GetRequiredService<IServiceScopeFactory>(),
                    di.GetRequiredService<IOptions<KafkaListenerSettings>>(),
                    di.GetRequiredService<ILogger<KafkaListener>>());

                listener.RequestFactory = customRequestFactory;

                return listener;
            });

            return subject;
        }
    }
}
