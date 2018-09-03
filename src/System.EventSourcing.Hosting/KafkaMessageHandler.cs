using System.Threading.Tasks;
using Microsoft.Extensions.Hosting.Kafka;

namespace System.EventSourcing.Hosting
{
    class KafkaMessageHandler : IMessageHandler<string, byte[]>
    {
        public Task Handle(string key, byte[] value)
        {
            throw new NotImplementedException();
        }
    }
}
