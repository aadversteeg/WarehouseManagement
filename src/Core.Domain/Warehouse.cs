using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Commands;
using Core.Events;
using Core.Extensions;
using Core.Extensions.Messaging;

namespace Core.Domain
{
    public partial class Warehouse : AggregateRoot
    {
        private IDictionary<int, int> _quantityByBatchId = new Dictionary<int, int>();
        private IDictionary<int, Location> _locationByLocationId = new Dictionary<int, Location>();
        private IDictionary<int, Batch> _batchesById = new Dictionary<int, Batch>();

        public Warehouse(IEventSink mediator) 
            : base(mediator)
        {
        }

        public Warehouse(IEventSink mediator, IReadOnlyCollection<Event> events) 
            : base(mediator, events)
        {
        }

        public void CreateLocation(string name)
        {
            if(name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if(name == string.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
            if( _locationByLocationId.Values.Any(l => l.Name == name ))
            {
                throw new InvalidOperationException($"Location with name {name} is already registered.");
            }

            var newLocationId = _locationByLocationId.Keys.Max() + 1;
            HandleAndPublish(new CreatedLocation { LocationId = newLocationId, Name = name });
        }

        public void RegisterBatch(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name == string.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
            if (_batchesById.Values.Any(b => b.Name == name))
            {
                throw new InvalidOperationException($"Batch with name {name} is already registered.");
            }
            var newBatchId = _batchesById.Keys.Max() + 1;
            HandleAndPublish(new RegisteredBatch { BatchId = newBatchId, Name = name });
        }

        public void ReceiveBatch(int batchId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (batchId <= 0) throw new ArgumentOutOfRangeException(nameof(batchId));

            var receiveLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Receive");
            HandleAndPublish(new AddedBatch() { BatchId = batchId, Quantity = quantity });
            HandleAndPublish(new AddedBatchToLocation() { LocationId = receiveLocation.Id, BatchId = batchId, Quantity = quantity });
        }

        public void ShipBatch(int batchId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (batchId <= 0) throw new ArgumentOutOfRangeException(nameof(batchId));

            var shipLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Ship");

            var quantityOnLocation = shipLocation.QuantityOfBatch(batchId);

            if (quantityOnLocation < quantity)
            {
                throw new InvalidOperationException($"Quantity {quantity} of batch with id {batchId} is not present on {shipLocation}. Available quantity is {quantityOnLocation}.");
            }

            HandleAndPublish(new RemovedBatchFromLocation() { LocationId = shipLocation.Id, BatchId = batchId, Quantity = quantity });
            HandleAndPublish(new RemovedBatch() { BatchId = batchId, Quantity = quantity });
        }

        public void MoveBatch(int batchId, int quantity, int fromLocationId, int toLocationId)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (batchId <= 0) throw new ArgumentOutOfRangeException(nameof(batchId));
            if (fromLocationId <= 0) throw new ArgumentOutOfRangeException(nameof(fromLocationId));
            if (toLocationId <= 0) throw new ArgumentOutOfRangeException(nameof(toLocationId));

            var batch = _batchesById[batchId];
            if (batch.IsPharma)
            {
                var toLocation = _locationByLocationId[toLocationId];
                if (!toLocation.IsEmpty)
                {
                    throw new InvalidOperationException($"Moving pharma batch {batch} to non empty location {toLocation} is not allowed");
                }
            }

            HandleAndPublish(new RemovedBatchFromLocation() { LocationId =fromLocationId, BatchId = batchId, Quantity = quantity });
            HandleAndPublish(new AddedBatchToLocation() { LocationId = toLocationId, BatchId = batchId, Quantity = quantity });
        }

        public void AssembleBatch(int batchId, int quantity, IReadOnlyCollection<BatchQuantity> from)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (batchId <= 0) throw new ArgumentOutOfRangeException(nameof(batchId));
            if (from == null) throw new ArgumentNullException(nameof(from));

            var assembleLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Assemble");

            var events = new List<Event>();
            foreach (var source in from)
            {
                var requiredQuantity = source.Quantity * quantity;
                var quantityOnLocation = assembleLocation.QuantityOfBatch(source.BatchId);
                if (quantityOnLocation < requiredQuantity)
                {
                    throw new InvalidOperationException($"Quantity {requiredQuantity} of batch with id {batchId} is not present on location {assembleLocation}. Available quantity is {quantityOnLocation}.");
                }
                events.Add(new RemovedBatchFromLocation() { LocationId = assembleLocation.Id, BatchId = source.BatchId, Quantity = requiredQuantity });
                events.Add(new RemovedBatch() { BatchId = source.BatchId, Quantity = requiredQuantity });
            }
            events.Add(new AddedBatch() { BatchId = batchId, Quantity = quantity });
            events.Add(new AddedBatchToLocation() { LocationId = assembleLocation.Id, BatchId = batchId, Quantity = quantity });

            HandleAndPublish(events);
        }

