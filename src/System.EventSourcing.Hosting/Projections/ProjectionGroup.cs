using System.Collections.Generic;

namespace System.EventSourcing.Hosting.Projections
{
    public class ProjectionGroup
    {
        public string EventKey { get; set; }

        public IEnumerable<ProjectionSet> Projections { get; set; }
    }
}
