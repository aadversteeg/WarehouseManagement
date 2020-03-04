using System;
using System.Collections.Generic;
using System.Linq;
using WarehouseManagement.Commands;
using WarehouseManagement.Events;

namespace WarehouseManagement
{
    public partial class Warehouse
    {
        private void Execute(ReceiveBatch cmd)
        {
            if (cmd == null) throw new ArgumentNullException();
            if (cmd.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.Quantity));
            if (cmd.BatchId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.BatchId));

            var receiveLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Receive");
            HandleAndPublish(new AddedBatch() { BatchId = cmd.BatchId, Quantity = cmd.Quantity });
            HandleAndPublish(new AddedBatchToLocation() { LocationId = receiveLocation.Id, BatchId = cmd.BatchId, Quantity = cmd.Quantity });
        }

        private void Execute(ShipBatch cmd)
        {
            if (cmd == null) throw new ArgumentNullException();
            if (cmd.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.Quantity));
            if (cmd.BatchId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.BatchId));

            var shipLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Ship");

            var quantityOnLocation = shipLocation.QuantityOfBatch(cmd.BatchId);

            if (quantityOnLocation < cmd.Quantity)
            {
                throw new InvalidOperationException($"Quantity {cmd.Quantity} of batch with id {cmd.BatchId} is not present on {shipLocation}. Available quantity is {quantityOnLocation}.");
            }

            HandleAndPublish(new RemovedBatchFromLocation() { LocationId = shipLocation.Id, BatchId = cmd.BatchId, Quantity = cmd.Quantity });
            HandleAndPublish(new RemovedBatch() { BatchId = cmd.BatchId, Quantity = cmd.Quantity });
        }

        private void Execute(AssembleBatch cmd)
        {
            if (cmd == null) throw new ArgumentNullException();
            if (cmd.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.Quantity));
            if (cmd.BatchId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.BatchId));
            if (cmd.From == null) throw new ArgumentNullException();

            var assembleLocation = _locationByLocationId.Values.First(locationId => locationId.Name == "Assemble");

            var events = new List<Event>();
            foreach (var source in cmd.From)
            {
                var requiredQuantity = source.Quantity * cmd.Quantity;
                var quantityOnLocation = assembleLocation.QuantityOfBatch(source.BatchId);
                if (quantityOnLocation < requiredQuantity)
                {
                    throw new InvalidOperationException($"Quantity {requiredQuantity} of batch with id {cmd.BatchId} is not present on location {assembleLocation}. Available quantity is {quantityOnLocation}.");
                }
                events.Add(new RemovedBatchFromLocation() { LocationId = assembleLocation.Id, BatchId = source.BatchId, Quantity = requiredQuantity });
                events.Add(new RemovedBatch() { BatchId = source.BatchId, Quantity = requiredQuantity });
            }
            events.Add(new AddedBatch() { BatchId = cmd.BatchId, Quantity = cmd.Quantity });
            events.Add(new AddedBatchToLocation() { LocationId = assembleLocation.Id, BatchId = cmd.BatchId, Quantity = cmd.Quantity });

            HandleAndPublish(events);
        }

        private void Execute(MoveBatch cmd)
        {
            if (cmd == null) throw new ArgumentNullException();
            if (cmd.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.Quantity));
            if (cmd.BatchId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.BatchId));
            if (cmd.FromLocationId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.FromLocationId));
            if (cmd.ToLocationId <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.ToLocationId));

            var batch = _batchesById[cmd.BatchId];
            if(batch.IsPharma)
            {
                var toLocation = _locationByLocationId[cmd.ToLocationId];
                if( !toLocation.IsEmpty)
                {
                    throw new InvalidOperationException( $"Moving pharma batch {batch} to non empty location {toLocation} is not allowed");
                }
            }

            HandleAndPublish(new RemovedBatchFromLocation() { LocationId = cmd.FromLocationId, BatchId = cmd.BatchId, Quantity = cmd.Quantity });
            HandleAndPublish(new AddedBatchToLocation() { LocationId = cmd.ToLocationId, BatchId = cmd.BatchId, Quantity = cmd.Quantity });
        }
    }
}