        private void Handle(AddedBatch evt)
        {
            if (_quantityByBatchId.TryGetValue(evt.BatchId, out int quantityInWarehouse))
            {
                _quantityByBatchId[evt.BatchId] = quantityInWarehouse + evt.Quantity;
            }
            else
            {
                _quantityByBatchId.Add(evt.BatchId, evt.Quantity);
            }
        }

        private void Handle(RemovedBatch evt)
        {
            var quantityInWarehouse = _quantityByBatchId[evt.BatchId];
            if (evt.Quantity > quantityInWarehouse)
            {
                quantityInWarehouse = quantityInWarehouse - evt.Quantity;

                _quantityByBatchId[evt.BatchId] = quantityInWarehouse;
            }
            _quantityByBatchId.Remove(evt.BatchId);
        }

        private void Handle(CreatedLocation evt)
        {
            var location = new Location()
            {
                Id = evt.LocationId,
                Name = evt.Name
            };
            _locationByLocationId.Add(location.Id, location);
        }

        private void Handle(AddedBatchToLocation evt)
        {
            var location = _locationByLocationId[evt.LocationId];
            location.AddBatch(evt.BatchId, evt.Quantity);
        }

        private void Handle(RemovedBatchFromLocation evt)
        {
            var location = _locationByLocationId[evt.LocationId];
            location.RemoveBatch(evt.BatchId, evt.Quantity);
        }

        private void Handle(RegisteredBatch evt)
        {
            var batch = new Batch()
            {
                Id = evt.BatchId,
                Name = evt.Name,
                IsPharma = evt.IsPharma
            };
            _batchesById.Add(batch.Id, batch);
        }

        protected override void Handle(Event evt)
        {
            if (evt is RemovedBatch shippedBatch)
            {
                Handle(shippedBatch);
            }

            if (evt is AddedBatch receivedBatch)
            {
                Handle(receivedBatch);
            }

            if (evt is CreatedLocation createdLocation)
            {
                Handle(createdLocation);
            }

            if (evt is RemovedBatchFromLocation removedBatchFromLocation)
            {
                Handle(removedBatchFromLocation);
            }

            if (evt is AddedBatchToLocation addedBatchToLocation)
            {
                Handle(addedBatchToLocation);
            }

            if (evt is RegisteredBatch registeredBatch)
            {
                Handle(registeredBatch);
            }
        }

        public void Execute(Command cmd)
        {
            if (cmd is Commands.ReceiveBatch receiveBatch)
            {
                ReceiveBatch(receiveBatch.BatchId, receiveBatch.Quantity);
            }

            if (cmd is Commands.ShipBatch shipBatch)
            {
                ShipBatch(shipBatch.BatchId, shipBatch.Quantity);
            }

            if (cmd is Commands.AssembleBatch assembleBatch)
            {
                Execute(assembleBatch);
            }

            if(cmd is Commands.MoveBatch moveBatch)
            {
                MoveBatch(moveBatch.BatchId, moveBatch.Quantity, moveBatch.FromLocationId, moveBatch.ToLocationId);
            }

            if(cmd is Commands.CreateLocation createLocation)
            {
                CreateLocation(createLocation.Name);
            }

            if(cmd is Commands.RegisterBatch registerBatch)
            {
                RegisterBatch(registerBatch.Name);
            }
        }
    }
}
