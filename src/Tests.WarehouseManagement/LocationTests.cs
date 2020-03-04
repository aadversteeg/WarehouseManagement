using AutoFixture;
using WarehouseManagement;
using Xunit;

namespace Tests.WarehouseManagement
{
    public class LocationTests
    {
        [Fact(DisplayName="L001 - Location with no batches is empty.")]
        public void L001()
        {
            // arrange
            var location = new Location();

            // act
            var isEmpty = location.IsEmpty;

            // assert
            Assert.True(isEmpty);
        }

        [Fact(DisplayName = "L002 - Location with a batch is not empty.")]
        public void L002()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            location.AddBatch(fixture.Create<int>(), fixture.Create<int>());

            // act
            var isEmpty = location.IsEmpty;

            // assert
            Assert.False(isEmpty);
        }

        [Fact(DisplayName = "L003 - Location contains added batch.")]
        public void L003()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            var batchId = fixture.Create<int>();
            location.AddBatch(batchId, fixture.Create<int>());

            // act
            var containsBatch = location.ContainsBatch(batchId);

            // assert
            Assert.True(containsBatch);
        }

        [Fact(DisplayName = "L004 - Location does not contain not added batch.")]
        public void L004()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            var idOfBatch1 = fixture.Create<int>();
            var idOfBatch2 = idOfBatch1 + fixture.Create<int>();
            location.AddBatch(idOfBatch1, fixture.Create<int>());

            // act
            var containsBatch = location.ContainsBatch(idOfBatch2);

            // assert
            Assert.False(containsBatch);
        }

        [Fact(DisplayName = "L005 - QuantityOfBatch returns sum of added and removed batches.")]
        public void L005()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            var batchId = fixture.Create<int>();
            var quantity1 = fixture.Create<int>();
            var quantity2 = fixture.Create<int>() + quantity1;
            location.AddBatch(batchId, quantity2);
            location.RemoveBatch(batchId, quantity1);

            // act
            var quantityOfBatch = location.QuantityOfBatch(batchId);
            
            // assert
            Assert.Equal(quantity2-quantity1, quantityOfBatch);
        }

        [Fact(DisplayName = "L006 - QuantityOfBatch returns zero if added batches are removed.")]
        public void L006()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            var idOfBatchOne = fixture.Create<int>();
            var idOfBatchTwo = fixture.Create<int>();
            var quantity1 = fixture.Create<int>();
            var quantity2 = quantity1;
            location.AddBatch(idOfBatchOne, quantity2);
            location.RemoveBatch(idOfBatchOne, quantity1);

            // act
            var quantityOfBatch = location.QuantityOfBatch(idOfBatchTwo);

            // assert
            Assert.Equal(0, quantityOfBatch);
        }



        [Fact(DisplayName = "L007 - Location does not contain batch if added batches are removed.")]
        public void L007()
        {
            // arrange
            var fixture = new Fixture();
            var location = new Location();
            var idOfBatchOne = fixture.Create<int>();
            var idOfBatchTwo = fixture.Create<int>();
            var quantity1 = fixture.Create<int>();
            var quantity2 = quantity1;
            location.AddBatch(idOfBatchOne, quantity2);
            location.RemoveBatch(idOfBatchOne, quantity1);

            // act
            var quantityOfBatch = location.ContainsBatch(idOfBatchTwo);

            // assert
            Assert.False(quantityOfBatch);
        }
    }
}
