using System;
using System.Collections.Generic;
using System.EventSourcing.AspNetCore.Hosting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CustomRequestFactory
{
    class CompilationJobCompleted : Projection<object>
    {

        public override string EventDescriptor => "docugate.io/v1alpha1/compiler/job.completed";

        public override Task Handle(HttpContext ctx)
        {
            return Task.CompletedTask;
        }

        public override Task Handle(object @event)
        {
            // no implementation needed since Handle(HttpContext) will not call it!
            return Task.CompletedTask;
        }
    }
}
