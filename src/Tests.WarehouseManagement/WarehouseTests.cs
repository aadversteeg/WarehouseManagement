using AutoFixture;
using System;
using Tests.WarehouseManagement.TestHelpers;
using WarehouseManagement;
using WarehouseManagement.Commands;
using WarehouseManagement.Events;
using Xunit;

namespace Tests.WarehouseManagement
{
    public class WarehouseTests
    {
        [Fact(DisplayName ="W001 - ReceiveBatch should add batch to warehouse on location receive.")]
        public void W001()
        {
            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { Id = 1, Name = "Receive" },
                    new CreatedLocation() { Id = 2, Name = "Ship" },
                    new CreatedLocation() { Id = 3, Name = "Assemble" },
                    new CreatedLocation() { Id = 4, Name = "Store" }
                )
                .When(new ReceiveBatch { BatchId = 1, Quantity = 10 })
                .Then(
                    new AddedBatch { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation { BatchId = 1, LocationId = 1, Quantity = 10 }
                );
        }

        [Fact(DisplayName = "W002 - ShipBatch should remove batch from warehouse")]
        public void W002()
        {
            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { Id = 1, Name = "Receive" },
                    new CreatedLocation() { Id = 2, Name = "Ship" },
                    new CreatedLocation() { Id = 3, Name = "Assemble" },
                    new CreatedLocation() { Id = 4, Name = "Store" },
                    new AddedBatch { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 2, BatchId = 1, Quantity = 10 })
                .When(new ShipBatch { BatchId = 1, Quantity = 7 })
                .Then(
                    new RemovedBatchFromLocation { LocationId = 2, BatchId = 1, Quantity = 7 },
                    new RemovedBatch { BatchId = 1, Quantity = 7 }
                );
        }

        [Fact(DisplayName = "W003 - Assemble should remove batches for new batch from warehouse and add new batch")]
        public void W003()
        {
            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { Id = 1, Name = "Receive" },
                    new CreatedLocation() { Id = 2, Name = "Ship" },
                    new CreatedLocation() { Id = 3, Name = "Assemble" },
                    new CreatedLocation() { Id = 4, Name = "Store" },
                    new AddedBatch { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 3, BatchId = 1, Quantity = 10 },
                    new AddedBatch { BatchId = 2, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 3, BatchId = 2, Quantity = 10 }
                )
                .When(new AssembleBatch {
                    BatchId = 3,
                    Quantity = 2,
                    From = new[]
                    {
                        new BatchQuantity { BatchId = 1, Quantity = 1},
                        new BatchQuantity { BatchId = 2, Quantity = 2}
                    }
                })
                .Then(
                    new RemovedBatchFromLocation { LocationId = 3, BatchId = 1, Quantity = 2 },
                    new RemovedBatch { BatchId = 1, Quantity = 2 },
                    new RemovedBatchFromLocation { LocationId = 3, BatchId = 2, Quantity = 4 },
                    new RemovedBatch { BatchId = 2, Quantity = 4 },
                    new AddedBatch { BatchId = 3, Quantity = 2 },
                    new AddedBatchToLocation { LocationId = 3, BatchId = 3, Quantity = 2}
                );
        }

        [Fact(DisplayName = "W004 - ShipBatch should give error when not enouch available of batch in warehouse")]
        public void W004()
        {
            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { Id = 1, Name = "Receive" },
                    new CreatedLocation() { Id = 2, Name = "Ship" },
                    new CreatedLocation() { Id = 3, Name = "Assemble" },
                    new CreatedLocation() { Id = 4, Name = "Store" },
                    new AddedBatchToLocation { LocationId = 2, BatchId = 1, Quantity = 5 })
                .When(new ShipBatch { BatchId = 1, Quantity = 7 })
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W005 - ShipBatch should give error when batch not in warehouse")]
        public void W005()
        {
            new Test<Warehouse>()
                .Given()
                .When(new ShipBatch { BatchId = 1, Quantity = 7 })
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W006 - Moving pharma batch to location containing pharma batch gives an error")]
        public void W006()
        {
            var fixture = new Fixture();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = 1, Name = fixture.Create<string>(), IsPharma = true },
                    new RegisteredBatch() { BatchId = 2, Name = fixture.Create<string>(), IsPharma = false },
                    new CreatedLocation() { Id = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { Id = 5, Name = fixture.Create<string>() },
                    new AddedBatch() { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = 1, LocationId = 4, Quantity = 10 },
                    new AddedBatch() { BatchId = 2, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = 2, LocationId = 5, Quantity = 10 }
                )
                .When(new MoveBatch { BatchId = 1, Quantity = 7, FromLocationId = 4, ToLocationId = 5 })
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W007 - Moving batch to empty location")]
        public void W007()
        {
            var fixture = new Fixture();
            var batchId = fixture.Create<int>();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = batchId, Name = fixture.Create<string>(), IsPharma = true},
                    new CreatedLocation() { Id = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { Id = 5, Name = fixture.Create<string>() },
                    new AddedBatch() { BatchId = batchId, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = batchId, LocationId = 4, Quantity = 10 }
                )
                .When(new MoveBatch { BatchId = batchId, Quantity = 7, FromLocationId = 4, ToLocationId =5 })
                .Then(
                    new RemovedBatchFromLocation() { BatchId = batchId, LocationId = 4, Quantity = 7 },
                    new AddedBatchToLocation() { BatchId = batchId, LocationId = 5, Quantity = 7 }
                );
        }
    }
}
