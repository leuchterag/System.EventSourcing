using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public class TransformationBuilder<TContext> : ITransformationBuilder<TContext>
        where TContext : IContext
    {
        public ITransformationMiddlewareBuilder<TContext> Base { get; set; }

        public Func<TContext, bool> Condition { get; set; }
        
        public Func<TContext, Task<TContext>> Transformation { get; set; }

        public Func<TContext, Func<TContext, Task>, Task> Handler { get; set; }
    }
}