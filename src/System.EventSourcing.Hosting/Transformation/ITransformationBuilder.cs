using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public interface ITransformationBuilder<TContext>
        where TContext : IContext
    {
        ITransformationMiddlewareBuilder<TContext> Base { get; set; }
        
        Func<TContext, bool> Condition { get; set; }
        
        Func<TContext, Task<TContext>> Transformation { get; set; }

        Func<TContext, Func<TContext, Task>, Task> Handler { get; set; }
    }
}