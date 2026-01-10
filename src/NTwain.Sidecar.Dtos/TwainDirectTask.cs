using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// TWAIN Direct task definition.
/// </summary>
public record TwainDirectTask
{
    /// <summary>
    /// Actions to perform for this task.
    /// </summary>
    [JsonPropertyName("actions")]
    public TaskAction[]? Actions { get; init; }
}

/// <summary>
/// A task action defining scanner behavior.
/// </summary>
public record TaskAction
{
    /// <summary>
    /// Name of the action.
    /// </summary>
    [JsonPropertyName("action")]
    public required string Action { get; init; }

    /// <summary>
    /// Streams to configure for this action.
    /// </summary>
    [JsonPropertyName("streams")]
    public TaskStream[]? Streams { get; init; }

    /// <summary>
    /// Exception to throw if action fails.
    /// </summary>
    [JsonPropertyName("exception")]
    public string? Exception { get; init; }

    /// <summary>
    /// Vendor-specific data.
    /// </summary>
    [JsonPropertyName("vendor")]
    public string? Vendor { get; init; }
}

/// <summary>
/// A stream definition within a task.
/// </summary>
public record TaskStream
{
    /// <summary>
    /// Name of the stream.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Sources for this stream.
    /// </summary>
    [JsonPropertyName("sources")]
    public TaskSource[]? Sources { get; init; }
}

/// <summary>
/// A source definition within a stream.
/// </summary>
public record TaskSource
{
    /// <summary>
    /// Name of the source.
    /// </summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>
    /// Pixel formats for this source.
    /// </summary>
    [JsonPropertyName("pixelFormats")]
    public TaskPixelFormat[]? PixelFormats { get; init; }

    /// <summary>
    /// Exception to throw if source fails.
    /// </summary>
    [JsonPropertyName("exception")]
    public string? Exception { get; init; }
}

/// <summary>
/// Pixel format configuration within a source.
/// </summary>
public record TaskPixelFormat
{
    /// <summary>
    /// Pixel format type.
    /// </summary>
    [JsonPropertyName("pixelFormat")]
    public required string PixelFormat { get; init; }

    /// <summary>
    /// Attributes for this pixel format.
    /// </summary>
    [JsonPropertyName("attributes")]
    public TaskAttributes[]? Attributes { get; init; }

    /// <summary>
    /// Exception to throw if pixel format fails.
    /// </summary>
    [JsonPropertyName("exception")]
    public string? Exception { get; init; }
}

/// <summary>
/// Attributes for pixel format configuration.
/// </summary>
public record TaskAttributes
{
    /// <summary>
    /// Attribute name/type.
    /// </summary>
    [JsonPropertyName("attribute")]
    public required string Attribute { get; init; }

    /// <summary>
    /// Values for this attribute.
    /// </summary>
    [JsonPropertyName("values")]
    public TaskAttributeValue[]? Values { get; init; }

    /// <summary>
    /// Exception to throw if attribute fails.
    /// </summary>
    [JsonPropertyName("exception")]
    public string? Exception { get; init; }
}

/// <summary>
/// A value for a task attribute.
/// </summary>
public record TaskAttributeValue
{
    /// <summary>
    /// The value.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; init; }

    /// <summary>
    /// Exception to throw if value fails.
    /// </summary>
    [JsonPropertyName("exception")]
    public string? Exception { get; init; }
}

/// <summary>
/// Task reply returned after sending a task.
/// </summary>
public record TwainDirectTaskReply
{
    /// <summary>
    /// Actions with their results.
    /// </summary>
    [JsonPropertyName("actions")]
    public TaskActionReply[]? Actions { get; init; }
}

/// <summary>
/// Reply for a single task action.
/// </summary>
public record TaskActionReply
{
    /// <summary>
    /// Name of the action.
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; init; }

    /// <summary>
    /// Results of the action.
    /// </summary>
    [JsonPropertyName("results")]
    public TaskActionResult? Results { get; init; }

    /// <summary>
    /// Streams configured by this action.
    /// </summary>
    [JsonPropertyName("streams")]
    public TaskStreamReply[]? Streams { get; init; }
}

/// <summary>
/// Result of a task action.
/// </summary>
public record TaskActionResult
{
    /// <summary>
    /// Whether the action succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Detected status.
    /// </summary>
    [JsonPropertyName("detected")]
    public string? Detected { get; init; }

    /// <summary>
    /// JSON key for the result.
    /// </summary>
    [JsonPropertyName("jsonKey")]
    public string? JsonKey { get; init; }
}

/// <summary>
/// Reply for a stream in a task action.
/// </summary>
public record TaskStreamReply
{
    /// <summary>
    /// Name of the stream.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Sources in this stream.
    /// </summary>
    [JsonPropertyName("sources")]
    public TaskSourceReply[]? Sources { get; init; }
}

/// <summary>
/// Reply for a source in a stream.
/// </summary>
public record TaskSourceReply
{
    /// <summary>
    /// Source name.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    /// <summary>
    /// Pixel formats for this source.
    /// </summary>
    [JsonPropertyName("pixelFormats")]
    public TaskPixelFormatReply[]? PixelFormats { get; init; }
}

/// <summary>
/// Reply for a pixel format in a source.
/// </summary>
public record TaskPixelFormatReply
{
    /// <summary>
    /// Pixel format type.
    /// </summary>
    [JsonPropertyName("pixelFormat")]
    public string? PixelFormat { get; init; }

    /// <summary>
    /// Attributes for this pixel format.
    /// </summary>
    [JsonPropertyName("attributes")]
    public TaskAttributesReply[]? Attributes { get; init; }
}

/// <summary>
/// Reply for attributes in a pixel format.
/// </summary>
public record TaskAttributesReply
{
    /// <summary>
    /// Attribute name.
    /// </summary>
    [JsonPropertyName("attribute")]
    public string? Attribute { get; init; }

    /// <summary>
    /// Values for this attribute.
    /// </summary>
    [JsonPropertyName("values")]
    public TaskAttributeValueReply[]? Values { get; init; }
}

/// <summary>
/// Reply for an attribute value.
/// </summary>
public record TaskAttributeValueReply
{
    /// <summary>
    /// The value.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; init; }

    /// <summary>
    /// Status of applying this value.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
