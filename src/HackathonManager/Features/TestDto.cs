using FastEndpoints;
using FastIDs.TypeId;
using NodaTime;

namespace HackathonManager.Features;

public sealed record TestDto(TypeId Id, string Name, [property: ToHeader("ETag")] string Version, Instant Queried);
