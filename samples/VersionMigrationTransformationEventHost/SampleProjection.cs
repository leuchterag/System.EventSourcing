using System;
using System.EventSourcing;
using System.EventSourcing.Hosting;
using System.EventSourcing.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SimpleEventHost
{

    [Event("https://sample.service.com/sample", "created")]
    public class SampleEvent
    {
        public string Id { get; set; }
    }

    class SampleProjection : IProjection<SampleEvent>
    {
        private readonly IEventContext context;
        private readonly ILogger<SampleProjection> logger;

        public SampleProjection(IEventContext context, ILogger<SampleProjection> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Task Handle(SampleEvent @event)
        {
            logger.LogInformation("captured event in projection:\n{event}\n{tags}\nId: {id}", @event, context.Tags, @event.Id);

            return Task.CompletedTask;
        }
    }
}
