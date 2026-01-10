using ImageMagick;

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Image encoder using Magick.NET for PDF/raster compression.
/// Provides high-level encoding/decoding for complete images.
/// </summary>
public static class MagickImageEncoder
{
    /// <summary>
    /// Encodes raw pixel data to compressed format.
    /// </summary>
    /// <param name="pixelData">Raw pixel data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="format">Pixel format</param>
    /// <param name="compression">Target compression</param>
    /// <param name="jpegQuality">JPEG quality (1-100) when using JPEG compression</param>
    /// <returns>Encoded image data suitable for embedding in PDF.</returns>
    public static byte[] Encode(byte[] pixelData, int width, int height, 
        RasterPixelFormat format, RasterCompression compression, int jpegQuality = 85)
    {
        ArgumentNullException.ThrowIfNull(pixelData);

        using var magickImage = CreateMagickImage(pixelData, width, height, format);

        return compression switch
        {
            RasterCompression.Uncompressed => pixelData,
            RasterCompression.CcittGroup4 => EncodeCcittGroup4(magickImage),
            RasterCompression.Jpeg => EncodeJpeg(magickImage, jpegQuality),
            RasterCompression.Flate => RasterUtilities.CompressFlate(pixelData),
            _ => throw new NotSupportedException($"Compression {compression} is not supported for encoding.")
        };
    }

    /// <summary>
    /// Decodes image data from encoded bytes to raw pixels.
    /// </summary>
    /// <param name="data">Encoded image data.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="pixelFormat">Target pixel format.</param>
    /// <param name="compression">Compression used.</param>
    /// <returns>Raw pixel data.</returns>
    public static byte[] Decode(byte[] data, int width, int height, 
        RasterPixelFormat pixelFormat, RasterCompression compression)
    {
        ArgumentNullException.ThrowIfNull(data);

        return compression switch
        {
            RasterCompression.Uncompressed => data,
            RasterCompression.Flate => RasterUtilities.DecompressFlate(data),
            RasterCompression.Jpeg => DecodeJpeg(data, pixelFormat),
            RasterCompression.CcittGroup4 => DecodeCcittGroup4(data, width, height, pixelFormat),
            _ => throw new NotSupportedException($"Compression {compression} is not supported for decoding.")
        };
    }

    private static MagickImage CreateMagickImage(byte[] pixelData, int width, int height, RasterPixelFormat format)
    {
        var (pixelMapping, storageType) = GetPixelMappingAndStorage(format);
        
        var magickImage = new MagickImage();

        // Read raw pixel data
        var readSettings = new PixelReadSettings(
            (uint)width,
            (uint)height,
            storageType,
            pixelMapping);

        magickImage.ReadPixels(pixelData, readSettings);

        // Set depth based on pixel format
        magickImage.Depth = format switch
        {
            RasterPixelFormat.Gray16 or RasterPixelFormat.Rgb48 => 16,
            RasterPixelFormat.Bitonal => 1,
            _ => 8
        };

        // For 1-bit images, ensure proper colorspace
        if (format == RasterPixelFormat.Bitonal)
        {
            magickImage.ColorType = ColorType.Bilevel;
        }

        return magickImage;
    }

    private static (string Mapping, StorageType Storage) GetPixelMappingAndStorage(RasterPixelFormat format)
    {
        return format switch
        {
            RasterPixelFormat.Bitonal => ("R", StorageType.Char),
            RasterPixelFormat.Gray8 => ("R", StorageType.Char),
            RasterPixelFormat.Gray16 => ("R", StorageType.Short),
            RasterPixelFormat.Rgb24 => ("RGB", StorageType.Char),
            RasterPixelFormat.Rgb48 => ("RGB", StorageType.Short),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static byte[] EncodeCcittGroup4(MagickImage image)
    {
        // Ensure image is bilevel for Group 4
        image.ColorType = ColorType.Bilevel;
        image.Depth = 1;
        image.Settings.Compression = CompressionMethod.Group4;
        image.Format = MagickFormat.Group4;

        using var stream = new MemoryStream();
        image.Write(stream);
        return stream.ToArray();
    }

    private static byte[] EncodeJpeg(MagickImage image, int quality)
    {
        image.Quality = (uint)Math.Clamp(quality, 1, 100);
        image.Format = MagickFormat.Jpeg;

        using var stream = new MemoryStream();
        image.Write(stream);
        return stream.ToArray();
    }

    private static byte[] DecodeJpeg(byte[] data, RasterPixelFormat format)
    {
        using var image = new MagickImage(data);
        return ExtractPixelData(image, format);
    }

    private static byte[] DecodeCcittGroup4(byte[] data, int width, int height, RasterPixelFormat format)
    {
        var settings = new MagickReadSettings
        {
            Width = (uint)width,
            Height = (uint)height,
            Format = MagickFormat.Group4
        };

        using var image = new MagickImage(data, settings);
        return ExtractPixelData(image, format);
    }

    private static byte[] ExtractPixelData(MagickImage image, RasterPixelFormat format)
    {
        var (mapping, _) = GetPixelMappingAndStorage(format);

        // Convert to appropriate depth if needed
        switch (format)
        {
            case RasterPixelFormat.Bitonal:
                image.ColorType = ColorType.Bilevel;
                image.Depth = 1;
                break;
            case RasterPixelFormat.Gray8:
            case RasterPixelFormat.Rgb24:
                image.Depth = 8;
                break;
            case RasterPixelFormat.Gray16:
            case RasterPixelFormat.Rgb48:
                image.Depth = 16;
                break;
        }

        using var pixels = image.GetPixels();
        return pixels.ToByteArray(mapping) ?? [];
    }

    /// <summary>
    /// Convert image from one format to another
    /// </summary>
    public static byte[] ConvertFormat(byte[] pixelData, int width, int height,
        RasterPixelFormat sourceFormat, RasterPixelFormat targetFormat)
    {
        if (sourceFormat == targetFormat)
            return pixelData;

        using var image = CreateMagickImage(pixelData, width, height, sourceFormat);
        return ExtractPixelData(image, targetFormat);
    }

    /// <summary>
    /// Apply rotation to image data
    /// </summary>
    public static byte[] Rotate(byte[] pixelData, int width, int height, 
        RasterPixelFormat format, int degrees, out int newWidth, out int newHeight)
    {
        degrees = ((degrees % 360) + 360) % 360;
        
        if (degrees == 0)
        {
            newWidth = width;
            newHeight = height;
            return pixelData;
        }

        using var image = CreateMagickImage(pixelData, width, height, format);
        image.Rotate(degrees);
        
        newWidth = (int)image.Width;
        newHeight = (int)image.Height;
        
        return ExtractPixelData(image, format);
    }

    /// <summary>
    /// Scale image data
    /// </summary>
    public static byte[] Scale(byte[] pixelData, int width, int height,
        RasterPixelFormat format, int newWidth, int newHeight)
    {
        using var image = CreateMagickImage(pixelData, width, height, format);
        image.Resize((uint)newWidth, (uint)newHeight);
        return ExtractPixelData(image, format);
    }
}
