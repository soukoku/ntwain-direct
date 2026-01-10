using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Base record for all TWAIN Direct command responses.
/// </summary>
public record CommandResponse
{
    /// <summary>
    /// The kind of response.
    /// </summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>
    /// Unique identifier matching the request's commandId.
    /// </summary>
    [JsonPropertyName("commandId")]
    public string? CommandId { get; init; }

    /// <summary>
    /// Method that was called.
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>
    /// Results of the command.
    /// </summary>
    [JsonPropertyName("results")]
    public CommandResults? Results { get; init; }
}

/// <summary>
/// Command results container.
/// </summary>
public record CommandResults
{
    /// <summary>
    /// Whether the command succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Session information (for session-related commands).
    /// </summary>
    [JsonPropertyName("session")]
    public SessionInfo? Session { get; init; }

    /// <summary>
    /// Events that have occurred (for waitForEvents).
    /// </summary>
    [JsonPropertyName("events")]
    public SessionEvent[]? Events { get; init; }

    /// <summary>
    /// Metadata for an image block.
    /// </summary>
    [JsonPropertyName("metadata")]
    public ImageBlockMetadata? Metadata { get; init; }
}

/// <summary>
/// TWAIN Direct status information.
/// </summary>
public record TwainDirectStatus
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Detected status of the operation.
    /// </summary>
    [JsonPropertyName("detected")]
    public string? Detected { get; init; }

    /// <summary>
    /// JSON key for the status.
    /// </summary>
    [JsonPropertyName("jsonKey")]
    public string? JsonKey { get; init; }
}
