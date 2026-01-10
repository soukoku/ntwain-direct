using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Privet info response (for local discovery).
/// </summary>
public record PrivetInfo
{
    /// <summary>
    /// Version of the privet protocol.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// Display name of the device.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Description of the device.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// URL for more information.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// Type of device.
    /// </summary>
    [JsonPropertyName("type")]
    public string[]? Type { get; init; }

    /// <summary>
    /// Unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Device state.
    /// </summary>
    [JsonPropertyName("device_state")]
    public string? DeviceState { get; init; }

    /// <summary>
    /// Connection state.
    /// </summary>
    [JsonPropertyName("connection_state")]
    public string? ConnectionState { get; init; }

    /// <summary>
    /// Manufacturer.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    /// <summary>
    /// Model name.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>
    /// Serial number.
    /// </summary>
    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; init; }

    /// <summary>
    /// Firmware version.
    /// </summary>
    [JsonPropertyName("firmware")]
    public string? Firmware { get; init; }

    /// <summary>
    /// Uptime in seconds.
    /// </summary>
    [JsonPropertyName("uptime")]
    public int? Uptime { get; init; }

    /// <summary>
    /// Setup URL.
    /// </summary>
    [JsonPropertyName("setup_url")]
    public string? SetupUrl { get; init; }

    /// <summary>
    /// Support URL.
    /// </summary>
    [JsonPropertyName("support_url")]
    public string? SupportUrl { get; init; }

    /// <summary>
    /// Update URL.
    /// </summary>
    [JsonPropertyName("update_url")]
    public string? UpdateUrl { get; init; }

    /// <summary>
    /// X-Privet-Token for authentication.
    /// </summary>
    [JsonPropertyName("x-privet-token")]
    public string? XPrivetToken { get; init; }

    /// <summary>
    /// Supported APIs.
    /// </summary>
    [JsonPropertyName("api")]
    public string[]? Api { get; init; }

    /// <summary>
    /// Semantic state of the device.
    /// </summary>
    [JsonPropertyName("semantic_state")]
    public string? SemanticState { get; init; }
}
