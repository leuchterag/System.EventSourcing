using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Middleware
{
    public interface IMiddlewareAppBuilder<TContext, TBaseContextType> : IConfigurable
        where TContext : IContext
    {
        IEventSourcingBuilder<TBaseContextType> Base { get; set; }

        void Use(Func<TContext, Func<TContext, Task>, Task> middleware);
        
        Middleware<TContext> Build();
    }
}
