using System.Collections.Frozen;

namespace HackathonManager.Interfaces;

public interface IUniqueConstraintMapping
{
    /// <summary>
    ///     Maps the names of database unique constraints to the name that should be used in an error message
    ///     describing it.
    /// </summary>
    static abstract FrozenDictionary<string, string> UniqueConstraintMappings { get; }
}
