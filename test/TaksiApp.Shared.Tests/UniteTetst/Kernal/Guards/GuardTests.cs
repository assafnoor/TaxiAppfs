using FluentAssertions;
using TaksiApp.Shared.Kernel.Guards;


/// <summary>
/// Tests for the Guard static class.
/// Covers all guard clause methods and edge cases.
/// </summary>
public class GuardTests
{
    #region Require<TException> Tests

    [Fact]
    public void Require_WithTrueCondition_ShouldNotThrow()
    {
        // Act
        var act = () => Guard.Require<ArgumentException>(true, "Should not throw");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Require_WithFalseCondition_ShouldThrowSpecifiedException()
    {
        // Act
        var act = () => Guard.Require<ArgumentException>(false, "Test message");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Test message");
    }

    [Fact]
    public void Require_WithInvalidOperationException_ShouldThrowCorrectException()
    {
        // Act
        var act = () => Guard.Require<InvalidOperationException>(false, "Operation failed");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed");
    }

    [Fact]
    public void Require_WithCustomExceptionType_ShouldThrowCustomException()
    {
        // Act
        var act = () => Guard.Require<InvalidDataException>(false, "Data is invalid");

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("Data is invalid");
    }

    #endregion

    #region NotNull Tests

    [Fact]
    public void NotNull_WithNonNullValue_ShouldNotThrow()
    {
        // Arrange
        var value = "test";

        // Act
        var act = () => Guard.NotNull(value);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotNull_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Guard.NotNull(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotNull_ShouldCaptureParameterName()
    {
        // Arrange
        string? customer = null;

        // Act
        var act = () => Guard.NotNull(customer);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("customer");
    }

    [Fact]
    public void NotNull_WithCustomParameterName_ShouldUseCustomName()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Guard.NotNull(value, "customName");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("customName");
    }

    [Fact]
    public void NotNull_WithComplexObject_ShouldWork()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        var act = () => Guard.NotNull(obj);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region NotNullOrEmpty Tests

    [Theory]
    [InlineData("test")]
    [InlineData("a")]
    [InlineData("  test  ")] // Contains non-whitespace
    public void NotNullOrEmpty_WithValidString_ShouldNotThrow(string value)
    {
        // Act
        var act = () => Guard.NotNullOrEmpty(value);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void NotNullOrEmpty_WithNullOrWhitespace_ShouldThrowArgumentException(string? value)
    {
        // Act
        var act = () => Guard.NotNullOrEmpty(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null, empty, or whitespace*");
    }

    [Fact]
    public void NotNullOrEmpty_ShouldCaptureParameterName()
    {
        // Arrange
        string? name = "";

        // Act
        var act = () => Guard.NotNullOrEmpty(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("name");
    }

    #endregion

    #region GreaterThan Tests

    [Theory]
    [InlineData(10, 5)]
    [InlineData(1, 0)]
    [InlineData(0, -1)]
    [InlineData(100, 99)]
    public void GreaterThan_WithValueGreaterThanMin_ShouldNotThrow(int value, int min)
    {
        // Act
        var act = () => Guard.GreaterThan(value, min);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void GreaterThan_WithValueEqualToMin_ShouldThrowArgumentOutOfRangeException(int value, int min)
    {
        // Act
        var act = () => Guard.GreaterThan(value, min);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than*");
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(0, 1)]
    [InlineData(-1, 0)]
    public void GreaterThan_WithValueLessThanMin_ShouldThrowArgumentOutOfRangeException(int value, int min)
    {
        // Act
        var act = () => Guard.GreaterThan(value, min);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than*");
    }

    [Fact]
    public void GreaterThan_WithDecimal_ShouldWork()
    {
        // Arrange
        decimal value = 10.5m;
        decimal min = 10.0m;

        // Act
        var act = () => Guard.GreaterThan(value, min);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GreaterThan_WithDateTime_ShouldWork()
    {
        // Arrange
        var value = DateTime.Now;
        var min = DateTime.Now.AddDays(-1);

        // Act
        var act = () => Guard.GreaterThan(value, min);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region GreaterThanOrEqual Tests

    [Theory]
    [InlineData(10, 5)]
    [InlineData(5, 5)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    public void GreaterThanOrEqual_WithValueGreaterThanOrEqualToMin_ShouldNotThrow(int value, int min)
    {
        // Act
        var act = () => Guard.GreaterThanOrEqual(value, min);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(0, 1)]
    [InlineData(-1, 0)]
    public void GreaterThanOrEqual_WithValueLessThanMin_ShouldThrowArgumentOutOfRangeException(int value, int min)
    {
        // Act
        var act = () => Guard.GreaterThanOrEqual(value, min);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than or equal to*");
    }

    [Fact]
    public void GreaterThanOrEqual_ShouldCaptureParameterName()
    {
        // Arrange
        int count = -1;

        // Act
        var act = () => Guard.GreaterThanOrEqual(count, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("count");
    }

    #endregion

    #region InRange Tests

    [Theory]
    [InlineData(5, 0, 10)]
    [InlineData(0, 0, 10)]
    [InlineData(10, 0, 10)]
    [InlineData(50, 0, 100)]
    public void InRange_WithValueInRange_ShouldNotThrow(int value, int min, int max)
    {
        // Act
        var act = () => Guard.InRange(value, min, max);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-1, 0, 10)]
    [InlineData(11, 0, 10)]
    [InlineData(101, 0, 100)]
    public void InRange_WithValueOutOfRange_ShouldThrowArgumentOutOfRangeException(int value, int min, int max)
    {
        // Act
        var act = () => Guard.InRange(value, min, max);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage($"*must be between {min} and {max}*");
    }

    [Fact]
    public void InRange_WithDecimal_ShouldWork()
    {
        // Arrange
        decimal value = 50.5m;
        decimal min = 0m;
        decimal max = 100m;

        // Act
        var act = () => Guard.InRange(value, min, max);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithDouble_ShouldWork()
    {
        // Arrange
        double value = 50.5;
        double min = 0;
        double max = 100;

        // Act
        var act = () => Guard.InRange(value, min, max);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_ShouldCaptureParameterName()
    {
        // Arrange
        int age = 150;

        // Act
        var act = () => Guard.InRange(age, 0, 120);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("age");
    }

    [Fact]
    public void InRange_WithNegativeRange_ShouldWork()
    {
        // Arrange
        int value = -5;
        int min = -10;
        int max = 0;

        // Act
        var act = () => Guard.InRange(value, min, max);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Guards_InMethodValidation_ShouldWorkTogether()
    {
        // Arrange
        string? name = null;
        int age = -1;

        // Act & Assert
        var actName = () => Guard.NotNullOrEmpty(name);
        actName.Should().Throw<ArgumentException>();

        var actAge = () => Guard.GreaterThanOrEqual(age, 0);
        actAge.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Guards_WithValidInput_ShouldAllowMethodExecution()
    {
        // Arrange
        string name = "Test User";
        int age = 25;
        decimal balance = 1000.50m;

        // Act
        var act = () =>
        {
            Guard.NotNullOrEmpty(name);
            Guard.InRange(age, 0, 120);
            Guard.GreaterThanOrEqual(balance, 0);
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}