using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.EventSourcing.Client;
using System.Text;

namespace SimpleEventHost
{
    public class Tf1
    {
        public string T1 { get; set; }
    }

    [Route("/v1/test")]
    [Authorize]
    public class TestMvcController : Controller
    {
        readonly IEventClient _client;

        public TestMvcController(IService dummy, IEventClient client)
        {
            _client = client;
        }

        [HttpPut]
        public void Test([FromBody] Tf1 obj)
        {
            _client.Publish(new TEvent());
        }
    }
}
