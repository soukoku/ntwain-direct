using System.IO.Compression;

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Shared utility methods for PDF/raster operations.
/// </summary>
public static class RasterUtilities
{
    /// <summary>
    /// Compress data using zlib/Flate compression.
    /// </summary>
    public static byte[] CompressFlate(byte[] data)
    {
        return CompressFlate(data, 0, data.Length);
    }

    /// <summary>
    /// Compress data using zlib/Flate compression.
    /// </summary>
    public static byte[] CompressFlate(byte[] data, int offset, int count)
    {
        using var output = new MemoryStream();
        using (var deflate = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, offset, count);
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decompress zlib/Flate data.
    /// </summary>
    public static byte[] DecompressFlate(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var inflate = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        inflate.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Get bits per component for a pixel format.
    /// </summary>
    public static int GetBitsPerComponent(RasterPixelFormat format) => format switch
    {
        RasterPixelFormat.Bitonal => 1,
        RasterPixelFormat.Gray8 or RasterPixelFormat.Rgb24 => 8,
        RasterPixelFormat.Gray16 or RasterPixelFormat.Rgb48 => 16,
        _ => 8
    };

    /// <summary>
    /// Get number of color components for a pixel format.
    /// </summary>
    public static int GetComponents(RasterPixelFormat format) => format switch
    {
        RasterPixelFormat.Bitonal or RasterPixelFormat.Gray8 or RasterPixelFormat.Gray16 => 1,
        RasterPixelFormat.Rgb24 or RasterPixelFormat.Rgb48 => 3,
        _ => 1
    };

    /// <summary>
    /// Calculate raw data size for an image.
    /// </summary>
    public static int CalculateRawSize(int width, int height, RasterPixelFormat format)
    {
        int bitsPerPixel = GetBitsPerComponent(format) * GetComponents(format);
        int bitsPerRow = width * bitsPerPixel;
        int bytesPerRow = (bitsPerRow + 7) / 8;
        return bytesPerRow * height;
    }

    /// <summary>
    /// Determine if a format is grayscale.
    /// </summary>
    public static bool IsGrayscale(RasterPixelFormat format) =>
        format is RasterPixelFormat.Bitonal or RasterPixelFormat.Gray8 or RasterPixelFormat.Gray16;

    /// <summary>
    /// Determine if a format is RGB color.
    /// </summary>
    public static bool IsRgb(RasterPixelFormat format) =>
        format is RasterPixelFormat.Rgb24 or RasterPixelFormat.Rgb48;
}
