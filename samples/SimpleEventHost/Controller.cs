﻿using Microsoft.AspNetCore.Mvc;

namespace SimpleEventHost
{
    public class Tf
    {
        public string T { get; set; }
    }

    [Route("/v1/events/user")]
    public class TestController
    {
        public TestController()
        {

        }

        [HttpPut]
        public void Test([FromBody] Tf obj)
        {
            
        }
    }
}
