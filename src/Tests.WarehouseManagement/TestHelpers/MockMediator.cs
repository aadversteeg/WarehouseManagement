using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseManagement;
using WarehouseManagement.Events;

namespace Tests.WarehouseManagement.TestHelpers
{
    public class MockMediator : IMediator
    {
        public IList<Event> _events = new List<Event>();

        public Task Publish(Event evt)
        {
            _events.Add(evt);
            return Task.CompletedTask;
        }

        public Task Publish(IEnumerable<Event> events)
        {
            foreach (var evt in events)
            {
                _events.Add(evt);
            }
            return Task.CompletedTask;
        }

        public IEnumerable<Event> PublishedEvents => _events;
    }
}
