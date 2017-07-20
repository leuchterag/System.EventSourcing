using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public abstract class Projection<TEvent> : AspNetProjection, IProjection<TEvent>
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public override async Task Handle(HttpContext ctx)
        {
            var payload = await DeserializeFromStream(ctx.Request.Body);
            await Handle(payload);
        }

        public static async Task<TEvent> DeserializeFromStream(Stream stream)
        {
            using (var memorySteam = new MemoryStream())
            {
                lock(stream)
                {
                    var position = stream.Position;

                    stream.CopyTo(memorySteam);

                    stream.Seek(position, SeekOrigin.Begin);

                    var serializer = new JsonSerializer();

                    using (var sr = new StreamReader(memorySteam))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return serializer.Deserialize<TEvent>(jsonTextReader);
                    }
                }
            }

        }

        public abstract Task Handle(TEvent @event);
    }
}
