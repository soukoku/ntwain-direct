using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Request to create a new TWAIN Direct session.
/// </summary>
public record CreateSessionRequest : CommandRequest
{
    /// <summary>
    /// Creates a new CreateSessionRequest.
    /// </summary>
    public CreateSessionRequest()
    {
        Kind = "twainlocalscanner";
        Method = "createSession";
    }
}

/// <summary>
/// Response from creating a session.
/// </summary>
public record CreateSessionResponse : CommandResponse;

/// <summary>
/// Request to close an existing TWAIN Direct session.
/// </summary>
public record CloseSessionRequest : CommandRequest
{
    /// <summary>
    /// Creates a new CloseSessionRequest.
    /// </summary>
    public CloseSessionRequest()
    {
        Kind = "twainlocalscanner";
        Method = "closeSession";
    }
}

/// <summary>
/// Response from closing a session.
/// </summary>
public record CloseSessionResponse : CommandResponse;

/// <summary>
/// Request to get the current session state.
/// </summary>
public record GetSessionRequest : CommandRequest
{
    /// <summary>
    /// Creates a new GetSessionRequest.
    /// </summary>
    public GetSessionRequest()
    {
        Kind = "twainlocalscanner";
        Method = "getSession";
    }
}

/// <summary>
/// Response from getting session state.
/// </summary>
public record GetSessionResponse : CommandResponse;

/// <summary>
/// Request to release image blocks after they have been processed.
/// </summary>
public record ReleaseImageBlocksRequest : CommandRequest
{
    /// <summary>
    /// Creates a new ReleaseImageBlocksRequest.
    /// </summary>
    public ReleaseImageBlocksRequest()
    {
        Kind = "twainlocalscanner";
        Method = "releaseImageBlocks";
    }
}

/// <summary>
/// Parameters for releasing image blocks.
/// </summary>
public record ReleaseImageBlocksParams : CommandParams
{
    /// <summary>
    /// The first image block number to release.
    /// </summary>
    [JsonPropertyName("imageBlockNum")]
    public new required int ImageBlockNum { get; init; }

    /// <summary>
    /// The last image block number to release.
    /// </summary>
    [JsonPropertyName("lastImageBlockNum")]
    public required int LastImageBlockNum { get; init; }
}

/// <summary>
/// Response from releasing image blocks.
/// </summary>
public record ReleaseImageBlocksResponse : CommandResponse;

/// <summary>
/// Request to send a task to the scanner.
/// </summary>
public record SendTaskRequest : CommandRequest
{
    /// <summary>
    /// Creates a new SendTaskRequest.
    /// </summary>
    public SendTaskRequest()
    {
        Kind = "twainlocalscanner";
        Method = "sendTask";
    }
}

/// <summary>
/// Response from sending a task.
/// </summary>
public record SendTaskResponse : CommandResponse;

/// <summary>
/// Request for scanner information (infoex command).
/// </summary>
public record InfoExRequest : CommandRequest
{
    /// <summary>
    /// Creates a new InfoExRequest.
    /// </summary>
    public InfoExRequest()
    {
        Kind = "twainlocalscanner";
        Method = "infoex";
    }
}

/// <summary>
/// Response containing scanner information.
/// </summary>
public record InfoExResponse : CommandResponse
{
    /// <summary>
    /// Scanner information results.
    /// </summary>
    [JsonPropertyName("results")]
    public new InfoExResults? Results { get; init; }
}

/// <summary>
/// Results from infoex command.
/// </summary>
public record InfoExResults
{
    /// <summary>
    /// Whether the command succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Array of scanner information.
    /// </summary>
    [JsonPropertyName("scanners")]
    public ScannerInfo[]? Scanners { get; init; }
}

/// <summary>
/// Request to wait for session events.
/// </summary>
public record WaitForEventsRequest : CommandRequest
{
    /// <summary>
    /// Creates a new WaitForEventsRequest.
    /// </summary>
    public WaitForEventsRequest()
    {
        Kind = "twainlocalscanner";
        Method = "waitForEvents";
    }
}

/// <summary>
/// Parameters for waiting for events.
/// </summary>
public record WaitForEventsParams : CommandParams
{
    /// <summary>
    /// The session revision to start waiting from.
    /// Events with revision greater than this will be returned.
    /// </summary>
    [JsonPropertyName("sessionRevision")]
    public new required int SessionRevision { get; init; }
}

/// <summary>
/// Response from waiting for events.
/// </summary>
public record WaitForEventsResponse : CommandResponse
{
    /// <summary>
    /// Results containing events.
    /// </summary>
    [JsonPropertyName("results")]
    public new WaitForEventsResults? Results { get; init; }
}

/// <summary>
/// Results from waiting for events.
/// </summary>
public record WaitForEventsResults
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Array of events that occurred.
    /// </summary>
    [JsonPropertyName("events")]
    public SessionEvent[]? Events { get; init; }
}

/// <summary>
/// Long polling configuration for waitForEvents.
/// </summary>
public record LongPollConfig
{
    /// <summary>
    /// Timeout in milliseconds for the long poll request.
    /// </summary>
    [JsonPropertyName("timeoutMs")]
    public int TimeoutMs { get; init; } = 30000;

    /// <summary>
    /// Maximum number of events to return in a single response.
    /// </summary>
    [JsonPropertyName("maxEvents")]
    public int MaxEvents { get; init; } = 100;
}
