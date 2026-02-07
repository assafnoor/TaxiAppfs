using FluentAssertions;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Tests.UniteTetst.Kernal.Results;

/// <summary>
/// Tests for the generic Result&lt;T&gt; type.
/// Covers value handling, monadic operations (Bind, Map), and conversions.
/// </summary>
public class ResultOfTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        const string expectedValue = "test value";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "Resource not found");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_WhenAccessedOnFailureResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result<string>.Failure(error);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value on failure result");
    }

    [Fact]
    public void Error_WhenAccessedOnSuccessResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Success("test");

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Error on success result");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Match_WithSuccessResult_ShouldExecuteOnSuccessFunc()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: value => { onSuccessCalled = true; return $"value: {value}"; },
            onFailure: _ => { onFailureCalled = true; return "error"; });

        // Assert
        onSuccessCalled.Should().BeTrue();
        onFailureCalled.Should().BeFalse();
        output.Should().Be("value: 42");
    }

    [Fact]
    public void Match_WithFailureResult_ShouldExecuteOnFailureFunc()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result<int>.Failure(error);
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: value => { onSuccessCalled = true; return $"value: {value}"; },
            onFailure: e => { onFailureCalled = true; return $"error: {e.Code}"; });

        // Assert
        onSuccessCalled.Should().BeFalse();
        onFailureCalled.Should().BeTrue();
        output.Should().Be("error: Test.Error");
    }

    [Fact]
    public void Match_WithActions_WithSuccessResult_ShouldExecuteOnSuccessAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var capturedValue = 0;
        var onFailureCalled = false;

        // Act
        result.Match(
            onSuccess: value => capturedValue = value,
            onFailure: _ => onFailureCalled = true);

        // Assert
        capturedValue.Should().Be(42);
        onFailureCalled.Should().BeFalse();
    }

    [Fact]
    public void Bind_WithSuccessResult_ShouldExecuteFuncAndReturnNewResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var boundResult = result.Bind(value => Result<string>.Success($"Value: {value}"));

        // Assert
        boundResult.IsSuccess.Should().BeTrue();
        boundResult.Value.Should().Be("Value: 42");
    }

    [Fact]
    public void Bind_WithSuccessResultReturningFailure_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var boundResult = result.Bind(_ => Result<string>.Failure(error));

        // Assert
        boundResult.IsFailure.Should().BeTrue();
        boundResult.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_WithFailureResult_ShouldNotExecuteFuncAndPropagateError()
    {
        // Arrange
        var error = Error.Validation("Original.Error", "Original message");
        var result = Result<int>.Failure(error);
        var funcExecuted = false;

        // Act
        var boundResult = result.Bind(value =>
        {
            funcExecuted = true;
            return Result<string>.Success($"Value: {value}");
        });

        // Assert
        funcExecuted.Should().BeFalse();
        boundResult.IsFailure.Should().BeTrue();
        boundResult.Error.Should().Be(error);
    }

    [Fact]
    public void Map_WithSuccessResult_ShouldTransformValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mappedResult = result.Map(value => $"Value: {value}");

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be("Value: 42");
    }

    [Fact]
    public void Map_WithFailureResult_ShouldNotExecuteFuncAndPropagateError()
    {
        // Arrange
        var error = Error.Validation("Original.Error", "Original message");
        var result = Result<int>.Failure(error);
        var funcExecuted = false;

        // Act
        var mappedResult = result.Map(value =>
        {
            funcExecuted = true;
            return $"Value: {value}";
        });

        // Assert
        funcExecuted.Should().BeFalse();
        mappedResult.IsFailure.Should().BeTrue();
        mappedResult.Error.Should().Be(error);
    }

    [Fact]
    public void Tap_WithSuccessResult_ShouldExecuteActionAndReturnSameResult()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var capturedValue = 0;

        // Act
        var returnedResult = result.Tap(value => capturedValue = value);

        // Assert
        capturedValue.Should().Be(42);
        returnedResult.Should().Be(result);
        returnedResult.IsSuccess.Should().BeTrue();
        returnedResult.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_WithFailureResult_ShouldNotExecuteAction()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result<int>.Failure(error);
        var actionExecuted = false;

        // Act
        var returnedResult = result.Tap(_ => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        returnedResult.Should().Be(result);
    }

    [Fact]
    public void Ensure_WithSuccessResultAndTruePredicate_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var ensuredResult = result.Ensure(value => value > 0, error);

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
        ensuredResult.Value.Should().Be(42);
    }

    [Fact]
    public void Ensure_WithSuccessResultAndFalsePredicate_ShouldReturnFailure()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var error = Error.Validation("Test.Error", "Value must be negative");

        // Act
        var ensuredResult = result.Ensure(value => value < 0, error);

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.Error.Should().Be(error);
    }

    [Fact]
    public void Ensure_WithFailureResult_ShouldReturnOriginalFailure()
    {
        // Arrange
        var originalError = Error.Validation("Original.Error", "Original message");
        var result = Result<int>.Failure(originalError);
        var newError = Error.Validation("New.Error", "New message");

        // Act
        var ensuredResult = result.Ensure(_ => true, newError);

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.Error.Should().Be(originalError);
    }

    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(0, 5, 5)]
    [InlineData(-10, 10, 0)]
    public void MonadicChaining_WithSuccessResults_ShouldComposeOperations(int a, int b, int expected)
    {
        // Arrange & Act
        var result = Result<int>.Success(a)
            .Map(x => x + b)
            .Bind(x => Result<int>.Success(x))
            .Ensure(x => x >= 0, Error.Validation("Test.Negative", "Value must be non-negative"));

        // Assert
        if (expected >= 0)
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expected);
        }
        else
        {
            result.IsFailure.Should().BeTrue();
        }
    }

    [Fact]
    public void ComplexMonadicChain_WithFailureInMiddle_ShouldPropagateError()
    {
        // Arrange
        var error = Error.Validation("Division.ByZero", "Cannot divide by zero");

        // Act
        var result = Result<int>.Success(42)
            .Map(x => x * 2) // 84
            .Bind(x => Result<int>.Failure(error)) // Fails here
            .Map(x => x + 10) // Should not execute
            .Tap(x => throw new InvalidOperationException("Should not reach here")); // Should not execute

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}