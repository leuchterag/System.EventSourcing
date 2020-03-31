using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public class TransformationMiddlewareBuilder<TContext> : ITransformationMiddlewareBuilder<TContext>
        where TContext : IContext
    {
        public IList<Func<TContext, Func<TContext, Task>, Task>> Handlers { get; set; } = new List<Func<TContext, Func<TContext, Task>, Task>>();
    }
}