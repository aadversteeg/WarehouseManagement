using System.Collections.Generic;
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
