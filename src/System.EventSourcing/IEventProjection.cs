namespace System.EventSourcing
{
    public interface IEventProjection
    {
        string EventDescriptor { get; }
    }
}