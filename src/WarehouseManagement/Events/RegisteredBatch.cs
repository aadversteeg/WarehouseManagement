namespace WarehouseManagement.Events
{
    public class RegisteredBatch : Event
    {
        public int BatchId { get; set; }

        public string Name { get; set; }

        public bool IsPharma { get; set; }
    }
}
