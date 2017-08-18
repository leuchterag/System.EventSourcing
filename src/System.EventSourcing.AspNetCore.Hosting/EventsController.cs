﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{

    [Route("/v1/events/")]
    //[Authorize]
    public class EventsController : Controller
    {
        private readonly IEnumerable<AspNetProjection> _projections;

        public EventsController(IEnumerable<AspNetProjection> projections, IHttpContextFactory contextFactory)
        {
            _projections = projections;
        }

        [HttpPut("{eventdescriptor}")]
        public async Task Test(string eventdescriptor)
        {
            var candidates = _projections.Where(x => x.EventDescriptor.Equals(eventdescriptor, StringComparison.OrdinalIgnoreCase)).ToArray();
            await Task.WhenAll(candidates.Select(x => x.Handle(HttpContext)));
        }
    }
}