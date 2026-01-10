using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Session information returned by session commands.
/// </summary>
public record SessionInfo
{
    /// <summary>
    /// Unique identifier for the session.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    /// <summary>
    /// Current revision number of the session state.
    /// </summary>
    [JsonPropertyName("revision")]
    public int Revision { get; init; }

    /// <summary>
    /// Current state of the session.
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; init; }

    /// <summary>
    /// Status of the session.
    /// </summary>
    [JsonPropertyName("status")]
    public SessionStatus? Status { get; init; }

    /// <summary>
    /// Whether the done capturing flag is set.
    /// </summary>
    [JsonPropertyName("doneCapturing")]
    public bool DoneCapturing { get; init; }

    /// <summary>
    /// Whether the end of job flag is set.
    /// </summary>
    [JsonPropertyName("endOfJob")]
    public bool EndOfJob { get; init; }

    /// <summary>
    /// List of available image blocks.
    /// </summary>
    [JsonPropertyName("imageBlocks")]
    public int[]? ImageBlocks { get; init; }

    /// <summary>
    /// The number of the image block being released.
    /// </summary>
    [JsonPropertyName("imageBlocksDrained")]
    public int? ImageBlocksDrained { get; init; }

    /// <summary>
    /// Task reply information.
    /// </summary>
    [JsonPropertyName("task")]
    public TwainDirectTaskReply? Task { get; init; }
}

/// <summary>
/// Session status information.
/// </summary>
public record SessionStatus
{
    /// <summary>
    /// Whether the session is in a successful state.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Detected status.
    /// </summary>
    [JsonPropertyName("detected")]
    public string? Detected { get; init; }
}
