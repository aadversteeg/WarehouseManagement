using Extensions.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions
{
    public abstract class AggregateRoot
    {
        private IEventSink _eventSink;

        public AggregateRoot(IEventSink eventSink, IReadOnlyCollection<Event> events)
            : this(eventSink)
        {
            Handle(events);
        }

        public AggregateRoot(IEventSink eventSink)
        {
            _eventSink = eventSink;
        }

        protected Task HandleAndPublish(IReadOnlyCollection<Event> events)
        {
            foreach (var ev in events)
            {
                Handle(ev);
            }
            return _eventSink.Publish(events);
        }

        protected void Handle(IReadOnlyCollection<Event> events)
        {
            foreach (var ev in events)
            {
                Handle(ev);
            }
        }

        protected Task HandleAndPublish(Event evt)
        {
            Handle(evt);
            return _eventSink.Publish(new[] { evt });
        }

        protected abstract void Handle(Event evt);
    }
}
