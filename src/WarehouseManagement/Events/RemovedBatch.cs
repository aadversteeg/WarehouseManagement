﻿namespace WarehouseManagement.Events
{
    public class RemovedBatch : Event
    {
        public int BatchId { get; set; }
        public int Quantity { get; set; }

    }
}
