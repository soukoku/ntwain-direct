// Image decoder for PDF/raster strip data
// Supports decompression of JPEG, CCITT, and Flate compressed image data

using System.IO.Compression;

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Decodes compressed image data from PDF/raster streams
/// </summary>
public static class ImageDecoder
{
    /// <summary>
    /// Decode strip data based on compression type
    /// </summary>
    /// <param name="data">Compressed data</param>
    /// <param name="compression">Compression type</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="bitsPerComponent">Bits per color component</param>
    /// <param name="components">Number of color components</param>
    /// <returns>Decoded raw pixel data</returns>
    public static byte[] Decode(byte[] data, RasterCompression compression, int width, int height, int bitsPerComponent, int components)
    {
        return compression switch
        {
            RasterCompression.Uncompressed => data,
            RasterCompression.Flate => DecodeFlate(data),
            RasterCompression.Jpeg => DecodeJpeg(data),
            RasterCompression.CcittGroup4 => DecodeCcittGroup4(data, width, height),
            _ => throw new PdfRasterException($"Unsupported compression: {compression}")
        };
    }

    /// <summary>
    /// Decode Flate (zlib) compressed data
    /// </summary>
    public static byte[] DecodeFlate(byte[] data)
    {
        try
        {
            using var input = new MemoryStream(data);
            using var inflate = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            inflate.CopyTo(output);
            return output.ToArray();
        }
        catch (Exception ex)
        {
            throw new PdfRasterException("Failed to decode Flate data", ex);
        }
    }

    /// <summary>
    /// Decode JPEG (DCTDecode) compressed data
    /// </summary>
    /// <remarks>
    /// JPEG data in PDF is stored as a complete JPEG stream.
    /// Returns the raw JPEG bytes which can be decoded by image libraries.
    /// </remarks>
    public static byte[] DecodeJpeg(byte[] data)
    {
        // JPEG data is already a complete JPEG file
        // Most callers will want to use an image library to decode further
        // For raw extraction, we just return the JPEG data
        return data;
    }

    /// <summary>
    /// Decode JPEG data to raw pixels using ImageMagick
    /// </summary>
    public static byte[] DecodeJpegToPixels(byte[] jpegData, out int width, out int height, out int components)
    {
        try
        {
            using var image = new ImageMagick.MagickImage(jpegData);
            width = (int)image.Width;
            height = (int)image.Height;
            
            // Determine components based on colorspace
            components = image.ColorSpace switch
            {
                ImageMagick.ColorSpace.Gray => 1,
                ImageMagick.ColorSpace.sRGB or ImageMagick.ColorSpace.RGB => 3,
                ImageMagick.ColorSpace.CMYK => 4,
                _ => 3
            };
            
            string mapping = components switch
            {
                1 => "R",
                3 => "RGB",
                4 => "CMYK",
                _ => "RGB"
            };
            
            using var pixels = image.GetPixels();
            return pixels.ToByteArray(mapping) ?? [];
        }
        catch (Exception ex)
        {
            throw new PdfRasterException("Failed to decode JPEG to pixels", ex);
        }
    }

    /// <summary>
    /// Decode CCITT Group 4 fax compressed data
    /// </summary>
    public static byte[] DecodeCcittGroup4(byte[] data, int width, int height, bool blackIs1 = true)
    {
        try
        {
            // Use ImageMagick for CCITT Group 4 decoding
            var settings = new ImageMagick.MagickReadSettings
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = ImageMagick.MagickFormat.Group4
            };
            
            using var image = new ImageMagick.MagickImage(data, settings);
            
            // Ensure bilevel output
            image.ColorType = ImageMagick.ColorType.Bilevel;
            image.Depth = 1;
            
            // Extract as raw bits
            using var pixels = image.GetPixels();
            var rawPixels = pixels.ToByteArray("R") ?? [];
            
            // Convert to packed bits (1 bit per pixel)
            return PackBits(rawPixels, width, height, blackIs1);
        }
        catch (Exception ex)
        {
            throw new PdfRasterException("Failed to decode CCITT Group 4 data", ex);
        }
    }

    /// <summary>
    /// Pack 8-bit per pixel grayscale data to 1-bit packed format
    /// </summary>
    private static byte[] PackBits(byte[] bytes, int width, int height, bool blackIs1)
    {
        int rowBytes = (width + 7) / 8;
        var packed = new byte[rowBytes * height];
        
        int srcIndex = 0;
        int dstIndex = 0;
        
        for (int y = 0; y < height; y++)
        {
            int bitIndex = 0;
            byte currentByte = 0;
            
            for (int x = 0; x < width; x++)
            {
                bool isBlack = bytes[srcIndex++] < 128;
                if (!blackIs1) isBlack = !isBlack;
                
                if (isBlack)
                {
                    currentByte |= (byte)(0x80 >> bitIndex);
                }
                
                bitIndex++;
                if (bitIndex == 8)
                {
                    packed[dstIndex++] = currentByte;
                    currentByte = 0;
                    bitIndex = 0;
                }
            }
            
            // Write any remaining bits
            if (bitIndex > 0)
            {
                packed[dstIndex++] = currentByte;
            }
        }
        
        return packed;
    }

    /// <summary>
    /// Unpack 1-bit packed data to 8-bit per pixel grayscale
    /// </summary>
    public static byte[] UnpackBits(byte[] packedData, int width, int height, bool blackIs1 = true)
    {
        var unpacked = new byte[width * height];
        int rowBytes = (width + 7) / 8;
        int dstIndex = 0;
        
        for (int y = 0; y < height; y++)
        {
            int srcIndex = y * rowBytes;
            
            for (int x = 0; x < width; x++)
            {
                int byteIndex = x / 8;
                int bitIndex = 7 - (x % 8);
                
                bool isSet = (packedData[srcIndex + byteIndex] & (1 << bitIndex)) != 0;
                bool isBlack = blackIs1 ? isSet : !isSet;
                
                unpacked[dstIndex++] = isBlack ? (byte)0 : (byte)255;
            }
        }
        
        return unpacked;
    }

    /// <summary>
    /// Calculate the expected raw data size for an image strip
    /// </summary>
    public static int CalculateRawSize(int width, int height, int bitsPerComponent, int components)
    {
        int bitsPerPixel = bitsPerComponent * components;
        int bitsPerRow = width * bitsPerPixel;
        int bytesPerRow = (bitsPerRow + 7) / 8;
        return bytesPerRow * height;
    }

    /// <summary>
    /// Get the number of components for a pixel format
    /// </summary>
    public static int GetComponents(RasterPixelFormat format)
    {
        return format switch
        {
            RasterPixelFormat.Bitonal => 1,
            RasterPixelFormat.Gray8 => 1,
            RasterPixelFormat.Gray16 => 1,
            RasterPixelFormat.Rgb24 => 3,
            RasterPixelFormat.Rgb48 => 3,
            _ => 1
        };
    }

    /// <summary>
    /// Get bits per component for a pixel format
    /// </summary>
    public static int GetBitsPerComponent(RasterPixelFormat format)
    {
        return format switch
        {
            RasterPixelFormat.Bitonal => 1,
            RasterPixelFormat.Gray8 => 8,
            RasterPixelFormat.Gray16 => 16,
            RasterPixelFormat.Rgb24 => 8,
            RasterPixelFormat.Rgb48 => 16,
            _ => 8
        };
    }
}
