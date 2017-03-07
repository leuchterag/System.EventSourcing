using System.Threading.Tasks;

namespace System.EventSourcing.Client
{
    public delegate Task EventMiddleware(object evnt, Type type, Event newEvent);
}
