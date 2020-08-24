using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public interface ITransformationMiddlewareBuilder<TContext>
        where TContext : IContext
    {
        IList<Func<TContext, Task<(bool considerOrigin, bool transformApplies, TContext transformed)>>> Handlers { get; set; }
    }
}