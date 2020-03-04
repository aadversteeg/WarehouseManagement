using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Commands;
using WarehouseManagement.Events;

namespace WarehouseManagement
{
    public partial class Warehouse
    {
        private IDictionary<int, int> _quantityByBatchId = new Dictionary<int, int>();
        private IDictionary<int, Location> _locationByLocationId = new Dictionary<int, Location>();
        private IDictionary<int, Batch> _batchesById = new Dictionary<int, Batch>();

        private IMediator _mediator;

        public Warehouse(IMediator mediator, IEnumerable<Event> events)
            : this(mediator)
        {
            Handle(events);
        }

        public Warehouse(IMediator mediator)
        {
            _mediator = mediator;
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

        private Task HandleAndPublish(IEnumerable<Event> events)
        {
            foreach (var ev in events)
            {
                Handle(ev);
            }
            return _mediator.Publish(events);
        }

        private void Handle(IEnumerable<Event> events)
        {
            foreach (var ev in events)
            {
                Handle(ev);
            }
        }

        private Task HandleAndPublish(Event evt)
        {
            Handle(evt);
            return _mediator.Publish(evt);
        }

        private void Handle(Event evt)
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
