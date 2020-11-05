using Core.Events;

namespace Core
{
    public partial class Warehouse
    {
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
    }
}
