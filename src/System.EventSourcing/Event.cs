namespace System.EventSourcing
{
    public class Event
    {
        public string Name { get; set; }

        public byte[] Content { get; set; }
    }
}
