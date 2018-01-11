using Newtonsoft.Json;
using System.EventSourcing.Client.Reflection;

namespace SimpleEventHost
{
    [Event("user", "created")]
    public class TEvent
    {
        [JsonProperty("T1")]
        public string T { get; set; }
    }
}