using Asp.Versioning;
using FastEndpoints.AspVersioning;

namespace HackathonManager;

public static class ApiTags
{
    public const string Users = "Users";

    public static void Create()
    {
        VersionSets.CreateApi("Test", v => v.HasApiVersion(new ApiVersion(1.0)));
        VersionSets.CreateApi(Users, b => b.HasApiVersion(new ApiVersion(1)));
    }
}
