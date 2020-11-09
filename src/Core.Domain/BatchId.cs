using Core.Extensions;
using System.Collections.Generic;

namespace Core.Domain
{
    public class BatchId : ValueObject
    {
        private int _value;

        private BatchId(int value)
        {
            _value = value;
        }

        public static Result<BatchId> Create(int value)
        {
            return Result.Success(new BatchId(value));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
