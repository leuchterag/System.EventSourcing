using System;
using System.EventSourcing;
using System.Threading.Tasks;

namespace SimpleEventHost
{
    public class SampleEvent
    {
        public string Id { get; set; }
    }

    class SampleProjection : IProjection<SampleEvent>
    {
        public string EventDescriptor => "sample.service.com/sample.created";

        public Task Handle(SampleEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
