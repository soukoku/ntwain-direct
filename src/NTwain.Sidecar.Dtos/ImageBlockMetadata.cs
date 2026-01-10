using System.Text.Json.Serialization;

namespace NTwain.Sidecar.Dtos;

/// <summary>
/// Metadata for an image block.
/// </summary>
public record ImageBlockMetadata
{
    /// <summary>
    /// Address information for the image.
    /// </summary>
    [JsonPropertyName("address")]
    public ImageAddress? Address { get; init; }

    /// <summary>
    /// Image description.
    /// </summary>
    [JsonPropertyName("image")]
    public ImageDescription? Image { get; init; }

    /// <summary>
    /// Status of the image capture.
    /// </summary>
    [JsonPropertyName("status")]
    public MetadataStatus? Status { get; init; }
}

/// <summary>
/// Address information for locating an image in a scanning session.
/// </summary>
public record ImageAddress
{
    /// <summary>
    /// Image number within the source.
    /// </summary>
    [JsonPropertyName("imageNumber")]
    public int ImageNumber { get; init; }

    /// <summary>
    /// Sheet number within the job.
    /// </summary>
    [JsonPropertyName("sheetNumber")]
    public int SheetNumber { get; init; }

    /// <summary>
    /// Source of the image (e.g., "feederFront", "feederRear", "flatbed").
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    /// <summary>
    /// Stream name if multiple streams are configured.
    /// </summary>
    [JsonPropertyName("streamName")]
    public string? StreamName { get; init; }

    /// <summary>
    /// Source name within the stream.
    /// </summary>
    [JsonPropertyName("sourceName")]
    public string? SourceName { get; init; }

    /// <summary>
    /// Pixel format name.
    /// </summary>
    [JsonPropertyName("pixelFormatName")]
    public string? PixelFormatName { get; init; }

    /// <summary>
    /// Whether this is more data for a partial image.
    /// </summary>
    [JsonPropertyName("moreParts")]
    public string? MoreParts { get; init; }
}

/// <summary>
/// Description of the captured image.
/// </summary>
public record ImageDescription
{
    /// <summary>
    /// Compression used for the image data.
    /// </summary>
    [JsonPropertyName("compression")]
    public string? Compression { get; init; }

    /// <summary>
    /// Pixel format of the image.
    /// </summary>
    [JsonPropertyName("pixelFormat")]
    public string? PixelFormat { get; init; }

    /// <summary>
    /// Width of the image in pixels.
    /// </summary>
    [JsonPropertyName("pixelWidth")]
    public int PixelWidth { get; init; }

    /// <summary>
    /// Height of the image in pixels.
    /// </summary>
    [JsonPropertyName("pixelHeight")]
    public int PixelHeight { get; init; }

    /// <summary>
    /// Offset of this block from the left edge in pixels.
    /// </summary>
    [JsonPropertyName("pixelOffsetX")]
    public int PixelOffsetX { get; init; }

    /// <summary>
    /// Offset of this block from the top edge in pixels.
    /// </summary>
    [JsonPropertyName("pixelOffsetY")]
    public int PixelOffsetY { get; init; }

    /// <summary>
    /// Horizontal resolution in DPI.
    /// </summary>
    [JsonPropertyName("resolution")]
    public double Resolution { get; init; }

    /// <summary>
    /// Size of the image data in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }
}

/// <summary>
/// Status information for captured metadata.
/// </summary>
public record MetadataStatus
{
    /// <summary>
    /// Whether the image capture was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Detected status of the scan.
    /// </summary>
    [JsonPropertyName("detected")]
    public string? Detected { get; init; }
}

/// <summary>
/// Extended image metadata with all possible properties.
/// </summary>
public record ExtendedImageMetadata : ImageBlockMetadata
{
    /// <summary>
    /// File format of the image.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// MIME type of the image.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }

    /// <summary>
    /// Whether this is the last part of a multi-part image.
    /// </summary>
    [JsonPropertyName("lastPart")]
    public bool LastPart { get; init; }

    /// <summary>
    /// Part number for multi-part images.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public int? PartNumber { get; init; }

