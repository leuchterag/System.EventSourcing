namespace System.EventSourcing.AspNetCore.Hosting
{
    public class ServiceRedirectionTarget : IRedirectionTarget
    {
        public ServiceRedirectionTarget(IServiceProvider redirect)
        {
            Provider = redirect;
        }

        public IServiceProvider Provider { get; private set; }
    }
}
