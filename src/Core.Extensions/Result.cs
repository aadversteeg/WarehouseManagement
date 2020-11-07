using System;
using System.Collections.Generic;

namespace Core.Extensions
{
    public class Result
    {
        protected readonly bool _isSuccess;
        protected readonly IReadOnlyCollection<Error> _errors;

        protected Result()
        {
            _isSuccess = true;
        }

        protected Result(IReadOnlyCollection<Error> errors)
        {
            _isSuccess = false;
            _errors = errors;
        }

        public IReadOnlyCollection<Error> Errors
        {
            get
            {
                if(_isSuccess)
                {
                    throw new InvalidOperationException();
                }
                return _errors;
            }
        }

        public bool IsFailure
        {
            get
            {
                return !_isSuccess;
            }
        }

        public bool IsSuccess
        {
            get
            {
                return _isSuccess;
            }
        }

        public static Result Success()
        {
            return new Result();
        }

        public static Result Fail(IReadOnlyCollection<Error> errors)
        {
            return new Result(errors);
        }

        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value);
        }

        public static Result<TValue> Fail<TValue>(IReadOnlyCollection<Error> errors)
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
                if (!_isSuccess)
                {
                    throw new InvalidOperationException();
                }
                return _value;
            }
        }
    }
}
