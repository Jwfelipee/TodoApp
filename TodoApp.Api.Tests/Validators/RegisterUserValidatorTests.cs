using FluentValidation.TestHelper;
using TodoApp.Api.DTOs;
using TodoApp.Api.Validators;

namespace TodoApp.Api.Tests.Validators;

public class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator;

    public RegisterUserValidatorTests()
    {
        _validator = new RegisterUserValidator();
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "joao@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveNameError()
    {
        // Arrange
        var dto = new RegisterUserDto("", "joao@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveEmailError()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "invalid-email", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "joao@example.com", "");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_WithPasswordLessThan6Characters_ShouldHavePasswordError()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "joao@example.com", "pass");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_WithPasswordExactly6Characters_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "joao@example.com", "pass12");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithAllFieldsEmpty_ShouldHaveMultipleErrors()
    {
        // Arrange
        var dto = new RegisterUserDto("", "", "");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("João Silva")]
    [InlineData("Maria Santos")]
    [InlineData("A")]
    public async Task Validate_WithDifferentValidNames_ShouldNotHaveNameErrors(string validName)
    {
        // Arrange
        var dto = new RegisterUserDto(validName, "user@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        var hasNameError = result.Errors.Any(e => e.PropertyName == "Name");
        Assert.False(hasNameError);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("pass123")]
    [InlineData("VeryLongPasswordWith123")]
    public async Task Validate_WithDifferentValidPasswords_ShouldNotHavePasswordErrors(string validPassword)
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "user@example.com", validPassword);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        var hasPasswordError = result.Errors.Any(e => e.PropertyName == "Password");
        Assert.False(hasPasswordError);
    }

    [Fact]
    public async Task Validate_WithPasswordExactly5Characters_ShouldHavePasswordError()
    {
        // Arrange
        var dto = new RegisterUserDto("João Silva", "user@example.com", "pass1");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }
}