    /// <summary>
    /// Total number of parts for multi-part images.
    /// </summary>
    [JsonPropertyName("totalParts")]
    public int? TotalParts { get; init; }

    /// <summary>
    /// Barcode data found in the image.
    /// </summary>
    [JsonPropertyName("barcodes")]
    public BarcodeData[]? Barcodes { get; init; }

    /// <summary>
    /// Patch code data found in the image.
    /// </summary>
    [JsonPropertyName("patchCodes")]
    public PatchCodeData[]? PatchCodes { get; init; }

    /// <summary>
    /// MICR data found in the image.
    /// </summary>
    [JsonPropertyName("micr")]
    public MicrData? Micr { get; init; }

    /// <summary>
    /// OCR data found in the image.
    /// </summary>
    [JsonPropertyName("ocr")]
    public OcrData? Ocr { get; init; }
}

/// <summary>
/// Barcode data detected in an image.
/// </summary>
public record BarcodeData
{
    /// <summary>
    /// Type of barcode.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Value/text of the barcode.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>
    /// X coordinate of barcode location.
    /// </summary>
    [JsonPropertyName("x")]
    public int? X { get; init; }

    /// <summary>
    /// Y coordinate of barcode location.
    /// </summary>
    [JsonPropertyName("y")]
    public int? Y { get; init; }

    /// <summary>
    /// Width of the barcode region.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; init; }

    /// <summary>
    /// Height of the barcode region.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; init; }

    /// <summary>
    /// Confidence level of the detection (0-100).
    /// </summary>
    [JsonPropertyName("confidence")]
    public int? Confidence { get; init; }
}

/// <summary>
/// Patch code data detected in an image.
/// </summary>
public record PatchCodeData
{
    /// <summary>
    /// Type of patch code (e.g., "patch1", "patch2", "patch3", "patch4", "patch6", "patchT").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// X coordinate of patch code location.
    /// </summary>
    [JsonPropertyName("x")]
    public int? X { get; init; }

    /// <summary>
    /// Y coordinate of patch code location.
    /// </summary>
    [JsonPropertyName("y")]
    public int? Y { get; init; }

    /// <summary>
    /// Width of the patch code region.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; init; }

    /// <summary>
    /// Height of the patch code region.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; init; }
}

/// <summary>
/// MICR (Magnetic Ink Character Recognition) data detected in an image.
/// </summary>
public record MicrData
{
    /// <summary>
    /// Full MICR line text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Routing number.
    /// </summary>
    [JsonPropertyName("routingNumber")]
    public string? RoutingNumber { get; init; }

    /// <summary>
    /// Account number.
    /// </summary>
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; init; }

    /// <summary>
    /// Check number.
    /// </summary>
    [JsonPropertyName("checkNumber")]
    public string? CheckNumber { get; init; }

    /// <summary>
    /// Amount field.
    /// </summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    /// <summary>
    /// Confidence level of the detection (0-100).
    /// </summary>
    [JsonPropertyName("confidence")]
    public int? Confidence { get; init; }
}

/// <summary>
/// OCR data detected in an image.
/// </summary>
public record OcrData
{
    /// <summary>
    /// Detected text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Language of the text.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>
    /// Confidence level of the detection (0-100).
    /// </summary>
    [JsonPropertyName("confidence")]
    public int? Confidence { get; init; }

    /// <summary>
    /// Individual words detected.
    /// </summary>
    [JsonPropertyName("words")]
    public OcrWord[]? Words { get; init; }
}

/// <summary>
/// A single word detected by OCR.
/// </summary>
public record OcrWord
{
    /// <summary>
    /// The word text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// X coordinate of the word.
    /// </summary>
    [JsonPropertyName("x")]
    public int? X { get; init; }

    /// <summary>
    /// Y coordinate of the word.
    /// </summary>
    [JsonPropertyName("y")]
    public int? Y { get; init; }

    /// <summary>
    /// Width of the word bounding box.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; init; }

    /// <summary>
    /// Height of the word bounding box.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; init; }

    /// <summary>
    /// Confidence level (0-100).
    /// </summary>
    [JsonPropertyName("confidence")]
    public int? Confidence { get; init; }
}
