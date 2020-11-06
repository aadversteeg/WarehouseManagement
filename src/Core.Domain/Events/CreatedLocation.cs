using Extensions.Messaging;

namespace Core.Events
{
    public class CreatedLocation : Event
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
