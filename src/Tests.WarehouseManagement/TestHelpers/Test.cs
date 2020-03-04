﻿using System;
using System.Collections.Generic;
using WarehouseManagement;
using WarehouseManagement.Commands;
using WarehouseManagement.Events;
using Xunit;

namespace Tests.WarehouseManagement.TestHelpers
{
    public class When<TAggregate> where TAggregate : class
    {
        private IEnumerable<Event> _givenEvents;
        private IEnumerable<Command> _commands;

        public When(IEnumerable<Event> givenEvents, IEnumerable<Command> commands)
        {
            _givenEvents = givenEvents;
            _commands = commands;
        }

        public TAggregate Then(params Event[] expectedEvents)
        {
            var type = typeof(TAggregate);
            var constructor = type.GetConstructor(new[] { typeof(IMediator), typeof(IEnumerable<Event>) });

            var mediatorMock = new MockMediator();
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
                var constructor = type.GetConstructor(new[] { typeof(IMediator), typeof(IEnumerable<Event>) });

                var mediatorMock = new MockMediator();
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

    public class Given<TAggregate> where TAggregate : class
    {
        private IEnumerable<Event> _events;

        public Given(IEnumerable<Event> events)
        {
            _events = events;
        }

        public When<TAggregate> When(params Command[] commands)
        {
            return new When<TAggregate>(_events, commands);
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
