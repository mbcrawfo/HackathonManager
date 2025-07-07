using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Services;

public sealed class PasswordService
{
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(ILogger<PasswordService> logger)
    {
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        var stopwatch = Stopwatch.StartNew();
        var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 13);
        _logger.LogTrace("Password hashing completed in {ElapsedMilliseconds} ms", stopwatch.Elapsed.TotalMilliseconds);

        return hash;
    }

    public bool VerifyPassword(string providedPassword, string hashedPassword)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
        finally
        {
            _logger.LogTrace("Password verified in {ElapsedMilliseconds} ms", stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
