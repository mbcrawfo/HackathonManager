using System.Text.Json;
using System.Text.Json.Serialization;
using FastIDs.TypeId.Serialization.SystemTextJson;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace HackathonManager.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureSerializerOptions(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.ConfigureForTypeId();

        return options;
    }
}
