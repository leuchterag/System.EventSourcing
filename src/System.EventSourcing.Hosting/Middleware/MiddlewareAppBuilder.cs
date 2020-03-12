using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Middleware
{
    public class MiddlewareAppBuilder<TContext, TBaseContextType> : IMiddlewareAppBuilder<TContext, TBaseContextType> where TContext : IContext
    {
        private readonly IList<Action> buildHooks = new List<Action>();

        public IList<Func<TContext, Func<TContext, Task>, Task>> Middlewares { get; set; } = new List<Func<TContext, Func<TContext, Task>, Task>>();

        public IEventSourcingBuilder<TBaseContextType> Base { get; set; }

        public Middleware<TContext> Build()
        {
            foreach (var hook in buildHooks)
            {
                hook();
            }

            // Assemble the middlewares sequentially
            Func<TContext, Task> seed = (ctx) => Task.CompletedTask;
            var reversedMiddlewares = Middlewares.Reverse();
            var handler = reversedMiddlewares.Aggregate(seed, (x, y) => (TContext ctx) => y(ctx, x));

            Middleware<TContext> middleware = (ctx) => handler(ctx);
            return middleware;
        }

        public void Configure(Action configurationHook)
        {
            buildHooks.Add(configurationHook);
        }

        public void Use(Func<TContext, Func<TContext, Task>, Task> middleware) => Middlewares.Add(middleware);
    }
}
