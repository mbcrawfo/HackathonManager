using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace HackathonManager.Utilities;

public static class PaginationCursor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    ///     Encodes a cursor object by serializing to json and converting to base64url.
    /// </summary>
    /// <param name="cursor"></param>
    /// <returns></returns>
    public static string Encode(object cursor)
    {
        using var stream = new MemoryStream(256);
        using var writer = new Utf8JsonWriter(stream);
        JsonSerializer.Serialize(writer, cursor, cursor.GetType(), SerializerOptions);

        writer.Flush();
        var bytesWritten = (int)writer.BytesCommitted;

        return Base64UrlEncoder.Encode(stream.GetBuffer(), offset: 0, bytesWritten);
    }

    /// <summary>
    ///     Decodes a string containing a cursor object in base64url json.
    /// </summary>
    /// <param name="base64Url"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="JsonException">
    ///     The cursor could not be decoded.
    /// </exception>
    public static T Decode<T>(string base64Url)
    {
        var bytes = Base64UrlEncoder.DecodeBytes(base64Url);
        var reader = new Utf8JsonReader(bytes);
        return JsonSerializer.Deserialize<T>(ref reader, SerializerOptions)
            ?? throw new JsonException("Failed to deserialize base64url encoded cursor.");
    }
}
