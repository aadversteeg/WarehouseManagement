using AutoFixture;
using System;
using UnitTests.Core.TestHelpers;
using Core.Events;
using Xunit;
using Core.Domain;
using Core.Domain.Commands;

namespace UnitTests.Domain
{
    public class WarehouseTests
    {
        [Fact(DisplayName ="W001 - ReceiveBatch should add batch to warehouse on location receive.")]
        public void W001()
        {
            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { LocationId = 1, Name = "Receive" },
                    new CreatedLocation() { LocationId = 2, Name = "Ship" },
                    new CreatedLocation() { LocationId = 3, Name = "Assemble" },
                    new CreatedLocation() { LocationId = 4, Name = "Store" }
                )
                .When( w => w.ReceiveBatch( 1, 10))
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
                    new CreatedLocation() { LocationId = 1, Name = "Receive" },
                    new CreatedLocation() { LocationId = 2, Name = "Ship" },
                    new CreatedLocation() { LocationId = 3, Name = "Assemble" },
                    new CreatedLocation() { LocationId = 4, Name = "Store" },
                    new AddedBatch { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 2, BatchId = 1, Quantity = 10 })
                .When( w => w.ShipBatch(1,7))
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
                    new CreatedLocation() { LocationId = 1, Name = "Receive" },
                    new CreatedLocation() { LocationId = 2, Name = "Ship" },
                    new CreatedLocation() { LocationId = 3, Name = "Assemble" },
                    new CreatedLocation() { LocationId = 4, Name = "Store" },
                    new AddedBatch { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 3, BatchId = 1, Quantity = 10 },
                    new AddedBatch { BatchId = 2, Quantity = 10 },
                    new AddedBatchToLocation { LocationId = 3, BatchId = 2, Quantity = 10 }
                )
                .When(w => w.AssembleBatch( 
                    3,
                    2,
                    new[]
                    {
                        new BatchQuantity { BatchId = 1, Quantity = 1},
                        new BatchQuantity { BatchId = 2, Quantity = 2}
                    }
                ))
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
                    new CreatedLocation() { LocationId = 1, Name = "Receive" },
                    new CreatedLocation() { LocationId = 2, Name = "Ship" },
                    new CreatedLocation() { LocationId = 3, Name = "Assemble" },
                    new CreatedLocation() { LocationId = 4, Name = "Store" },
                    new AddedBatchToLocation { LocationId = 2, BatchId = 1, Quantity = 5 })
                .When(w => w.ShipBatch(1, 7))
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W005 - ShipBatch should give error when batch not in warehouse")]
        public void W005()
        {
            new Test<Warehouse>()
                .Given()
                .When(w => w.ShipBatch(1, 7))
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
                    new CreatedLocation() { LocationId = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() },
                    new AddedBatch() { BatchId = 1, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = 1, LocationId = 4, Quantity = 10 },
                    new AddedBatch() { BatchId = 2, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = 2, LocationId = 5, Quantity = 10 }
                )
                .When( w => w.MoveBatch(1, 7, 4, 5))
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
                    new CreatedLocation() { LocationId = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() },
                    new AddedBatch() { BatchId = batchId, Quantity = 10 },
                    new AddedBatchToLocation() { BatchId = batchId, LocationId = 4, Quantity = 10 }
                )
                .When( w => w.MoveBatch(batchId, 7, 4, 5))
                .Then(
                    new RemovedBatchFromLocation() { BatchId = batchId, LocationId = 4, Quantity = 7 },
                    new AddedBatchToLocation() { BatchId = batchId, LocationId = 5, Quantity = 7 }
                );
        }

        [Fact(DisplayName = "W008 - Creating location with unused name should succeed.")]
        public void W008()
        {
            var fixture = new Fixture();

            var newLocationname = fixture.Create<string>();

            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { LocationId = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.CreateLocation(fixture.Create<string>()))
                .Then(
                    new CreatedLocation { LocationId = 6, Name = newLocationname }
                );
        }

        [Fact(DisplayName = "W009 - Creating location with used name should fail.")]
        public void W009()
        {
            var fixture = new Fixture();

            var newLocationname = fixture.Create<string>();

            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { LocationId = 4, Name = newLocationname },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.CreateLocation(newLocationname))
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W010 - Creating location with null name should fail.")]
        public void W010()
        {
            var fixture = new Fixture();

            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { LocationId = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.CreateLocation(null))
                .Then<ArgumentNullException>();
        }

        [Fact(DisplayName = "W011 - Creating location with empty name should fail.")]
        public void W011()
        {
            var fixture = new Fixture();

            new Test<Warehouse>()
                .Given(
                    new CreatedLocation() { LocationId = 4, Name = fixture.Create<string>() },
                    new CreatedLocation() { LocationId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.CreateLocation(string.Empty))
                .Then<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "W012 - Registering batch with unused name should succeed.")]
        public void W012()
        {
            var fixture = new Fixture();

            var newBatchName = fixture.Create<string>();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = 4, Name = fixture.Create<string>() },
                    new RegisteredBatch() { BatchId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.RegisterBatch(fixture.Create<string>()))
                .Then(
                    new RegisteredBatch { BatchId = 6, Name = newBatchName }
                );
        }

        [Fact(DisplayName = "W013 - Registering batch with used name should fail.")]
        public void W013()
        {
            var fixture = new Fixture();
            var batchId = fixture.Create<int>();

            var newBatchName = fixture.Create<string>();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = 4, Name = newBatchName },
                    new RegisteredBatch() { BatchId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.RegisterBatch(newBatchName))
                .Then<InvalidOperationException>();
        }

        [Fact(DisplayName = "W014 - Registering batch with null name should fail.")]
        public void W014()
        {
            var fixture = new Fixture();
            var batchId = fixture.Create<int>();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = 4, Name = fixture.Create<string>() },
                    new RegisteredBatch() { BatchId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.RegisterBatch(null))
                .Then<ArgumentNullException>();
        }

        [Fact(DisplayName = "W015 - Registering batch with null name should fail.")]
        public void W015()
        {
            var fixture = new Fixture();
            var batchId = fixture.Create<int>();

            new Test<Warehouse>()
                .Given(
                    new RegisteredBatch() { BatchId = 4, Name = fixture.Create<string>() },
                    new RegisteredBatch() { BatchId = 5, Name = fixture.Create<string>() }
                )
                .When(w => w.RegisterBatch(string.Empty))
                .Then<ArgumentOutOfRangeException>();
        }
    }
}
