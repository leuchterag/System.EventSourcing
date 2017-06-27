using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public abstract class Projection<TEvent> : AspNetProjection, IProjection<TEvent>
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public override Task Handle(HttpContext ctx)
        {
            var payload = DeserializeFromStream(ctx.Request.Body);

            return Handle(payload);
        }

        public static TEvent DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<TEvent>(jsonTextReader);
            }
        }

        public abstract Task Handle(TEvent @event);
    }
}
