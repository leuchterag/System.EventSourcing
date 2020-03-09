namespace System.EventSourcing.Hosting.Middleware
{
    public class MiddlewareContext<TKey, TContent> : IContext
    {
        public TKey Key { get; set; }

        public TContent Content { get; set; }

        public IServiceProvider Services { get; set; }
    }
}
