using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public abstract class AspNetProjection : IEventProjection
    {
        public abstract string EventDescriptor { get; }

        public abstract Task Handle(HttpContext ctx);
    }
}
