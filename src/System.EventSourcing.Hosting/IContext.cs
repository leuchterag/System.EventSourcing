namespace System.EventSourcing.Hosting
{
    public interface IContext
    {
        IServiceProvider Services { get; set; }
    }
}
