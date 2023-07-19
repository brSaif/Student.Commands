using System.Text.Json.Serialization;

namespace gRPCOnHttp3.Domain.Common;

/// <summary>
/// Marker interface for event Data.
/// </summary>
public interface IEventData
{
    /// <summary>
    /// Gets the type of an <see cref="Event"/>.
    /// </summary>
    [JsonIgnore]
    EventType Type { get; }
}