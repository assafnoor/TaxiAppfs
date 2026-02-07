using FluentAssertions;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Tests.UniteTetst.Kernal.Results;

/// <summary>
/// Tests for the Result type (non-generic).
/// Covers success/failure states, pattern matching, and error handling.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Error_WhenAccessedOnSuccessResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Error on success result");
    }

    [Fact]
    public void Match_WithSuccessResult_ShouldExecuteOnSuccessFunc()
    {
        // Arrange
        var result = Result.Success();
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: () => { onSuccessCalled = true; return "success"; },
            onFailure: _ => { onFailureCalled = true; return "failure"; });

        // Assert
        onSuccessCalled.Should().BeTrue();
        onFailureCalled.Should().BeFalse();
        output.Should().Be("success");
    }

    [Fact]
    public void Match_WithFailureResult_ShouldExecuteOnFailureFunc()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result.Failure(error);
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        var output = result.Match(
            onSuccess: () => { onSuccessCalled = true; return "success"; },
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
        var result = Result.Success();
        var onSuccessCalled = false;
        var onFailureCalled = false;

        // Act
        result.Match(
            onSuccess: () => onSuccessCalled = true,
            onFailure: _ => onFailureCalled = true);

        // Assert
        onSuccessCalled.Should().BeTrue();
        onFailureCalled.Should().BeFalse();
    }

    [Fact]
    public void Tap_WithSuccessResult_ShouldExecuteAction()
    {
        // Arrange
        var result = Result.Success();
        var actionExecuted = false;

        // Act
        var returnedResult = result.Tap(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().Be(result);
    }

    [Fact]
    public void Tap_WithFailureResult_ShouldNotExecuteAction()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result.Failure(error);
        var actionExecuted = false;

        // Act
        var returnedResult = result.Tap(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        returnedResult.Should().Be(result);
    }

    [Fact]
    public async Task TapAsync_WithSuccessResult_ShouldExecuteAsyncAction()
    {
        // Arrange
        var result = Result.Success();
        var actionExecuted = false;

        // Act
        var returnedResult = await result.TapAsync(async () =>
        {
            await Task.Delay(1);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        returnedResult.Should().Be(result);
    }

    [Fact]
    public async Task TapAsync_WithFailureResult_ShouldNotExecuteAsyncAction()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");
        var result = Result.Failure(error);
        var actionExecuted = false;

        // Act
        var returnedResult = await result.TapAsync(async () =>
        {
            await Task.Delay(1);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        returnedResult.Should().Be(result);
    }

    [Fact]
    public void Ensure_WithSuccessResultAndTruePredicate_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result.Success();
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var ensuredResult = result.Ensure(() => true, error);

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WithSuccessResultAndFalsePredicate_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Success();
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var ensuredResult = result.Ensure(() => false, error);

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.Error.Should().Be(error);
    }

    [Fact]
    public void Ensure_WithFailureResult_ShouldReturnOriginalFailure()
    {
        // Arrange
        var originalError = Error.Validation("Original.Error", "Original message");
        var result = Result.Failure(originalError);
        var newError = Error.Validation("New.Error", "New message");

        // Act
        var ensuredResult = result.Ensure(() => true, newError);

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.Error.Should().Be(originalError);
    }

    [Fact]
    public void Success_Generic_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Failure_Generic_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}