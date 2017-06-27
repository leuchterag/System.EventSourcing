using System.IO;
using System.Threading.Tasks;

namespace System.EventSourcing
{
    public interface IEventProjection
    {
        string EventDescriptor { get; }
    }
}