using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Information about a scanner device.
/// </summary>
public record ScannerInfo
{
    /// <summary>
    /// Unique identifier for the scanner.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the scanner.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Description of the scanner.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Manufacturer of the scanner.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    /// <summary>
    /// Model of the scanner.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>
    /// Serial number of the scanner.
    /// </summary>
    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; init; }

    /// <summary>
    /// Firmware version of the scanner.
    /// </summary>
    [JsonPropertyName("firmware")]
    public string? Firmware { get; init; }

    /// <summary>
    /// Type of connection (e.g., "usb", "network").
    /// </summary>
    [JsonPropertyName("connectionType")]
    public string? ConnectionType { get; init; }

    /// <summary>
    /// Version of TWAIN Direct protocol supported.
    /// </summary>
    [JsonPropertyName("twainDirectVersion")]
    public string? TwainDirectVersion { get; init; }

    /// <summary>
    /// TWAIN protocol version.
    /// </summary>
    [JsonPropertyName("twainVersion")]
    public string? TwainVersion { get; init; }

    /// <summary>
    /// Driver version.
    /// </summary>
    [JsonPropertyName("driverVersion")]
    public string? DriverVersion { get; init; }

    /// <summary>
    /// Hostname for network scanners.
    /// </summary>
    [JsonPropertyName("hostName")]
    public string? HostName { get; init; }

    /// <summary>
    /// IP address for network scanners.
    /// </summary>
    [JsonPropertyName("ipv4")]
    public string? Ipv4 { get; init; }

    /// <summary>
    /// IPv6 address for network scanners.
    /// </summary>
    [JsonPropertyName("ipv6")]
    public string? Ipv6 { get; init; }

    /// <summary>
    /// Whether the scanner is online.
    /// </summary>
    [JsonPropertyName("online")]
    public bool Online { get; init; }

    /// <summary>
    /// Cloud identifier if registered.
    /// </summary>
    [JsonPropertyName("cloudId")]
    public string? CloudId { get; init; }

    /// <summary>
    /// Type of scanner hardware.
    /// </summary>
    [JsonPropertyName("ty")]
    public string? Ty { get; init; }

    /// <summary>
    /// HTTPS port for secure connections.
    /// </summary>
    [JsonPropertyName("httpsPort")]
    public int? HttpsPort { get; init; }

    /// <summary>
    /// HTTP port.
    /// </summary>
    [JsonPropertyName("httpPort")]
    public int? HttpPort { get; init; }

    /// <summary>
    /// Note field for additional info.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; init; }
}

/// <summary>
/// Scanner status information.
/// </summary>
public record ScannerStatus
{
    /// <summary>
    /// Whether the scanner is ready.
    /// </summary>
    [JsonPropertyName("ready")]
    public bool Ready { get; init; }

    /// <summary>
    /// Current scanner state.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>
    /// Error code if any.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Error message if any.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Paper status information.
    /// </summary>
    [JsonPropertyName("paper")]
    public PaperStatus? Paper { get; init; }

    /// <summary>
    /// Cover status information.
    /// </summary>
    [JsonPropertyName("cover")]
    public CoverStatus? Cover { get; init; }
}

/// <summary>
/// Paper status information.
/// </summary>
public record PaperStatus
{
    /// <summary>
    /// Whether paper is present in the feeder.
    /// </summary>
    [JsonPropertyName("present")]
    public bool Present { get; init; }

    /// <summary>
    /// Whether a paper jam is detected.
    /// </summary>
    [JsonPropertyName("jam")]
    public bool Jam { get; init; }

    /// <summary>
    /// Whether the feeder is empty.
    /// </summary>
    [JsonPropertyName("empty")]
    public bool Empty { get; init; }
}

/// <summary>
/// Cover status information.
/// </summary>
public record CoverStatus
{
    /// <summary>
    /// Whether the cover is open.
    /// </summary>
    [JsonPropertyName("open")]
    public bool Open { get; init; }
}
