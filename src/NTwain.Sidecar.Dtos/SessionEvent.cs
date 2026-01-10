using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// A session event notification.
/// </summary>
public record SessionEvent
{
    /// <summary>
    /// Type of event.
    /// </summary>
    [JsonPropertyName("event")]
    public required string Event { get; init; }

    /// <summary>
    /// Session information at the time of the event.
    /// </summary>
    [JsonPropertyName("session")]
    public SessionInfo? Session { get; init; }
}
