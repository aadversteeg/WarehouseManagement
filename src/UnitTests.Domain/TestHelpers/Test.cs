using System;
using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.Messaging;
using Xunit;

namespace UnitTests.Core.TestHelpers
{
    public class WhenUsingCommands<TAggregate> where TAggregate : class
    {
        private IEnumerable<Event> _givenEvents;
        private IEnumerable<Command> _commands;

        public WhenUsingCommands(IEnumerable<Event> givenEvents, IEnumerable<Command> commands)
        {
            _givenEvents = givenEvents;
            _commands = commands;
        }

        public TAggregate Then(params Event[] expectedEvents)
        {
            var type = typeof(TAggregate);
            var constructor = type.GetConstructor(new[] { typeof(IEventSink), typeof(IEnumerable<Event>) });

            var mediatorMock = new MockEventSink();
            var aggregate = constructor.Invoke(new object[] { mediatorMock, _givenEvents }) as TAggregate;

            var method = type.GetMethod("Execute", new[] { typeof(Command) });
            foreach (var command in _commands)
            {
                method.Invoke(aggregate, new[] { command });
            }

            Assert.Equal(expectedEvents, mediatorMock.PublishedEvents, new CustomComparer<Event>());
            return aggregate;
        }

        public void Then<TException>() where TException : Exception
        {
            Exception exceptionThrown = null;

            try
            {
                var type = typeof(TAggregate);
                var constructor = type.GetConstructor(new[] { typeof(IEventSink), typeof(IEnumerable<Event>) });

                var mediatorMock = new MockEventSink();
                var aggregate = constructor.Invoke(new object[] { mediatorMock, _givenEvents }) as TAggregate;

                var method = type.GetMethod("Execute", new[] { typeof(Command) });
                foreach (var command in _commands)
                {
                    method.Invoke(aggregate, new[] { command });
                }
            }
            catch (Exception e)
            {
                exceptionThrown = e.InnerException;
            }
            Assert.Equal(typeof(TException), exceptionThrown?.GetType());
        }
    }

    public class WhenUsingAction<TAggregate> where TAggregate : class
    {
        private IEnumerable<Event> _givenEvents;
        private Action<TAggregate> _action;

        public WhenUsingAction(IEnumerable<Event> givenEvents, Action<TAggregate> action)
        {
            _givenEvents = givenEvents;
            _action = action;
        }

        public TAggregate Then(params Event[] expectedEvents)
        {
            var type = typeof(TAggregate);
            var constructor = type.GetConstructor(new[] { typeof(IEventSink), typeof(IReadOnlyCollection<Event>)});

            var mediatorMock = new MockEventSink();
            var aggregate = constructor.Invoke(new object[] { mediatorMock, _givenEvents }) as TAggregate;

            _action(aggregate);
            

            Assert.Equal(expectedEvents, mediatorMock.PublishedEvents, new CustomComparer<Event>());
            return aggregate;
        }

        public void Then<TException>() where TException : Exception
        {
            Exception exceptionThrown = null;

            try
            {
                var type = typeof(TAggregate);
                var constructor = type.GetConstructor(new[] { typeof(IEventSink), typeof(IReadOnlyCollection<Event>) });

                var mediatorMock = new MockEventSink();
                var aggregate = constructor.Invoke(new object[] { mediatorMock, _givenEvents }) as TAggregate;

                _action(aggregate);
            }
            catch (Exception e)
            {
                exceptionThrown = e;
            }
            Assert.Equal(typeof(TException), exceptionThrown?.GetType());
        }
    }

    public class Given<TAggregate> where TAggregate : class
    {
        private IEnumerable<Event> _events;

        public Given(IEnumerable<Event> events)
        {
            _events = events;
        }

        public WhenUsingCommands<TAggregate> When(params Command[] commands)
        {
            return new WhenUsingCommands<TAggregate>(_events, commands);
        }

        public WhenUsingAction<TAggregate> When(Action<TAggregate> action)
        {
            return new WhenUsingAction<TAggregate>(_events, action);
        }
    }

    public class Test<TAggregate> where TAggregate : class
    {
        public Given<TAggregate> Given(params Event[] events)
        {
            return new Given<TAggregate>(events);
        }
    }
}
