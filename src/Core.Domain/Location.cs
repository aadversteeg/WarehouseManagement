using Core.Extensions;
using System.Collections.Generic;

namespace Core
{
    public class Location : Entity
    {
        IDictionary<int, int> _quantityByBatchId = new Dictionary<int, int>();

        public string Name { get; set; }

        public bool IsEmpty
        {
            get { return _quantityByBatchId.Count == 0; }
        }

        public void AddBatch( int batchId, int quantity)
        {
            if(_quantityByBatchId.TryGetValue(batchId, out int quantityOnLocation))
            {
                quantityOnLocation += quantity;
                _quantityByBatchId[batchId] = batchId;
            }
            else
            {
                _quantityByBatchId.Add(batchId, quantity);
            }
        }

        public void RemoveBatch(int batchId, int quantity)
        {
            var quantityOnLocation = _quantityByBatchId[batchId];
            if( quantityOnLocation > quantity)
            {
                _quantityByBatchId[batchId] = quantityOnLocation - quantity;
            }
            else
            {
                _quantityByBatchId.Remove(batchId);
            }
        }

        public int QuantityOfBatch(int batchId)
        {
            if( _quantityByBatchId.TryGetValue( batchId, out int quantity))
            {
                return quantity;
            }
            return 0;
        }

        public bool ContainsBatch(int batchId)
        {
            return _quantityByBatchId.ContainsKey(batchId);
        }

        public override string ToString()
        {
            return $"{Name} ({Id})"; 
        }
    }
}
