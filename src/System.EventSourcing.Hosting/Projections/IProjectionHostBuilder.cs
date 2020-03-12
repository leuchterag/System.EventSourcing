using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Projections
{
    public interface IProjectionHostBuilder<TContext>
    {
        IList<Func<IEnumerable<Type>>> ProjectionTypeSources { get; set; }
        
        Func<IEnumerable<Type>, IEnumerable<ProjectionGroup>> ProjectionExtractor { get; set; }

        Func<ProjectionSet, Func<TContext, Task>> InvocatorFactory { get; set; }

        Func<TContext, string> KeyExtractor { get; set; }

        Func<TContext, Func<Task>, Task> Build();
    }
}
