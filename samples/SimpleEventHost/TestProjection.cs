﻿using System;
using System.EventSourcing.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace SimpleEventHost
{
    public class TestProjection : Projection<Tf1>
    {
        public TestProjection(IService svc)
        {

        }

        public override string EventDescriptor => "User.Created";

        public override Task Handle(Tf1 @event)
        {
            return Task.CompletedTask;
        }
    }
}