using System;
using System.Threading.Tasks;

namespace System.EventSourcing.Client
{
    public interface IEventClient
    {
        Task Publish<TEvent>(TEvent evnt);
    }
}
