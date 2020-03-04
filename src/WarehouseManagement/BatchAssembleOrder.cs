using System.Collections.Generic;

namespace WarehouseManagement.Commands
{
    public class BatchAssembleOrder 
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }

        public IEnumerable<BatchQuantity> From { get; set; }
    }
}
