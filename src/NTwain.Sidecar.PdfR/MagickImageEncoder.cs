using ImageMagick;

namespace NTwain.Sidecar.PdfR;

/// <summary>
/// Image encoder using Magick.NET for PDF/raster compression.
/// </summary>
internal static class MagickImageEncoder
{
    /// <summary>
    /// Encodes image data using the specified compression.
    /// </summary>
    /// <param name="image">The image to encode.</param>
    /// <returns>Encoded image data suitable for embedding in PDF.</returns>
    public static byte[] Encode(PdfRasterImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using var magickImage = CreateMagickImage(image);

        return image.Compression switch
        {
            PdfRasterCompression.None => EncodeRaw(magickImage, image.PixelFormat),
            PdfRasterCompression.CcittGroup4 => EncodeCcittGroup4(magickImage),
            PdfRasterCompression.Jpeg => EncodeJpeg(magickImage, image.JpegQuality),
            _ => throw new NotSupportedException($"Compression {image.Compression} is not supported.")
        };
    }

    /// <summary>
    /// Decodes image data from encoded bytes.
    /// </summary>
    /// <param name="data">Encoded image data.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="pixelFormat">Target pixel format.</param>
    /// <param name="compression">Compression used.</param>
    /// <returns>Raw pixel data.</returns>
    public static byte[] Decode(byte[] data, int width, int height, PdfRasterPixelFormat pixelFormat, PdfRasterCompression compression)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (compression == PdfRasterCompression.None)
        {
            return data;
        }

        using var magickImage = new MagickImage(data);
        return ExtractPixelData(magickImage, pixelFormat);
    }

    private static MagickImage CreateMagickImage(PdfRasterImage image)
    {
        var (pixelMapping, storageType) = GetPixelMappingAndStorage(image.PixelFormat);
        var settings = new MagickReadSettings
        {
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Format = MagickFormat.Raw,
        };

        // Set depth based on pixel format
        settings.Depth = image.PixelFormat switch
        {
            PdfRasterPixelFormat.Gray16 or PdfRasterPixelFormat.Rgb48 => 16,
            _ => 8
        };

        var magickImage = new MagickImage();

        // Read raw pixel data
        var readSettings = new PixelReadSettings(
            (uint)image.Width,
            (uint)image.Height,
            storageType,
            pixelMapping);

        magickImage.ReadPixels(image.PixelData, readSettings);

        // Set resolution
        magickImage.Density = new Density(image.HorizontalDpi, image.VerticalDpi, DensityUnit.PixelsPerInch);

        // For 1-bit images, ensure proper colorspace
        if (image.PixelFormat == PdfRasterPixelFormat.BlackWhite1)
        {
            magickImage.ColorType = ColorType.Bilevel;
            magickImage.Depth = 1;
        }

        return magickImage;
    }

    private static (string Mapping, StorageType Storage) GetPixelMappingAndStorage(PdfRasterPixelFormat format)
    {
        return format switch
        {
            PdfRasterPixelFormat.BlackWhite1 => ("R", StorageType.Char),
            PdfRasterPixelFormat.Gray8 => ("R", StorageType.Char),
            PdfRasterPixelFormat.Gray16 => ("R", StorageType.Short),
            PdfRasterPixelFormat.Rgb24 => ("RGB", StorageType.Char),
            PdfRasterPixelFormat.Rgb48 => ("RGB", StorageType.Short),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static byte[] EncodeRaw(MagickImage image, PdfRasterPixelFormat format)
    {
        var (mapping, storageType) = GetPixelMappingAndStorage(format);
        using var pixels = image.GetPixels();
        return pixels.ToByteArray(mapping) ?? [];
    }

    private static byte[] EncodeCcittGroup4(MagickImage image)
    {
        // Ensure image is bilevel for Group 4
        image.ColorType = ColorType.Bilevel;
        image.Depth = 1;

        // Use Group4 compression via TIFF format, then extract the compressed data
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

    private static byte[] ExtractPixelData(MagickImage image, PdfRasterPixelFormat format)
    {
        var (mapping, _) = GetPixelMappingAndStorage(format);

        // Convert to appropriate depth if needed
        switch (format)
        {
            case PdfRasterPixelFormat.BlackWhite1:
                image.ColorType = ColorType.Bilevel;
                image.Depth = 1;
                break;
            case PdfRasterPixelFormat.Gray8:
            case PdfRasterPixelFormat.Rgb24:
                image.Depth = 8;
                break;
            case PdfRasterPixelFormat.Gray16:
            case PdfRasterPixelFormat.Rgb48:
                image.Depth = 16;
                break;
        }

        using var pixels = image.GetPixels();
        return pixels.ToByteArray(mapping) ?? [];
    }
}
