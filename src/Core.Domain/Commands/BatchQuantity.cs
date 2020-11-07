using Core.Extensions.Messaging;

namespace Core.Commands
{
    public class BatchQuantity : Command
    {
        public int BatchId { get; set;}
        public int Quantity { get; set; }
    }
}
