using Core.Extensions;
using System.Collections.Generic;

namespace Core.Domain
{
    public class LocationId : ValueObject
    {
        private int _value;

        private LocationId(int value)
        {
            _value = value;
        }

        public static Result<LocationId> Create(int value)
        {
            return Result.Success(new LocationId(value));
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
