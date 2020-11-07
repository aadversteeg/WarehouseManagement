using Core.Extensions.Messaging;

namespace Core.Events
{
    public class RegisteredBatch : Event
    {
        public int BatchId { get; set; }

        public string Name { get; set; }

        public bool IsPharma { get; set; }
    }
}
