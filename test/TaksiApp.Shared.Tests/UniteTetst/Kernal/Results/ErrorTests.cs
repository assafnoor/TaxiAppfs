using FluentAssertions;
using TaksiApp.Shared.Kernel.Results;
namespace TaksiApp.Shared.Tests.UniteTetst.Kernal.Results;


/// <summary>
/// Tests for the Error struct.
/// Covers all error types, metadata handling, equality, and factory methods.
/// </summary>
public class ErrorTests
{
    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange
        const string code = "Validation.Required";
        const string message = "Field is required";

        // Act
        var error = Error.Validation(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Validation);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Validation_WithMetadata_ShouldCreateValidationErrorWithMetadata()
    {
        // Arrange
        const string code = "Validation.Required";
        const string message = "Field is required";
        var metadata = new Dictionary<string, string>
        {
            ["field"] = "Email",
            ["attemptedValue"] = "invalid@"
        };

        // Act
        var error = Error.Validation(code, message, metadata);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Validation);
        error.Metadata.Should().NotBeNull();
        error.Metadata.Should().ContainKey("field");
        error.Metadata!["field"].Should().Be("Email");
        error.Metadata.Should().ContainKey("attemptedValue");
        error.Metadata["attemptedValue"].Should().Be("invalid@");
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange
        const string code = "Customer.NotFound";
        const string message = "Customer not found";

        // Act
        var error = Error.NotFound(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.NotFound);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange
        const string code = "User.EmailExists";
        const string message = "Email already exists";

        // Act
        var error = Error.Conflict(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Conflict);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailureError()
    {
        // Arrange
        const string code = "Database.Timeout";
        const string message = "Database operation timed out";

        // Act
        var error = Error.Failure(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Failure);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Arrange
        const string code = "Auth.InvalidToken";
        const string message = "Token is invalid or expired";

        // Act
        var error = Error.Unauthorized(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Unauthorized);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange
        const string code = "Auth.InsufficientPermissions";
        const string message = "User lacks required permissions";

        // Act
        var error = Error.Forbidden(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Forbidden);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void None_ShouldReturnPredefinedNoneError()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Code.Should().Be("Error.None");
        error.Message.Should().Be("No error");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void NullValue_ShouldReturnPredefinedNullValueError()
    {
        // Act
        var error = Error.NullValue;

        // Assert
        error.Code.Should().Be("Error.NullValue");
        error.Message.Should().Be("A required value was null");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Equals_WithSameCodeAndType_ShouldReturnTrue()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error", "Message 1");
        var error2 = Error.Validation("Test.Error", "Message 2"); // Different message

        // Act & Assert
        error1.Equals(error2).Should().BeTrue();
        (error1 == error2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error1", "Message");
        var error2 = Error.Validation("Test.Error2", "Message");

        // Act & Assert
        error1.Equals(error2).Should().BeFalse();
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error", "Message");
        var error2 = Error.NotFound("Test.Error", "Message"); // Same code, different type

        // Act & Assert
        error1.Equals(error2).Should().BeFalse();
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_ShouldWorkCorrectly()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error", "Message");
        object error2 = Error.Validation("Test.Error", "Message");
        object notAnError = "not an error";

        // Act & Assert
        error1.Equals(error2).Should().BeTrue();
        error1.Equals(notAnError).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameCodeAndType_ShouldReturnSameHashCode()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error", "Message 1");
        var error2 = Error.Validation("Test.Error", "Message 2");

        // Act
        var hash1 = error1.GetHashCode();
        var hash2 = error2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentCodeOrType_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error1", "Message");
        var error2 = Error.Validation("Test.Error2", "Message");
        var error3 = Error.NotFound("Test.Error1", "Message");

        // Act
        var hash1 = error1.GetHashCode();
        var hash2 = error2.GetHashCode();
        var hash3 = error3.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBe(hash3);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test message");

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be("[Validation] Test.Error: Test message");
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    public void ToString_ShouldIncludeErrorType(ErrorType errorType)
    {
        // Arrange
        var error = errorType switch
        {
            ErrorType.Validation => Error.Validation("Test.Code", "Message"),
            ErrorType.NotFound => Error.NotFound("Test.Code", "Message"),
            ErrorType.Conflict => Error.Conflict("Test.Code", "Message"),
            ErrorType.Failure => Error.Failure("Test.Code", "Message"),
            ErrorType.Unauthorized => Error.Unauthorized("Test.Code", "Message"),
            ErrorType.Forbidden => Error.Forbidden("Test.Code", "Message"),
            _ => throw new ArgumentOutOfRangeException(nameof(errorType))
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Contain(errorType.ToString());
    }

    [Fact]
    public void Metadata_ShouldBeReadOnly()
    {
        // Arrange
        var metadata = new Dictionary<string, string> { ["key"] = "value" };
        var error = Error.Validation("Test.Error", "Message", metadata);

        // Act
        var act = () => error.Metadata!.TryAdd("newKey", "newValue");

        // Assert
        act.Should().Throw<NotSupportedException>(); // ReadOnlyDictionary throws this
    }

    [Fact]
    public void Metadata_WithNullInput_ShouldHaveNullMetadata()
    {
        // Act
        var error = Error.Validation("Test.Error", "Message", metadata: null);

        // Assert
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Metadata_WithEmptyDictionary_ShouldHaveEmptyMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, string>();

        // Act
        var error = Error.Validation("Test.Error", "Message", metadata);

        // Assert
        error.Metadata.Should().NotBeNull();
        error.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void ErrorsInDictionary_ShouldWorkAsKeys()
    {
        // Arrange
        var error1 = Error.Validation("Test.Error", "Message");
        var error2 = Error.Validation("Test.Error", "Different message");
        var error3 = Error.NotFound("Test.Error", "Message");

        var dictionary = new Dictionary<Error, string>
        {
            [error1] = "First",
            [error3] = "Third"
        };

        // Act & Assert
        dictionary[error1].Should().Be("First");
        dictionary[error2].Should().Be("First"); // Same code and type as error1
        dictionary[error3].Should().Be("Third");
        dictionary.Should().ContainKey(error1);
        dictionary.Should().ContainKey(error2);
        dictionary.Should().ContainKey(error3);
    }
}