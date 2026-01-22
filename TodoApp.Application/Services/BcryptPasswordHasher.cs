using System;
using TodoApp.Application.Services.Interfaces;

namespace TodoApp.Application.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public Task<string> HashPasswordAsync(string password)
    {
        if (password == "")
        {
            Exception exception = new("Password can't be empty");
            throw exception;
        }
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        if (password == "")
        {
            Exception exception = new("Password can't be empty");
            throw exception;
        }

        if (passwordHash == "")
        {
            Exception exception = new("Password Hash can't be empty");
            throw exception;
        }
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
    }
}