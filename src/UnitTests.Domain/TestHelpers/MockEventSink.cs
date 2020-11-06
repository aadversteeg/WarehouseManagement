using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using Extensions.Messaging;

namespace UnitTests.Core.TestHelpers
{
    public class MockEventSink : IEventSink
    {
        public IList<Event> _events = new List<Event>();

        public Task Publish(IReadOnlyCollection<Event> events)
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
