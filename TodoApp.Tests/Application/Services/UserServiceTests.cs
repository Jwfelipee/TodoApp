using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TodoApp.Application.DTOs;
using TodoApp.Application.Services;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using Xunit.Abstractions;

namespace TodoApp.Tests.Application.Services
{
    public class UserServiceTests
    {
        private readonly BcryptPasswordHasher _bcryptPasswordHasher = new();
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly UserService _userService;
        private readonly IUserRepository _userRepository;

        public UserServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _userRepository = Substitute.For<IUserRepository>();
            _userService = new UserService(_userRepository, _bcryptPasswordHasher);
        }

        [Fact]
        public async Task Add_GivenAlreadyExistentUser_ShouldBeThrowException()
        {
            CreateUserDto userDto = new("John Doe", "example@email.com", "pass123");
            User user = new("John Doe", "example@email.com", "pass123");
            
            _userRepository.GetByEmailAsync(userDto.Email).Returns(user);

            var exception = await Assert.ThrowsAsync<Exception>(() => _userService.AddAsync(userDto));

            Assert.Equal("Email already in use", exception.Message);
        }

        [Fact]
        public async Task Add_GivenNewUser_ShouldBeAddUser()
        {
            CreateUserDto userDto = new("John Doe", "example@email.com", "pass123");

            _userRepository.GetByEmailAsync(userDto.Email).ReturnsNull();

            Guid newUserId = await _userService.AddAsync(userDto);

            Assert.IsType<Guid>(newUserId);
        }

        [Fact]
        public async Task Login_GivenIncorrectEmail_ShouldBeThrowExpection()
        {
            string email = "example@email.com";
            string password = "pass123";

            _userRepository.GetByEmailAsync(email).ReturnsNull();

            var exception = await Assert.ThrowsAsync<Exception>(() => _userService.LoginAsync(email, password));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Login_GivenIncorrectPassword_ShouldBeThrowExpection()
        {
            string name = "John Doe";
            string email = "example@email.com";
            string rawPassword = "pass123";
            string password = await _bcryptPasswordHasher.HashPasswordAsync(rawPassword);
            User user = new(name, email, password);
            string incorrectPassword = "pass123456";

            _userRepository.GetByEmailAsync(email).Returns(user);

            var exception = await Assert.ThrowsAsync<Exception>(() => _userService.LoginAsync(email, incorrectPassword));

            Assert.Equal("Usuário ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Login_GivenCorrectParams_ShouldBeReturnUser()
        {
            string name = "John Doe";
            string email = "example@email.com";
            string rawPassword = "pass123";

            string password = await _bcryptPasswordHasher.HashPasswordAsync(rawPassword);
            User user = new(name, email, password);
            _userRepository.GetByEmailAsync(email).Returns(user);

            User userLogged = await _userService.LoginAsync(email, rawPassword);

            Assert.IsType<User>(userLogged);
            Assert.Equal(name, userLogged.Name);
            Assert.Equal(email, userLogged.Email);
            Assert.Equal(password, userLogged.PasswordHash);
        }
    }
}