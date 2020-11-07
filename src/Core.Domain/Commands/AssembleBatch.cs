using Core.Extensions.Messaging;
using System.Collections.Generic;

namespace Core.Commands
{
    public class AssembleBatch : Command
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }

        public IReadOnlyCollection<BatchQuantity> From { get; set; }
    }
}
