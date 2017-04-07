using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleEventHost
{
    public class Tf
    {
        public string T { get; set; }
    }

    [Route("/v1/events/user")]
    [Authorize]
    public class TestProjectionController : Controller
    {
        public TestProjectionController(IService dummy)
        {

        }

        [HttpPut]
        public void Test([FromBody] Tf obj)
        {
            var x = this;
        }
    }
}
