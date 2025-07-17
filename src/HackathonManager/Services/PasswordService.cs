using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Services;

public sealed class PasswordService
{
    private readonly ILogger<PasswordService> _logger;
    private readonly int _workFactor = 13;

    public PasswordService(ILogger<PasswordService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var workFactorOverride = configuration.GetValue("BCryptWorkFactor_DoNotSet_ForTestingOnly", defaultValue: 0);
        if (workFactorOverride is not 0)
        {
            _logger.LogWarning("BCrypt work factor set to {WorkFactor} via configuration", workFactorOverride);
            _workFactor = workFactorOverride;
        }
    }

    public string HashPassword(string password)
    {
        var stopwatch = Stopwatch.StartNew();
        var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);
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
