using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Extensions.Messaging;

namespace Core.Extensions
{
    public interface IEventSink
    {
        Task Publish(IReadOnlyCollection<Event> events);
    }
}
