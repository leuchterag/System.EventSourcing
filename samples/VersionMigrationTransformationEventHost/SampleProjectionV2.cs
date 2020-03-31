using System;
using System.EventSourcing;
using System.EventSourcing.Hosting;
using System.EventSourcing.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SimpleEventHost
{

    [Event("https://sample.service.com/v2/sample", "created")]
    public class SampleEventV2
    {
        public string Id { get; set; }
    }

    class SampleProjectionV2 : IProjection<SampleEventV2>
    {
        private readonly IEventContext context;
        private readonly ILogger<SampleProjection> logger;

        public SampleProjectionV2(IEventContext context, ILogger<SampleProjection> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Task Handle(SampleEventV2 @event)
        {
            logger.LogInformation("captured event in projection v2:\n{event}\n{tags}\nId: {id}", @event, context.Tags, @event.Id);

            return Task.CompletedTask;
        }
    }
}
