using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace System.EventSourcing.Hosting
{
    public delegate Task MessageHandler<TKey, TContent>(TKey name, TContent content);
}
