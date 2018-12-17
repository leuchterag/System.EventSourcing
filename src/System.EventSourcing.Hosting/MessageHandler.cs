using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace System.EventSourcing.Hosting
{
    public delegate Task MessageHandler(string name, JObject content);
}
