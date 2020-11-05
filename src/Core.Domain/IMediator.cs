using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;

namespace Core
{
    public interface IMediator
    {
        Task Publish(Event evt);
        Task Publish(IEnumerable<Event> events);
    }
}
