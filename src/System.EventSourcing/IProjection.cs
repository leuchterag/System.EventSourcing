using System.Threading.Tasks;

namespace System.EventSourcing
{
    public interface IProjection<TEvent>
    {
        Task Handle(TEvent @event);
    }
}
