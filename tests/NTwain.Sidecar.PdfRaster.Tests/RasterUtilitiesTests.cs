using NTwain.Sidecar.PdfRaster;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Tests for <see cref="RasterUtilities"/> class.
/// </summary>
public class RasterUtilitiesTests
{
    [Theory]
    [InlineData(RasterPixelFormat.Bitonal, 1)]
    [InlineData(RasterPixelFormat.Gray8, 8)]
    [InlineData(RasterPixelFormat.Gray16, 16)]
    [InlineData(RasterPixelFormat.Rgb24, 8)]
    [InlineData(RasterPixelFormat.Rgb48, 16)]
    public void GetBitsPerComponent_ReturnsCorrectValue(RasterPixelFormat format, int expected)
    {
        var result = RasterUtilities.GetBitsPerComponent(format);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(RasterPixelFormat.Bitonal, 1)]
    [InlineData(RasterPixelFormat.Gray8, 1)]
    [InlineData(RasterPixelFormat.Gray16, 1)]
    [InlineData(RasterPixelFormat.Rgb24, 3)]
    [InlineData(RasterPixelFormat.Rgb48, 3)]
    public void GetComponents_ReturnsCorrectValue(RasterPixelFormat format, int expected)
    {
        var result = RasterUtilities.GetComponents(format);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(RasterPixelFormat.Bitonal, true)]
    [InlineData(RasterPixelFormat.Gray8, true)]
    [InlineData(RasterPixelFormat.Gray16, true)]
    [InlineData(RasterPixelFormat.Rgb24, false)]
    [InlineData(RasterPixelFormat.Rgb48, false)]
    public void IsGrayscale_ReturnsCorrectValue(RasterPixelFormat format, bool expected)
    {
        var result = RasterUtilities.IsGrayscale(format);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(RasterPixelFormat.Bitonal, false)]
    [InlineData(RasterPixelFormat.Gray8, false)]
    [InlineData(RasterPixelFormat.Gray16, false)]
    [InlineData(RasterPixelFormat.Rgb24, true)]
    [InlineData(RasterPixelFormat.Rgb48, true)]
    public void IsRgb_ReturnsCorrectValue(RasterPixelFormat format, bool expected)
    {
        var result = RasterUtilities.IsRgb(format);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CompressFlate_AndDecompressFlate_RoundTrips()
    {
        // Arrange
        var original = new byte[1000];
        Random.Shared.NextBytes(original);

        // Act
        var compressed = RasterUtilities.CompressFlate(original);
        var decompressed = RasterUtilities.DecompressFlate(compressed);

        // Assert
        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void CompressFlate_ReducesSize_ForRepetitiveData()
    {
        // Arrange
        var original = new byte[10000];
        // Fill with repetitive pattern (compresses well)
        for (int i = 0; i < original.Length; i++)
        {
            original[i] = (byte)(i % 10);
        }

        // Act
        var compressed = RasterUtilities.CompressFlate(original);

        // Assert
        Assert.True(compressed.Length < original.Length, 
            $"Compressed size ({compressed.Length}) should be less than original ({original.Length})");
    }

    [Fact]
    public void CalculateRawSize_Gray8_ReturnsCorrectSize()
    {
        var size = RasterUtilities.CalculateRawSize(100, 100, RasterPixelFormat.Gray8);
        Assert.Equal(10000, size); // 100 * 100 * 1 byte
    }

    [Fact]
    public void CalculateRawSize_Rgb24_ReturnsCorrectSize()
    {
        var size = RasterUtilities.CalculateRawSize(100, 100, RasterPixelFormat.Rgb24);
        Assert.Equal(30000, size); // 100 * 100 * 3 bytes
    }

    [Fact]
    public void CalculateRawSize_Bitonal_ReturnsCorrectSize()
    {
        // For 100 pixels wide, need 13 bytes per row (100/8 = 12.5, rounded up)
        var size = RasterUtilities.CalculateRawSize(100, 100, RasterPixelFormat.Bitonal);
        Assert.Equal(1300, size); // 13 * 100
    }

    [Fact]
    public void CalculateRawSize_BitonalExactWidth_ReturnsCorrectSize()
    {
        // For 8 pixels wide, need exactly 1 byte per row
        var size = RasterUtilities.CalculateRawSize(8, 10, RasterPixelFormat.Bitonal);
        Assert.Equal(10, size); // 1 * 10
    }
}
