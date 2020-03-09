using Newtonsoft.Json.Linq;

namespace System.EventSourcing.Hosting.Json
{
    public class StringJObjectContext : IContext
    {
        public string Key { get; set; }

        public JObject Content { get; set; }

        public IServiceProvider Services { get; set; }
    }
}