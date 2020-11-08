using Core.Extensions.Messaging;

namespace Core.Domain.Commands
{
    public class RegisterBatch : Command
    {
        public string Name { get; set; }
    }
}
