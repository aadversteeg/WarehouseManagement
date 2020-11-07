using Core.Extensions.Messaging;

namespace Core.Events
{
    public class AddedBatch : Event
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }
    }
}
