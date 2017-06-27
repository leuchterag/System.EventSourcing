using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Internal;
using System.Collections.Generic;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public class EventControllerFactory : DefaultControllerFactory
    {
        public EventControllerFactory(IControllerActivator controllerActivator, IEnumerable<IControllerPropertyActivator> propertyActivators) : base(controllerActivator, propertyActivators)
        {
        }

        public override object CreateController(ControllerContext context)
        {
            context.ActionDescriptor.ControllerTypeInfo = typeof(EventsController).GetTypeInfo();
            return base.CreateController(context);
        }

        public override void ReleaseController(ControllerContext context, object controller)
        {
            // this can just be ignored since the controller will be dismantled when the scope closes...
        }
    }
}
