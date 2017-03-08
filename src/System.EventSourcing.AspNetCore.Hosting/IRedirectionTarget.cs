namespace System.EventSourcing.AspNetCore.Hosting
{
    public interface IRedirectionTarget
    {
        IServiceProvider Provider { get; }
    }
}