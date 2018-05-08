using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace System.EventSourcing.AspNetCore.Hosting
{

    [Route("/v1/events/")]
    public class EventsController : Controller
    {
        private readonly IEnumerable<AspNetProjection> _projections;

        public EventsController(IEnumerable<AspNetProjection> projections, IHttpContextFactory contextFactory)
        {
            _projections = projections;
        }

        [HttpPut("{encodedeventdescriptor}")]
        [DisableFormValueModelBinding]
        public async Task Test([FromRoute] string encodedeventdescriptor)
        {
            var eventdescriptor = HttpUtility.UrlDecode(encodedeventdescriptor);
            var candidates = _projections.Where(x => x.EventDescriptor.Equals(eventdescriptor, StringComparison.OrdinalIgnoreCase)).ToArray();
            await Task.WhenAll(candidates.Select(x => x.Handle(HttpContext)));
        }
    }
}
