using System.Threading.Tasks;

namespace System.EventSourcing.Hosting.Middleware
{
    public delegate Task Middleware<TContext>(TContext ctx)
        where TContext : IContext;
}
