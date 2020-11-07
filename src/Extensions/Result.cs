using System;
using System.Collections.Generic;

namespace Extensions
{
    public class Result
    {
        protected readonly bool _succeeded;
        protected readonly IReadOnlyCollection<Error> _errors;

        protected Result()
        {
            _succeeded = true;
        }

        protected Result(IReadOnlyCollection<Error> errors)
        {
            _succeeded = false;
            _errors = errors;
        }

        public IReadOnlyCollection<Error> Errors
        {
            get
            {
                if(_succeeded)
                {
                    throw new InvalidOperationException();
                }
                return _errors;
            }
        }

        public bool Failed
        {
            get
            {
                return !_succeeded;
            }
        }

        public bool Succeeded
        {
            get
            {
                return _succeeded;
            }
        }

        public static Result Success()
        {
            return new Result();
        }

        public static Result Failure(IReadOnlyCollection<Error> errors)
        {
            return new Result(errors);
        }


        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value);
        }

        public static Result<TValue> Failure<TValue>(IReadOnlyCollection<Error> errors)
        {
            return new Result<TValue>(errors);
        }
    }
        

    public class Result<TValue> : Result 
    {
        TValue _value;

        internal Result(TValue value)
            : base()
        {
            _value = value;
        }

        internal Result(IReadOnlyCollection<Error> errors)
            : base(errors)
        {
        }

        public TValue Value
        {
            get
            {
                if (!_succeeded)
                {
                    throw new InvalidOperationException();
                }
                return _value;
            }
        }
    }
}
