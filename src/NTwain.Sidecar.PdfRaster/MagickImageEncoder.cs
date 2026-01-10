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
        // For bitonal images, we need special handling since the data is bit-packed
        if (format == RasterPixelFormat.Bitonal)
        {
            return CreateBitonalMagickImage(pixelData, width, height);
        }
        
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
            _ => 8
        };

        return magickImage;
    }

    /// <summary>
    /// Create a MagickImage from bit-packed bitonal data
    /// </summary>
    private static MagickImage CreateBitonalMagickImage(byte[] packedData, int width, int height)
    {
        // Unpack 1-bit data to 8-bit grayscale for Magick.NET
        var unpackedData = UnpackBitonalData(packedData, width, height);
        
        var magickImage = new MagickImage();
        
        var readSettings = new PixelReadSettings(
            (uint)width,
            (uint)height,
            StorageType.Char,
            "R");

        magickImage.ReadPixels(unpackedData, readSettings);
        
        // Convert to bilevel
        magickImage.ColorType = ColorType.Bilevel;
        magickImage.Depth = 1;
        
        return magickImage;
    }

    /// <summary>
    /// Unpack bit-packed bitonal data to 8-bit grayscale (0 or 255)
    /// </summary>
    private static byte[] UnpackBitonalData(byte[] packedData, int width, int height)
    {
        var unpacked = new byte[width * height];
        int bytesPerRow = (width + 7) / 8;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int byteIndex = y * bytesPerRow + x / 8;
                int bitIndex = 7 - (x % 8);
                
                if (byteIndex < packedData.Length)
                {
                    bool isSet = (packedData[byteIndex] & (1 << bitIndex)) != 0;
                    // In PDF, 1 = white, 0 = black for bitonal with default decode
                    // But we're using BlackIs1, so 1 = black, 0 = white
                    unpacked[y * width + x] = isSet ? (byte)0 : (byte)255;
                }
            }
        }
        
        return unpacked;
    }

    /// <summary>
    /// Pack 8-bit grayscale data to bit-packed bitonal
    /// </summary>
    private static byte[] PackBitonalData(byte[] unpackedData, int width, int height)
    {
        int bytesPerRow = (width + 7) / 8;
        var packed = new byte[bytesPerRow * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
                int byteIndex = y * bytesPerRow + x / 8;
                int bitIndex = 7 - (x % 8);
                
                if (srcIndex < unpackedData.Length)
                {
                    // Threshold: < 128 is black (1), >= 128 is white (0)
                    if (unpackedData[srcIndex] < 128)
                    {
                        packed[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }
        }
        
        return packed;
    }

    private static (string Mapping, StorageType Storage) GetPixelMappingAndStorage(RasterPixelFormat format)
    {
        return format switch
        {
            RasterPixelFormat.Bitonal => ("R", StorageType.Char), // Not used for bitonal, handled separately
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
        // For bitonal, we need to extract and pack the data
        if (format == RasterPixelFormat.Bitonal)
        {
            return ExtractBitonalPixelData(image);
        }
        
        var (mapping, storageType) = GetPixelMappingAndStorage(format);

        // Convert to appropriate depth if needed
        switch (format)
        {
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
        
        // Use the correct method based on storage type
        if (storageType == StorageType.Short)
        {
            // For 16-bit formats, we need to use ToShortArray and convert to bytes
            var shortArray = pixels.ToShortArray(mapping) ?? [];
            var byteArray = new byte[shortArray.Length * 2];
            Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
        else
        {
            // For 8-bit formats
            return pixels.ToByteArray(mapping) ?? [];
        }
    }

    /// <summary>
    /// Extract pixel data from a bilevel image and pack to 1-bit format
    /// </summary>
    private static byte[] ExtractBitonalPixelData(MagickImage image)
    {
        // Ensure it's grayscale
        image.ColorType = ColorType.Grayscale;
        image.Depth = 8;
        
        using var pixels = image.GetPixels();
        var unpacked = pixels.ToByteArray("R") ?? [];
        
        return PackBitonalData(unpacked, (int)image.Width, (int)image.Height);
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
