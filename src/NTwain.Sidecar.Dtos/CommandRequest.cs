using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Base record for all TWAIN Direct command requests.
/// </summary>
public record CommandRequest
{
    /// <summary>
    /// The kind of command being sent.
    /// </summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>
    /// Unique identifier for the command request.
    /// </summary>
    [JsonPropertyName("commandId")]
    public string? CommandId { get; init; }

    /// <summary>
    /// Method being called (e.g., "createSession", "sendTask").
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>
    /// Parameters for the command.
    /// </summary>
    [JsonPropertyName("params")]
    public CommandParams? Params { get; init; }
}

/// <summary>
/// Command parameters container.
/// </summary>
public record CommandParams
{
    /// <summary>
    /// Session ID for session-specific commands.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; init; }

    /// <summary>
    /// The task to send (for sendTask command).
    /// </summary>
    [JsonPropertyName("task")]
    public TwainDirectTask? Task { get; init; }

    /// <summary>
    /// Image block number (for readImageBlock/readImageBlockMetadata).
    /// </summary>
    [JsonPropertyName("imageBlockNum")]
    public int? ImageBlockNum { get; init; }

    /// <summary>
    /// Whether to include metadata with image block.
    /// </summary>
    [JsonPropertyName("withMetadata")]
    public bool? WithMetadata { get; init; }

    /// <summary>
    /// Session revision for waitForEvents.
    /// </summary>
    [JsonPropertyName("sessionRevision")]
    public int? SessionRevision { get; init; }
}
