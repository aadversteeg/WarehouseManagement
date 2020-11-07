using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Extensions;
using Core.Extensions.Messaging;

namespace Core
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

        public void AssembleBatch(BatchAssembleOrder order)
        {
            if (order == null) throw new ArgumentNullException();
            if (order.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(order.Quantity));
            if (order.BatchId <= 0) throw new ArgumentOutOfRangeException(nameof(order.BatchId));
            if (order.From == null) throw new ArgumentNullException();

            var assembleLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Assemble");

            var events = new List<Event>();
            foreach (var source in order.From)
            {
                var requiredQuantity = source.Quantity * order.Quantity;
                var quantityOnLocation = assembleLocation.QuantityOfBatch(source.BatchId);
                if (quantityOnLocation < requiredQuantity)
                {
                    throw new InvalidOperationException($"Quantity {requiredQuantity} of batch with id {order.BatchId} is not present on location {assembleLocation}. Available quantity is {quantityOnLocation}.");
                }
                events.Add(new RemovedBatchFromLocation() { LocationId = assembleLocation.Id, BatchId = source.BatchId, Quantity = requiredQuantity });
                events.Add(new RemovedBatch() { BatchId = source.BatchId, Quantity = requiredQuantity });
            }
            events.Add(new AddedBatch() { BatchId = order.BatchId, Quantity = order.Quantity });
            events.Add(new AddedBatchToLocation() { LocationId = assembleLocation.Id, BatchId = order.BatchId, Quantity = order.Quantity });

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
                Id = evt.Id,
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
            if (cmd is ReceiveBatch receiveBatch)
            {
                Execute(receiveBatch);
            }

            if (cmd is ShipBatch shipBatch)
            {
                Execute(shipBatch);
            }

            if (cmd is AssembleBatch assembleBatch)
            {
                Execute(assembleBatch);
            }

            if(cmd is MoveBatch moveBatch)
            {
                Execute(moveBatch);
            }
        }
    }
}
