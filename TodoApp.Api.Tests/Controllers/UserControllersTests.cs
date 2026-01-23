using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs;
using TodoApp.Application.Services;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using Xunit.Abstractions;

namespace TodoApp.Api.Tests.Controllers;

public class UserControllers
{
    private readonly UserService _userService;
    private readonly UserController _userController;
    private readonly IUserRepository _userRepository;
    private readonly BcryptPasswordHasher _bcryptPasswordHasher = new();
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly ITestOutputHelper _testOutputHelper;

    public UserControllers(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _registerValidator = Substitute.For<IValidator<RegisterUserDto>>();
        _loginValidator = Substitute.For<IValidator<LoginUserDto>>();

        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepository, _bcryptPasswordHasher);
        _userController = new UserController(_userService, _registerValidator, _loginValidator);
    }

    [Fact]
    public async Task Register_GivenInvalidParams_ShouldReturnBadRequest()
    {
        string name = "";
        string email = "example@email.com";
        string password = "pass123";
        RegisterUserDto invalidUser = new(name, email, password);

        List<ValidationFailure> lala =
        [
            new ValidationFailure("Nome", "Nome é obrigatório")
        ];

        _registerValidator.ValidateAsync(invalidUser).Returns(new ValidationResult(lala));

        IActionResult result = await _userController.Register(invalidUser);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_GivenAlreadyExistengUser_ShouldReturnBadRequest()
    {
        string name = "John Doe";
        string email = "example@email.com";
        string password = "pass123";
        RegisterUserDto registerUser = new(name, email, password);
        User savedUser = new(name, email, password);

        _registerValidator.ValidateAsync(registerUser).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).Returns(savedUser);

        IActionResult result = await _userController.Register(registerUser);
        BadRequestObjectResult badRequestObject = (BadRequestObjectResult)result;
        var errorMessage = badRequestObject.Value;

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already in use", errorMessage);
    }

    [Fact]
    public async Task Register_GivenValidParams_ShouldBeReturnCreated()
    {
        string name = "John Doe";
        string email = "example@email.com";
        string password = "pass123";
        RegisterUserDto registerUser = new(name, email, password);

        _registerValidator.ValidateAsync(registerUser).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).ReturnsNull();

        IActionResult result = await _userController.Register(registerUser);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    // Inicio dos testes com AI
    #region Login Tests

    [Fact]
    public async Task Login_GivenInvalidValidation_ShouldReturnBadRequest()
    {
        // Arrange
        string email = "";
        string password = "pass123";
        LoginUserDto loginDto = new(email, password);

        List<ValidationFailure> validationFailures =
        [
            new ValidationFailure("Email", "Email é obrigatório")
        ];

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult(validationFailures));

        // Act
        IActionResult result = await _userController.Login(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_GivenMultipleValidationErrors_ShouldReturnBadRequest()
    {
        // Arrange
        string email = "";
        string password = "";
        LoginUserDto loginDto = new(email, password);

        List<ValidationFailure> validationFailures =
        [
            new ValidationFailure("Email", "Email é obrigatório"),
            new ValidationFailure("Password", "Senha é obrigatória")
        ];

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult(validationFailures));

        // Act
        IActionResult result = await _userController.Login(loginDto);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Login_GivenValidValidation_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        string email = "notfound@email.com";
        string password = "password123";
        LoginUserDto loginDto = new(email, password);

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).ReturnsNull();

        // Act
        IActionResult result = await _userController.Login(loginDto);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_GivenValidValidation_WhenPasswordIsIncorrect_ShouldReturnUnauthorized()
    {
        // Arrange
        string email = "user@email.com";
        string correctPassword = "correctPassword123";
        string incorrectPassword = "wrongPassword123";
        LoginUserDto loginDto = new(email, incorrectPassword);

        string corretPasswordHashed = await _bcryptPasswordHasher.HashPasswordAsync(correctPassword);
        User user = new("John Doe", email, corretPasswordHashed);

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).Returns(user);

        // Act
        IActionResult result = await _userController.Login(loginDto);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_GivenValidCredentials_ShouldReturnOkWithUserData()
    {
        // Arrange
        string email = "user@email.com";
        string password = "password123";
        string name = "John Doe";
        LoginUserDto loginDto = new(email, password);

        string hashedPassword = await _bcryptPasswordHasher.HashPasswordAsync(password);
        User user = new(name, email, hashedPassword);

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).Returns(user);

        // Act
        IActionResult result = await _userController.Login(loginDto);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var responseData = okResult.Value;
        Assert.NotNull(responseData);

        var userIdProperty = responseData.GetType().GetProperty("userId");
        var nameProperty = responseData.GetType().GetProperty("name");
        var emailProperty = responseData.GetType().GetProperty("email");

        Assert.NotNull(userIdProperty);
        Assert.NotNull(nameProperty);
        Assert.NotNull(emailProperty);

        var userId = userIdProperty.GetValue(responseData);
        var returnedName = nameProperty.GetValue(responseData);
        var returnedEmail = emailProperty.GetValue(responseData);

        Assert.Equal(user.Id, (Guid)userId!);
        Assert.Equal(name, (string)returnedName!);
        Assert.Equal(email, (string)returnedEmail!);
    }

    [Fact]
    public async Task Login_ValidateIsCalledWithCorrectDto()
    {
        // Arrange
        string email = "test@email.com";
        string password = "testPassword123";
        LoginUserDto loginDto = new(email, password);

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).ReturnsNull();

        // Act
        await _userController.Login(loginDto);

        // Assert
        await _loginValidator.Received(1).ValidateAsync(loginDto);
    }

    [Fact]
    public async Task Login_WhenUserExists_ShouldQueryRepositoryWithCorrectEmail()
    {
        // Arrange
        string email = "test@email.com";
        string password = "testPassword123";
        LoginUserDto loginDto = new(email, password);

        _loginValidator.ValidateAsync(loginDto).Returns(new ValidationResult());
        _userRepository.GetByEmailAsync(email).ReturnsNull();

        // Act
        await _userController.Login(loginDto);

        // Assert
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    #endregion
}
