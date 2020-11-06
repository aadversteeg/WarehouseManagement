using Extensions.Messaging;

namespace Core.Commands
{
    public class ShipBatch : Command
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }
    }
}
