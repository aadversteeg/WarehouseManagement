using Core.Extensions.Messaging;

namespace Core.Events
{
    public class CreatedLocation : Event
    {
        public int LocationId { get; set; }

        public string Name { get; set; }
    }
}
