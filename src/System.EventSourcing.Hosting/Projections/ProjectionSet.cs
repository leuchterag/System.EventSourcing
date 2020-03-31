namespace System.EventSourcing.Hosting.Projections
{
    public class ProjectionSet
    {
        public Type ProjectionType { get; set; }

        public Type EventType { get; set; }
    }
}
