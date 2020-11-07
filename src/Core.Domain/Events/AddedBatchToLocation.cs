using Core.Extensions.Messaging;

namespace Core.Events
{
    public class AddedBatchToLocation : Event
    {
        public int BatchId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
    }
}
