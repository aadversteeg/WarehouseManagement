using System.Collections.Generic;

namespace Core.Extensions
{
    public class Error : ValueObject
    {
        public Error(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public string Code { get; }

        public string Description { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Code;
            yield return Description;
        }
    }
}
