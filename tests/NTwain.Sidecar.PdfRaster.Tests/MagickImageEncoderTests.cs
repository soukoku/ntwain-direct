using NTwain.Sidecar.PdfRaster;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Tests for <see cref="MagickImageEncoder"/>.
/// </summary>
public class MagickImageEncoderTests
{
    [Fact]
    public void Encode_WithNullPixelData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            MagickImageEncoder.Encode(null!, 10, 10, RasterPixelFormat.Gray8, RasterCompression.Uncompressed));
    }

    [Fact]
    public void Decode_WithNullData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            MagickImageEncoder.Decode(null!, 10, 10, RasterPixelFormat.Gray8, RasterCompression.Uncompressed));
    }

    [Fact]
    public void Encode_Uncompressed_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Uncompressed);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Decode_Uncompressed_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Decode(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Uncompressed);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Encode_Flate_ReducesSize()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Flate);

        // Assert
        Assert.True(result.Length < pixelData.Length, 
            "Flate compressed data should be smaller than original");
    }

    [Fact]
    public void Encode_Decode_Flate_RoundTrips()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var encoded = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Flate);
        var decoded = MagickImageEncoder.Decode(encoded, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Flate);

        // Assert
        Assert.Equal(pixelData, decoded);
    }

    [Fact]
    public void Encode_Jpeg_ProducesJpegData()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterCompression.Jpeg, jpegQuality: 85);

        // Assert
        // JPEG files start with FFD8
        Assert.True(result.Length >= 2);
        Assert.Equal(0xFF, result[0]);
        Assert.Equal(0xD8, result[1]);
    }

    [Fact]
    public void Encode_JpegRgb24_ProducesJpegData()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateRgb24TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Rgb24, RasterCompression.Jpeg, jpegQuality: 85);

        // Assert
        Assert.True(result.Length >= 2);
        Assert.Equal(0xFF, result[0]);
        Assert.Equal(0xD8, result[1]);
    }

    [Fact]
    public void ConvertFormat_SameFormat_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.ConvertFormat(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterPixelFormat.Gray8);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void ConvertFormat_Gray8ToRgb24_IncreasesSize()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.ConvertFormat(pixelData, width, height, 
            RasterPixelFormat.Gray8, RasterPixelFormat.Rgb24);

        // Assert
        Assert.Equal(pixelData.Length * 3, result.Length);
    }

    [Fact]
    public void Rotate_0Degrees_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Rotate(pixelData, width, height, 
            RasterPixelFormat.Gray8, 0, out var newWidth, out var newHeight);

        // Assert
        Assert.Equal(width, newWidth);
        Assert.Equal(height, newHeight);
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Rotate_90Degrees_SwapsDimensions()
    {
        // Arrange
        var width = 20;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Rotate(pixelData, width, height, 
            RasterPixelFormat.Gray8, 90, out var newWidth, out var newHeight);

        // Assert
        Assert.Equal(height, newWidth);
        Assert.Equal(width, newHeight);
    }

    [Fact]
    public void Scale_ProducesCorrectSize()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var newWidth = 50;
        var newHeight = 50;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Scale(pixelData, width, height, 
            RasterPixelFormat.Gray8, newWidth, newHeight);

        // Assert
        Assert.Equal(newWidth * newHeight, result.Length);
    }

    [Fact]
    public void Scale_Rgb24_ProducesCorrectSize()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var newWidth = 50;
        var newHeight = 50;
        var pixelData = CreateRgb24TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Scale(pixelData, width, height, 
            RasterPixelFormat.Rgb24, newWidth, newHeight);

        // Assert
        Assert.Equal(newWidth * newHeight * 3, result.Length);
    }

    #region Bitonal Tests

    [Fact]
    public void Encode_BitonalUncompressed_ReturnsSameData()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.Uncompressed);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Encode_BitonalFlate_ProducesCompressedData()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.Flate);

        // Assert
        Assert.NotEqual(pixelData.Length, result.Length);
    }

    [Fact]
    public void Encode_Decode_BitonalFlate_RoundTrips()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var encoded = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.Flate);
        var decoded = MagickImageEncoder.Decode(encoded, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.Flate);

        // Assert
        Assert.Equal(pixelData, decoded);
    }

    [Fact]
    public void Encode_BitonalCcittGroup4_ProducesCompressedData()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.CcittGroup4);

        // Assert
        Assert.True(result.Length > 0, "Should produce compressed data");
        // CCITT Group 4 typically produces smaller output for bitonal images
        Assert.True(result.Length < pixelData.Length * 2, 
            "CCITT compressed data should not be much larger than original");
    }

    [Fact]
    public void Encode_BitonalCcittGroup4_WithCheckerboard_ProducesData()
    {
        // Arrange - checkerboard pattern is worst case for CCITT
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.CcittGroup4);

        // Assert
        Assert.True(result.Length > 0, "Should produce output data");
    }

    [Fact]
    public void Encode_BitonalCcittGroup4_WithSolidWhite_CompressesWell()
    {
        // Arrange - solid white compresses very well
        var width = 100;
        var height = 100;
        var bytesPerRow = (width + 7) / 8;
        var pixelData = new byte[bytesPerRow * height]; // All zeros = all white

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.CcittGroup4);

        // Assert
        Assert.True(result.Length < pixelData.Length, 
            "Solid image should compress significantly with CCITT");
    }

    [Fact]
    public void Encode_Bitonal_WithOddWidth_HandlesCorrectly()
    {
        // Arrange - odd width that doesn't align to byte boundary
        var width = 73;
        var height = 50;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Bitonal, RasterCompression.CcittGroup4);

        // Assert
        Assert.True(result.Length > 0, "Should handle odd widths");
    }

    [Fact]
    public void Rotate_Bitonal_SwapsDimensions()
    {
        // Arrange
        var width = 80;
        var height = 40;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Rotate(pixelData, width, height, 
            RasterPixelFormat.Bitonal, 90, out var newWidth, out var newHeight);

        // Assert
        Assert.Equal(height, newWidth);
        Assert.Equal(width, newHeight);
    }

    [Fact]
    public void Scale_Bitonal_ProducesCorrectSize()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var newWidth = 50;
        var newHeight = 50;
        var pixelData = CreateBitonalTestImage(width, height);

        // Act
        var result = MagickImageEncoder.Scale(pixelData, width, height, 
            RasterPixelFormat.Bitonal, newWidth, newHeight);

        // Assert
        var expectedBytesPerRow = (newWidth + 7) / 8;
        Assert.Equal(expectedBytesPerRow * newHeight, result.Length);
    }

    #endregion

    #region Gray16 and Rgb48 Tests

    [Fact]
    public void Encode_Gray16_Uncompressed_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray16TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray16, RasterCompression.Uncompressed);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Encode_Decode_Gray16Flate_RoundTrips()
    {
        // Arrange
        var width = 20;
        var height = 20;
        var pixelData = CreateGray16TestImage(width, height);

        // Act
        var encoded = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Gray16, RasterCompression.Flate);
        var decoded = MagickImageEncoder.Decode(encoded, width, height, 
            RasterPixelFormat.Gray16, RasterCompression.Flate);

        // Assert
        Assert.Equal(pixelData, decoded);
    }

    [Fact]
    public void Encode_Rgb48_Uncompressed_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateRgb48TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Encode(pixelData, width, height, 
            RasterPixelFormat.Rgb48, RasterCompression.Uncompressed);

        // Assert
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Scale_Gray16_ProducesCorrectSize()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var newWidth = 25;
        var newHeight = 25;
        var pixelData = CreateGray16TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Scale(pixelData, width, height, 
            RasterPixelFormat.Gray16, newWidth, newHeight);

        // Assert
        Assert.Equal(newWidth * newHeight * 2, result.Length);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Rotate_360Degrees_ReturnsSameData()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act
        var result = MagickImageEncoder.Rotate(pixelData, width, height, 
            RasterPixelFormat.Gray8, 360, out var newWidth, out var newHeight);

        // Assert
        Assert.Equal(width, newWidth);
        Assert.Equal(height, newHeight);
        Assert.Equal(pixelData, result);
    }

    [Fact]
    public void Rotate_NegativeDegrees_HandlesCorrectly()
    {
        // Arrange
        var width = 20;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act - -90 should be same as 270
        var result = MagickImageEncoder.Rotate(pixelData, width, height, 
            RasterPixelFormat.Gray8, -90, out var newWidth, out var newHeight);

        // Assert
        Assert.Equal(height, newWidth);
        Assert.Equal(width, newHeight);
    }

    [Fact]
    public void Encode_UnsupportedCompression_ThrowsNotSupportedException()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => 
            MagickImageEncoder.Encode(pixelData, width, height, 
                RasterPixelFormat.Gray8, (RasterCompression)999));
    }

    [Fact]
    public void Decode_UnsupportedCompression_ThrowsNotSupportedException()
    {
        // Arrange
        var pixelData = new byte[100];

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => 
            MagickImageEncoder.Decode(pixelData, 10, 10, 
                RasterPixelFormat.Gray8, (RasterCompression)999));
    }

    #endregion

    #region Helper Methods

    private static byte[] CreateGray8TestImage(int width, int height)
    {
        var data = new byte[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                data[y * width + x] = (byte)((x + y) % 256);
            }
        }
        return data;
    }

    private static byte[] CreateGray16TestImage(int width, int height)
    {
        var data = new byte[width * height * 2];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 2;
                var value = (ushort)((x + y) * 256);
                data[offset] = (byte)(value >> 8);
                data[offset + 1] = (byte)(value & 0xFF);
            }
        }
        return data;
    }

    private static byte[] CreateRgb24TestImage(int width, int height)
    {
        var data = new byte[width * height * 3];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 3;
                data[offset] = (byte)(x * 255 / width);
                data[offset + 1] = (byte)(y * 255 / height);
                data[offset + 2] = 128;
            }
        }
        return data;
    }

    private static byte[] CreateRgb48TestImage(int width, int height)
    {
        var data = new byte[width * height * 6];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 6;
                var r = (ushort)(x * 65535 / width);
                var g = (ushort)(y * 65535 / height);
                var b = (ushort)32768;
                
                data[offset] = (byte)(r >> 8);
                data[offset + 1] = (byte)(r & 0xFF);
                data[offset + 2] = (byte)(g >> 8);
                data[offset + 3] = (byte)(g & 0xFF);
                data[offset + 4] = (byte)(b >> 8);
                data[offset + 5] = (byte)(b & 0xFF);
            }
        }
        return data;
    }

    private static byte[] CreateBitonalTestImage(int width, int height)
    {
        var bytesPerRow = (width + 7) / 8;
        var data = new byte[bytesPerRow * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Create a checkerboard pattern
                if ((x + y) % 2 == 0)
                {
                    var byteIndex = y * bytesPerRow + x / 8;
                    var bitIndex = 7 - (x % 8);
                    data[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }
        return data;
    }

    #endregion
}
