using System.Threading.Tasks;

namespace System.EventSourcing.Client
{
    public delegate Task EventHandle(Event evnt);
}
