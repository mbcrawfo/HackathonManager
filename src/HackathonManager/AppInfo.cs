using System;
using System.Reflection;

namespace HackathonManager;

public static class AppInfo
{
    public static readonly Assembly Assembly = typeof(Program).Assembly;

    public static readonly string Name =
        Assembly.GetName().Name ?? throw new InvalidOperationException("Assembly name is null");

    public static readonly string Version =
        Assembly.GetName().Version?.ToString() ?? throw new InvalidOperationException("Assembly version is null");
}
