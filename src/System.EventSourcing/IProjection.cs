using System.Threading.Tasks;

namespace System.EventSourcing
{
    public interface IProjection<TEvent> : IEventProjection
    {
        Task Handle(TEvent @event);
    }
}
