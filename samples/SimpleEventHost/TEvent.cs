using System.EventSourcing.Client.Reflection;

namespace SimpleEventHost
{
    [Event("user", "created")]
    public class TEvent
    {
        public string T { get; set; }
    }
}