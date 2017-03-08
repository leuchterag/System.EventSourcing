using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleEventHost
{
    public class Tf1
    {
        public string T1 { get; set; }
    }

    [Route("/v1/test/")]
    public class TestMvcController
    {
        public TestMvcController(IService dummy)
        {

        }

        [HttpPut]
        public void Test([FromBody] Tf1 obj)
        {

        }
    }
}
