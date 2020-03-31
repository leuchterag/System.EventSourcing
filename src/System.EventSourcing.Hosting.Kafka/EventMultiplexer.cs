using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.EventSourcing.Kafka;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Kafka
{
    class EventMultiplexer : IMessageHandler<string, byte[]>
    {
        private readonly IServiceProvider serviceProvider;

        public EventMultiplexer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task Handle(string name, byte[] payload)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var @event = JsonConvert.DeserializeObject<KafkaEvent>(Encoding.UTF8.GetString(payload));

                
                if (@event.Tags != null && @event.Tags.Any())
                {
                    var context = scope.ServiceProvider.GetService<IEventContext>();

                    foreach (var tag in @event.Tags)
                    {
                        context.Tags.Add(tag.Key, tag.Value);
                    }
                }

                var handler = scope.ServiceProvider.GetService<MessageHandler<string, JObject>>();
                return handler(name, @event.Content);
            }
        }
    }
}