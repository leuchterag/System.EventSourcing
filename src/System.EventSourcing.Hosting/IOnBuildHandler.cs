namespace System.EventSourcing.Hosting
{
    public interface IOnBuildHandler
    {
        void OnBuild(Action onBuildHook);
    }
}