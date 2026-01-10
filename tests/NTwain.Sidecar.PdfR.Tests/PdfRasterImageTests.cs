using Xunit;

namespace NTwain.Sidecar.PdfR.Tests;

/// <summary>
/// Tests for <see cref="PdfRasterImage"/> record.
/// </summary>
public class PdfRasterImageTests
{
    [Fact]
    public void DefaultDpi_Is200()
    {
        // Arrange & Act
        var image = new PdfRasterImage(
            PixelData: [1],
            Width: 1,
            Height: 1,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.None);

        // Assert
        Assert.Equal(200, image.HorizontalDpi);
        Assert.Equal(200, image.VerticalDpi);
    }

    [Fact]
    public void DefaultJpegQuality_Is85()
    {
        // Arrange & Act
        var image = new PdfRasterImage(
            PixelData: [1],
            Width: 1,
            Height: 1,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.Jpeg);

        // Assert
        Assert.Equal(85, image.JpegQuality);
    }

    [Fact]
    public void JpegQuality_CanBeOverridden()
    {
        // Arrange & Act
        var image = new PdfRasterImage(
            PixelData: [1],
            Width: 1,
            Height: 1,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.Jpeg)
        { JpegQuality = 95 };

        // Assert
        Assert.Equal(95, image.JpegQuality);
    }

    [Fact]
    public void Record_SupportsWithExpression()
    {
        // Arrange
        var original = new PdfRasterImage(
            PixelData: [1, 2, 3],
            Width: 3,
            Height: 1,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.None);

        // Act
        var modified = original with { Compression = PdfRasterCompression.Jpeg };

        // Assert
        Assert.Equal(PdfRasterCompression.None, original.Compression);
        Assert.Equal(PdfRasterCompression.Jpeg, modified.Compression);
        Assert.Same(original.PixelData, modified.PixelData);
    }
}
