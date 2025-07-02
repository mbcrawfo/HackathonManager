using System;
using FastIDs.TypeId;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HackathonManager.Utilities;

public sealed class TypeIdDecodedValueConverter : ValueConverter<TypeIdDecoded, Guid>
{
    public TypeIdDecodedValueConverter(string typePrefix)
        : base(id => id.Id, id => TypeIdDecoded.FromUuidV7(typePrefix, id)) { }
}
