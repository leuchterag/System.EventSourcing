using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Transformation
{
    public class TransformationMiddlewareBuilder<TContext> : ITransformationMiddlewareBuilder<TContext>
        where TContext : IContext
    {
        public IList<Func<TContext, Task<(bool considerOrigin, bool transformApplies, TContext transformed)>>> Handlers { get; set; }
            = new List<Func<TContext, Task<(bool considerOrigin, bool transformApplies, TContext transformed)>>>();
    }
}