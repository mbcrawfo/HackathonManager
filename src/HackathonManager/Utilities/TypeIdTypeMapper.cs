using System;
using FastIDs.TypeId;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace HackathonManager.Utilities;

public sealed class TypeIdTypeMapper : ITypeMapper
{
    /// <inheritdoc />
    public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
    {
        schema.Title = "TypeId";
        schema.Type = JsonObjectType.String;
        schema.Example = "prefix_01h93ech7jf5ktdwg6ye383x34";
    }

    /// <inheritdoc />
    public Type MappedType => typeof(TypeId);

    /// <inheritdoc />
    public bool UseReference => false;
}
