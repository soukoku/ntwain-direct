namespace NTwain.Sidecar.PdfR;

/// <summary>
/// Pixel format for PDF/raster images.
/// </summary>
public enum PdfRasterPixelFormat
{
    /// <summary>
    /// 1-bit black and white.
    /// </summary>
    BlackWhite1,

    /// <summary>
    /// 8-bit grayscale.
    /// </summary>
    Gray8,

    /// <summary>
    /// 16-bit grayscale.
    /// </summary>
    Gray16,

    /// <summary>
    /// 24-bit RGB color.
    /// </summary>
    Rgb24,

    /// <summary>
    /// 48-bit RGB color.
    /// </summary>
    Rgb48
}

/// <summary>
/// Compression type for PDF/raster images.
/// </summary>
public enum PdfRasterCompression
{
    /// <summary>
    /// No compression (raw image data).
    /// </summary>
    None,

    /// <summary>
    /// CCITT Group 4 compression (for 1-bit black and white only).
    /// </summary>
    CcittGroup4,

    /// <summary>
    /// JPEG compression (for grayscale and RGB images).
    /// </summary>
    Jpeg
}

/// <summary>
/// Represents image data to be written to a PDF/raster file.
/// </summary>
/// <param name="PixelData">Raw pixel data in the appropriate format.</param>
/// <param name="Width">Width of the image in pixels.</param>
/// <param name="Height">Height of the image in pixels.</param>
/// <param name="PixelFormat">Pixel format of the image.</param>
/// <param name="Compression">Compression to use for the image.</param>
/// <param name="HorizontalDpi">Horizontal resolution in DPI.</param>
/// <param name="VerticalDpi">Vertical resolution in DPI.</param>
public record PdfRasterImage(
    byte[] PixelData,
    int Width,
    int Height,
    PdfRasterPixelFormat PixelFormat,
    PdfRasterCompression Compression,
    double HorizontalDpi = 200,
    double VerticalDpi = 200)
{
    /// <summary>
    /// JPEG quality (1-100) when using JPEG compression.
    /// </summary>
    public int JpegQuality { get; init; } = 85;
}
