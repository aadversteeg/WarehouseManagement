using Core.Extensions.Messaging;

namespace Core.Domain.Commands
{
    public class CreateLocation : Command
    {
        public string Name { get; set; }
    }
}
