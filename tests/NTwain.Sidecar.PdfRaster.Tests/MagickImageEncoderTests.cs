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
}
