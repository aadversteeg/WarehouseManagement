namespace WarehouseManagement.Commands
{
    public class ReceiveBatch : Command
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }
    }
}
