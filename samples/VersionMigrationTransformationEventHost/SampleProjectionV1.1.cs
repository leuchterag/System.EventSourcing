using System;
using System.EventSourcing;
using System.EventSourcing.Hosting;
using System.EventSourcing.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SimpleEventHost
{

    [Event("https://sample.service.com/v1.1/sample", "created")]
    public class SampleEventV11
    {
        public string Id { get; set; }
    }

    class SampleProjectionV11 : IProjection<SampleEventV11>
    {
        private readonly IEventContext context;
        private readonly ILogger<SampleProjectionV11> logger;

        public SampleProjectionV11(IEventContext context, ILogger<SampleProjectionV11> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task Handle(SampleEventV11 @event)
        {
            logger.LogInformation("captured event in projection:\n{event}\n{tags}\nId: {id}", @event, context.Tags, @event.Id);

            await Task.Delay(1000);

            logger.LogInformation("completed event {id}", @event.Id);
        }
    }
}
