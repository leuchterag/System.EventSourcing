namespace System.EventSourcing.Hosting
{
    public interface IConfigurable
    {
        void Configure(Action onBuildHook);
    }
}