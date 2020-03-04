namespace WarehouseManagement.Commands
{
    public class MoveBatch : Command
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }

        public int FromLocationId { get; set; }

        public int ToLocationId { get; set; }
    }
}
