using Bogus;
using TodoApp.Application.Services;

namespace TodoApp.Tests.Domain.Services
{
    public class TodoAppTestsDomainServicesBcryptPasswordHasherTest
    {
        private readonly BcryptPasswordHasher _bcryptPasswordHasher;
        private readonly Faker _faker;

        public TodoAppTestsDomainServicesBcryptPasswordHasherTest()
        {
            _bcryptPasswordHasher = new BcryptPasswordHasher();
            _faker = new Faker();
        }

        [Fact]
        public async Task Hash_GivenStringParam_ShouldBeHashTheParam()
        {
            string password = _faker.Internet.Password();

            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(password);

            Assert.NotEqual(password, passwordHash);
        }
    }
}
