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
    }

    /// <inheritdoc />
    public Type MappedType => typeof(TypeId);

    /// <inheritdoc />
    public bool UseReference => false;
}
