using Core.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Extensions
{
    public class ResultTests
    {
        [Fact(DisplayName = "R-001: Calling Result.Success should return a success.")]
        public void R001()
        {
            // arrange, act
            var result = Result.Success();

            // assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Should().BeOfType<Result>();
        }

        [Fact(DisplayName = "R-002: Calling Result.Fail should return a failure with specified errors.")]
        public void R002()
        {
            // arrange, act
            var result = Result.Fail( new[] { new Error("ERR", "Failure reason") });

            // assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().BeEquivalentTo(new[] { new Error("ERR", "Failure reason") });
            result.Should().BeOfType<Result>();
        }

        [Fact(DisplayName = "R-003: Getting errors of a success result is an invalid operation.")]
        public void R003()
        {
            // arrange, act
            var result = Result.Success();

            IReadOnlyCollection<Error> errors;
            Action action = () => errors = result.Errors;

            // assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact(DisplayName = "R-001: Calling Result.Success should return a success with specified value.")]
        public void R004()
        {
            // arrange, act
            var result = Result.Success("Some Value");

            // assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Value.Should().Be("Some Value");
            result.Should().BeOfType<Result<string>>();
        }

        [Fact(DisplayName = "R-005: Getting value of a failure result is an invalid operation.")]
        public void R005()
        {
            // arrange, act
            var result = Result.Fail<string>(new[] { new Error("ERR", "Failure reason") });

            string value;
            Action action = () => value = result.Value;

            // assert
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
