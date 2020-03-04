using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseManagement.Events;

namespace WarehouseManagement
{
    public interface IMediator
    {
        Task Publish(Event evt);
        Task Publish(IEnumerable<Event> events);
    }
}
