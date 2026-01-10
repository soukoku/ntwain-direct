using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Represents a TWAIN Direct API error.
/// </summary>
public record ApiError
{
    /// <summary>
    /// Error code.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    /// Severity of the error.
    /// </summary>
    [JsonPropertyName("severity")]
    public Severity Severity { get; init; }

    /// <summary>
    /// Additional details about the error.
    /// </summary>
    [JsonPropertyName("details")]
    public string? Details { get; init; }
}

/// <summary>
/// HTTP status severity levels.
/// </summary>
public enum Severity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error
}
