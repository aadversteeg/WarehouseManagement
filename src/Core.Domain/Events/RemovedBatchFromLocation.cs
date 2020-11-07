using Core.Extensions.Messaging;

namespace Core.Events
{
    public class RemovedBatchFromLocation : Event
    {
        public int LocationId { get; set; }
        public int BatchId { get; set; }
        public int Quantity { get; set; }

    }
}
