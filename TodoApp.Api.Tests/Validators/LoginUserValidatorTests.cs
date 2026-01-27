using FluentValidation.TestHelper;
using TodoApp.Api.DTOs;
using TodoApp.Api.Validators;

namespace TodoApp.Api.Tests.Validators;

public class LoginUserValidatorTests
{
    private readonly LoginUserValidator _validator;

    public LoginUserValidatorTests()
    {
        _validator = new LoginUserValidator();
    }

    [Fact]
    public async Task Validate_WithValidEmailAndPassword_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new LoginUserDto("user@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveEmailError()
    {
        // Arrange
        var dto = new LoginUserDto("invalid-email", "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        // Arrange
        var dto = new LoginUserDto("", "password123");

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
        var dto = new LoginUserDto("user@example.com", "");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_WithEmptyEmailAndPassword_ShouldHaveErrors()
    {
        // Arrange
        var dto = new LoginUserDto("", "");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("test@domain.com")]
    [InlineData("user.name@company.co.uk")]
    [InlineData("valid.email+tag@example.org")]
    public async Task Validate_WithDifferentValidEmails_ShouldNotHaveErrors(string validEmail)
    {
        // Arrange
        var dto = new LoginUserDto(validEmail, "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    public async Task Validate_WithDifferentInvalidEmails_ShouldHaveErrors(string invalidEmail)
    {
        // Arrange
        var dto = new LoginUserDto(invalidEmail, "password123");

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }
}
