namespace WarehouseManagement.Events
{
    public class CreatedLocation : Event
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
