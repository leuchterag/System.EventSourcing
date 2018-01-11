using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Serialization
{
    public static class JsonSerializationExtension
    {
        public static EventClient UseJsonSerialization(this EventClient subject)
        {
            var serializer = new JsonSerializer();
            subject.UseParser(
                async (evt, type, evnt) =>
                {
                    using (var memstream = new MemoryStream())
                    {
                        string strContent = string.Empty;
                        await Task.Run(() => strContent = JsonConvert.SerializeObject(evt));
                        evnt.Content = strContent;
                    }
                });

            return subject;
        }
    }
}
