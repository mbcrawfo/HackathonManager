using System;
using FastIDs.TypeId;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace HackathonManager.Utilities;

/// <summary>
///     Describes the <see cref="TypeId" /> type in Swagger docs.
/// </summary>
public sealed class SwaggerTypeIdTypeMapper : ITypeMapper
{
    /// <inheritdoc />
    public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
    {
        schema.Type = JsonObjectType.String;
        schema.Format = "typeid";
        schema.Description = "A resource identifier with a resource type tag and a base32 formatted UUID.";
        schema.Example = "tag_01h93ech7jf5ktdwg6ye383x34";
    }

    /// <inheritdoc />
    public Type MappedType => typeof(TypeId);

    /// <inheritdoc />
    public bool UseReference => false;
}
