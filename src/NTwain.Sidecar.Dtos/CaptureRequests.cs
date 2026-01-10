using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Request to start capturing images.
/// </summary>
public record StartCapturingRequest : CommandRequest
{
    /// <summary>
    /// Creates a new StartCapturingRequest.
    /// </summary>
    public StartCapturingRequest()
    {
        Kind = "twainlocalscanner";
        Method = "startCapturing";
    }
}

/// <summary>
/// Response from starting capture.
/// </summary>
public record StartCapturingResponse : CommandResponse;

/// <summary>
/// Request to stop capturing images.
/// </summary>
public record StopCapturingRequest : CommandRequest
{
    /// <summary>
    /// Creates a new StopCapturingRequest.
    /// </summary>
    public StopCapturingRequest()
    {
        Kind = "twainlocalscanner";
        Method = "stopCapturing";
    }
}

/// <summary>
/// Response from stopping capture.
/// </summary>
public record StopCapturingResponse : CommandResponse;

/// <summary>
/// Request to read an image block.
/// </summary>
public record ReadImageBlockRequest : CommandRequest
{
    /// <summary>
    /// Creates a new ReadImageBlockRequest.
    /// </summary>
    public ReadImageBlockRequest()
    {
        Kind = "twainlocalscanner";
        Method = "readImageBlock";
    }
}

/// <summary>
/// Parameters for reading an image block.
/// </summary>
public record ReadImageBlockParams : CommandParams
{
    /// <summary>
    /// The image block number to read.
    /// </summary>
    [JsonPropertyName("imageBlockNum")]
    public new required int ImageBlockNum { get; init; }

    /// <summary>
    /// Whether to include metadata with the image block.
    /// </summary>
    [JsonPropertyName("withMetadata")]
    public new bool? WithMetadata { get; init; }
}

/// <summary>
/// Response from reading an image block.
/// </summary>
public record ReadImageBlockResponse : CommandResponse
{
    /// <summary>
    /// Results containing image block data.
    /// </summary>
    [JsonPropertyName("results")]
    public new ReadImageBlockResults? Results { get; init; }
}

/// <summary>
/// Results from reading an image block.
/// </summary>
public record ReadImageBlockResults
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Session information.
    /// </summary>
    [JsonPropertyName("session")]
    public SessionInfo? Session { get; init; }

    /// <summary>
    /// Metadata for the image block (if requested).
    /// </summary>
    [JsonPropertyName("metadata")]
    public ImageBlockMetadata? Metadata { get; init; }

    /// <summary>
    /// Detected status.
    /// </summary>
    [JsonPropertyName("detected")]
    public string? Detected { get; init; }
}

/// <summary>
/// Request to read image block metadata only.
/// </summary>
public record ReadImageBlockMetadataRequest : CommandRequest
{
    /// <summary>
    /// Creates a new ReadImageBlockMetadataRequest.
    /// </summary>
    public ReadImageBlockMetadataRequest()
    {
        Kind = "twainlocalscanner";
        Method = "readImageBlockMetadata";
    }
}

/// <summary>
/// Parameters for reading image block metadata.
/// </summary>
public record ReadImageBlockMetadataParams : CommandParams
{
    /// <summary>
    /// The image block number to get metadata for.
    /// </summary>
    [JsonPropertyName("imageBlockNum")]
    public new required int ImageBlockNum { get; init; }
}

/// <summary>
/// Response from reading image block metadata.
/// </summary>
public record ReadImageBlockMetadataResponse : CommandResponse
{
    /// <summary>
    /// Results containing metadata.
    /// </summary>
    [JsonPropertyName("results")]
    public new ReadImageBlockMetadataResults? Results { get; init; }
}

/// <summary>
/// Results from reading image block metadata.
/// </summary>
public record ReadImageBlockMetadataResults
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Session information.
    /// </summary>
    [JsonPropertyName("session")]
    public SessionInfo? Session { get; init; }

    /// <summary>
    /// Metadata for the image block.
    /// </summary>
    [JsonPropertyName("metadata")]
    public ImageBlockMetadata? Metadata { get; init; }
}
