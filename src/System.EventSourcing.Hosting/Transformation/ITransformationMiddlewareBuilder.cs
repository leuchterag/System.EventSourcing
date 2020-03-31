using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public interface ITransformationMiddlewareBuilder<TContext>
        where TContext : IContext
    {
        IList<Func<TContext, Func<TContext, Task>, Task>> Handlers { get; set; }
    }
}