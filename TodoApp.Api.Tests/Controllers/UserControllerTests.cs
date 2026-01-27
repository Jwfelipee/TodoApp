using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Newtonsoft.Json;
using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs;
using TodoApp.Application.DTOs;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain;

namespace TodoApp.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userService = Substitute.For<IUserService>();
        _registerValidator = Substitute.For<IValidator<RegisterUserDto>>();
        _loginValidator = Substitute.For<IValidator<LoginUserDto>>();
        _controller = new UserController(_userService, _registerValidator, _loginValidator);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var registerDto = new RegisterUserDto("João Silva", "joao@example.com", "password123");
        var userId = Guid.NewGuid();

        _registerValidator.ValidateAsync(registerDto)
            .Returns(new ValidationResult());

        _userService.AddAsync(Arg.Any<CreateUserDto>())
            .Returns(userId);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UserController.Register), createdResult.ActionName);
        Assert.NotNull(createdResult.Value);

        await _userService.Received(1).AddAsync(Arg.Is<CreateUserDto>(x =>
            x.Name == registerDto.Name &&
            x.Email == registerDto.Email &&
            x.Password == registerDto.Password
        ));
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterUserDto("", "invalid-email", "");
        var validationErrors = new List<ValidationFailure>
        {
            new("Name", "O nome é obrigatório."),
            new("Email", "Email inválido."),
            new("Password", "A senha deve ter no mínimo 6 caracteres.")
        };

        _registerValidator.ValidateAsync(registerDto)
            .Returns(new ValidationResult(validationErrors));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);

        await _userService.DidNotReceive().AddAsync(Arg.Any<CreateUserDto>());
    }

    [Fact]
    public async Task Register_WithServiceException_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterUserDto("João Silva", "joao@example.com", "password123");
        var exceptionMessage = "Email já cadastrado no sistema";

        _registerValidator.ValidateAsync(registerDto)
            .Returns(new ValidationResult());

        _userService.AddAsync(Arg.Any<CreateUserDto>())
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCallUserServiceAddAsync()
    {
        // Arrange
        var registerDto = new RegisterUserDto("João Silva", "joao@example.com", "password123");

        _registerValidator.ValidateAsync(registerDto)
            .Returns(new ValidationResult());

        _userService.AddAsync(Arg.Any<CreateUserDto>())
            .Returns(Guid.NewGuid());

        // Act
        await _controller.Register(registerDto);

        // Assert
        await _userService.Received(1).AddAsync(Arg.Any<CreateUserDto>());
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidData_ShouldReturnOkWithUserInfo()
    {
        // Arrange
        var loginDto = new LoginUserDto("joao@example.com", "password123");
        var user = new User("João Silva", "joao@example.com", "hashedpassword");

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult());

        _userService.LoginAsync("joao@example.com", "password123")
            .Returns(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        await _userService.Received(1).LoginAsync("joao@example.com", "password123");
    }

    [Fact]
    public async Task Login_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginUserDto("invalid-email", "");
        var validationErrors = new List<ValidationFailure>
        {
            new("Email", "Email inválido."),
            new("Password", "A senha é obrigatória.")
        };

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult(validationErrors));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);

        await _userService.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Login_WithUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginUserDto("nonexistent@example.com", "password123");

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult());

        _userService.LoginAsync("nonexistent@example.com", "password123")
            .ThrowsAsync(new Exception("Usuário não encontrado"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginUserDto("joao@example.com", "wrongpassword");

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult());

        _userService.LoginAsync("joao@example.com", "wrongpassword")
            .ThrowsAsync(new Exception("Senha incorreta"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithValidData_ShouldCallUserServiceLoginAsync()
    {
        // Arrange
        var loginDto = new LoginUserDto("joao@example.com", "password123");
        var user = new User("João Silva", "joao@example.com", "hashedpassword");

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult());

        _userService.LoginAsync("joao@example.com", "password123")
            .Returns(user);

        // Act
        await _controller.Login(loginDto);

        // Assert
        await _userService.Received(1).LoginAsync("joao@example.com", "password123");
    }

    [Fact]
    public async Task Login_WithValidData_ReturnedResponseShouldContainUserData()
    {
        // Arrange
        var loginDto = new LoginUserDto("joao@example.com", "password123");
        var user = new User("João Silva", "joao@example.com", "hashedpassword");

        _loginValidator.ValidateAsync(loginDto)
            .Returns(new ValidationResult());

        _userService.LoginAsync("joao@example.com", "password123")
            .Returns(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Convert the anonymous object to JSON and back to a dictionary for easier assertions
        var json = JsonConvert.SerializeObject(okResult.Value);
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        Assert.NotNull(responseDict);
        Assert.True(responseDict!.ContainsKey("userId"));
        Assert.True(responseDict.ContainsKey("name"));
        Assert.True(responseDict.ContainsKey("email"));
        Assert.True(responseDict.ContainsKey("message"));
    }

    #endregion
}
