using System;
using System.EventSourcing.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace SimpleEventHost
{
    public class TestProjection : Projection<Tf1>
    {
#pragma warning disable RECS0154 // This is a sample
        public TestProjection(IService svc)
#pragma warning restore RECS0154 // This is a sample
        {

        }

        public override string EventDescriptor => "User.Created";

        public override Task Handle(Tf1 @event)
        {
            if(@event.T1 == "throw")
            {
                throw new Exception("Requested Exception");
            }

            return Task.CompletedTask;
        }
    }
}
