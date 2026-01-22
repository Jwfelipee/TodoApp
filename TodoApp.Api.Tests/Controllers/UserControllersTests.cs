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
        User savedUser = new (name, email, password);

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
}
