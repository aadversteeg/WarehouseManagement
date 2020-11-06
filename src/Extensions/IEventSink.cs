using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions.Messaging;

namespace Extensions
{
    public interface IEventSink
    {
        Task Publish(IReadOnlyCollection<Event> events);
    }
}
