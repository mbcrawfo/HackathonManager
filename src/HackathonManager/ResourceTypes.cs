using HackathonManager.Utilities;

namespace HackathonManager;

public static class ResourceTypes
{
    public const string User = "user";

    public static readonly TypeIdDecodedValueConverter UserIdValueConverter = new(User);
}
