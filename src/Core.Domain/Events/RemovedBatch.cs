using Core.Extensions.Messaging;

namespace Core.Events
{
    public class RemovedBatch : Event
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }

    }
}
