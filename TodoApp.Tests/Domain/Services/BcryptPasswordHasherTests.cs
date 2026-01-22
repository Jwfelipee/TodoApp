using TodoApp.Application.Services;
using Xunit.Abstractions;

namespace TodoApp.Tests.Domain.Services
{
    [Trait("category", "ServicesBcrypPasswordHasher")]
    public class TodoAppTestsDomainServicesBcryptPasswordHasherTests(ITestOutputHelper testOutputHelper)
    {
        private readonly BcryptPasswordHasher _bcryptPasswordHasher = new();
        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

        [Fact]
        public async Task Hash_GivenStringParam_ShouldBeHashTheParam()
        {
            string password = "pass123";

            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(password);

            Assert.NotEqual(password, passwordHash);
        }

        [Fact]
        public async Task Hash_GivenEmptyParam_ShouldThrowException()
        {
            string emptyPassword = "";

            var exception = await Assert.ThrowsAsync<Exception>(() => _bcryptPasswordHasher.HashPasswordAsync(emptyPassword));
            
            Assert.Equal("Password can't be empty", exception.Message);
        }

        [Fact]
        public async Task VerifyPassword_GivenRawPasswordAndHashIt_ShouldBeReturnTrue()
        {
            string rawPassword = "pass123";
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(rawPassword);

            bool verifiedPasswords = await _bcryptPasswordHasher.VerifyPasswordAsync(rawPassword, passwordHash);

            Assert.True(verifiedPasswords);
        }

        [Fact]
        public async Task VerifyPassword_GivenDifferenteRawPasswordAndVerifyHash_ShouldBeReturnFalse()
        {
            string rawCorrectPassword = "pass123";
            string correctPasswordHash = await _bcryptPasswordHasher.HashPasswordAsync(rawCorrectPassword);
            string rawIncorrectPassword = "pass123456";

            bool verifiedPasswords = await _bcryptPasswordHasher.VerifyPasswordAsync(rawIncorrectPassword, correctPasswordHash);

            Assert.False(verifiedPasswords);
        }

        [Fact]
        public async Task VerifyPassword_GivenEmptyPasswordHash_ShouldThrowException()
        {
            string rawPassword = "pass123";
            string passwordHash = "";

            var exception = await Assert.ThrowsAsync<Exception>(() => _bcryptPasswordHasher.VerifyPasswordAsync(rawPassword, passwordHash));

            Assert.Equal("Password Hash can't be empty", exception.Message);
        }

        [Fact]
        public async Task VerifyPassword_GivenEmptyRawPassword_ShouldThrowException()
        {
            string rawPassword = "";
            string passwordHash = "Hash123";

            var exception = await Assert.ThrowsAsync<Exception>(() => _bcryptPasswordHasher.VerifyPasswordAsync(rawPassword, passwordHash));

            Assert.Equal("Password can't be empty", exception.Message);
        }
    }
}
