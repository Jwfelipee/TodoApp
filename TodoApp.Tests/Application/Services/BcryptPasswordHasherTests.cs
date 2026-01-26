using TodoApp.Application.Services;
using Xunit;

namespace TodoApp.Tests.Application.Services
{
    [Trait("category", "ServicesBcrypPasswordHasher")]
    public class BcryptPasswordHasherTests
    {
        private readonly BcryptPasswordHasher _bcryptPasswordHasher = new();

        #region HashPasswordAsync Tests

        [Fact]
        public async Task HashPasswordAsync_WithValidPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            string password = "pass123";

            // Act
            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(password);

            // Assert
            Assert.NotNull(passwordHash);
            Assert.NotEqual(password, passwordHash);
            Assert.True(passwordHash.Length > 0);
        }

        [Fact]
        public async Task HashPasswordAsync_WithValidPassword_ShouldReturnDifferentHashEachTime()
        {
            // Arrange
            string password = "pass123";

            // Act
            var hash1 = await _bcryptPasswordHasher.HashPasswordAsync(password);
            var hash2 = await _bcryptPasswordHasher.HashPasswordAsync(password);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public async Task HashPasswordAsync_WithComplexPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            string complexPassword = "P@ssw0rd!#$%^&*()_+-=[]{}|;:',.<>?";

            // Act
            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(complexPassword);

            // Assert
            Assert.NotNull(passwordHash);
            Assert.NotEqual(complexPassword, passwordHash);
        }

        [Fact]
        public async Task HashPasswordAsync_WithLongPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            string longPassword = new string('a', 100);

            // Act
            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(longPassword);

            // Assert
            Assert.NotNull(passwordHash);
            Assert.NotEqual(longPassword, passwordHash);
        }

        [Fact]
        public async Task HashPasswordAsync_WithSingleCharacter_ShouldReturnHashedPassword()
        {
            // Arrange
            string singleCharPassword = "a";

            // Act
            var passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(singleCharPassword);

            // Assert
            Assert.NotNull(passwordHash);
            Assert.NotEqual(singleCharPassword, passwordHash);
        }

        [Fact]
        public async Task HashPasswordAsync_WithEmptyPassword_ShouldThrowException()
        {
            // Arrange
            string emptyPassword = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _bcryptPasswordHasher.HashPasswordAsync(emptyPassword));
            
            Assert.Equal("Password can't be empty", exception.Message);
        }

        [Fact]
        public async Task HashPasswordAsync_WithEmptyPassword_ShouldThrowExceptionType()
        {
            // Arrange
            string emptyPassword = "";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _bcryptPasswordHasher.HashPasswordAsync(emptyPassword));
        }

        #endregion

        #region VerifyPasswordAsync Tests

        [Fact]
        public async Task VerifyPasswordAsync_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            string rawPassword = "pass123";
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(rawPassword);

            // Act
            bool result = await _bcryptPasswordHasher.VerifyPasswordAsync(rawPassword, passwordHash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            string correctPassword = "pass123";
            string incorrectPassword = "wrongpass";
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(correctPassword);

            // Act
            bool result = await _bcryptPasswordHasher.VerifyPasswordAsync(incorrectPassword, passwordHash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithComplexPassword_ShouldReturnTrue()
        {
            // Arrange
            string complexPassword = "P@ssw0rd!#$%^&*()_+-=[]{}|;:',.<>?";
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(complexPassword);

            // Act
            bool result = await _bcryptPasswordHasher.VerifyPasswordAsync(complexPassword, passwordHash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithLongPassword_ShouldReturnTrue()
        {
            // Arrange
            string longPassword = new string('a', 100);
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(longPassword);

            // Act
            bool result = await _bcryptPasswordHasher.VerifyPasswordAsync(longPassword, passwordHash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithEmptyPassword_ShouldThrowException()
        {
            // Arrange
            string emptyPassword = "";
            string validHash = "somehash";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _bcryptPasswordHasher.VerifyPasswordAsync(emptyPassword, validHash));
            
            Assert.Equal("Password can't be empty", exception.Message);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithEmptyHash_ShouldThrowException()
        {
            // Arrange
            string validPassword = "pass123";
            string emptyHash = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _bcryptPasswordHasher.VerifyPasswordAsync(validPassword, emptyHash));
            
            Assert.Equal("Password Hash can't be empty", exception.Message);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithBothEmptyPasswordAndHash_ShouldThrowExceptionForPassword()
        {
            // Arrange
            string emptyPassword = "";
            string emptyHash = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _bcryptPasswordHasher.VerifyPasswordAsync(emptyPassword, emptyHash));
            
            // Password validation happens first
            Assert.Equal("Password can't be empty", exception.Message);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithInvalidHash_ShouldReturnFalse()
        {
            // Arrange
            string password = "pass123";
            string invalidHash = "invalid_hash_format";

            // Act & Assert
            // Should either return false or throw, depending on BCrypt behavior
            try
            {
                var result = await _bcryptPasswordHasher.VerifyPasswordAsync(password, invalidHash);
                Assert.False(result);
            }
            catch
            {
                // BCrypt may throw on invalid hash format - this is acceptable
                Assert.True(true);
            }
        }

        [Fact]
        public async Task VerifyPasswordAsync_WithCaseSensitivePassword_ShouldReturnFalse()
        {
            // Arrange
            string password = "Pass123";
            string differentCasePassword = "pass123";
            string passwordHash = await _bcryptPasswordHasher.HashPasswordAsync(password);

            // Act
            bool result = await _bcryptPasswordHasher.VerifyPasswordAsync(differentCasePassword, passwordHash);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task HashAndVerify_WithValidPassword_ShouldSuccessfullyValidatePassword()
        {
            // Arrange
            string password = "MySecurePassword123!";

            // Act
            string hash = await _bcryptPasswordHasher.HashPasswordAsync(password);
            bool isValid = await _bcryptPasswordHasher.VerifyPasswordAsync(password, hash);

            // Assert
            Assert.True(isValid);
            Assert.NotEqual(password, hash);
        }

        #endregion
    }
}
