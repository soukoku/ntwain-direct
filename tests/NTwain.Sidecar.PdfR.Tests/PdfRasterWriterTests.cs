using Xunit;

namespace NTwain.Sidecar.PdfR.Tests;

/// <summary>
/// Tests for <see cref="PdfRasterWriter"/>.
/// </summary>
public class PdfRasterWriterTests
{
    [Fact]
    public void Constructor_WithNullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PdfRasterWriter(null!));
    }

    [Fact]
    public void Constructor_WithNonWritableStream_ThrowsArgumentException()
    {
        using var stream = new MemoryStream([], writable: false);

        Assert.Throws<ArgumentException>(() => new PdfRasterWriter(stream));
    }

    [Fact]
    public void AddPage_WithNullImage_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();
        using var writer = new PdfRasterWriter(stream);

        Assert.Throws<ArgumentNullException>(() => writer.AddPage(null!));
    }

    [Fact]
    public void AddPage_WithCcittOnNonBw_ThrowsArgumentException()
    {
        using var stream = new MemoryStream();
        using var writer = new PdfRasterWriter(stream);

        var image = new PdfRasterImage(
            new byte[100],
            Width: 10,
            Height: 10,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.CcittGroup4);

        Assert.Throws<ArgumentException>(() => writer.AddPage(image));
    }

    [Fact]
    public void AddPage_WithJpegOnBw_ThrowsArgumentException()
    {
        using var stream = new MemoryStream();
        using var writer = new PdfRasterWriter(stream);

        var image = new PdfRasterImage(
            new byte[100],
            Width: 10,
            Height: 10,
            PixelFormat: PdfRasterPixelFormat.BlackWhite1,
            Compression: PdfRasterCompression.Jpeg);

        Assert.Throws<ArgumentException>(() => writer.AddPage(image));
    }

    [Fact]
    public void Write_UncompressedGray8_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateGray8TestImage(width, height);

        var image = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.None,
            HorizontalDpi: 200,
            VerticalDpi: 200);

        using var outputStream = new MemoryStream();

        // Act
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            writer.AddPage(image);
            writer.Finish();
        }

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();

        Assert.True(pdfData.Length > 0, "PDF should have content");
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdfData, 0, 5));
        Assert.Contains("%%EOF", System.Text.Encoding.ASCII.GetString(pdfData));
    }

    [Fact]
    public void Write_JpegCompressedGray8_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateGray8TestImage(width, height);

        var image = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.Jpeg,
            HorizontalDpi: 300,
            VerticalDpi: 300)
        { JpegQuality = 85 };

        using var outputStream = new MemoryStream();

        // Act
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            writer.AddPage(image);
            writer.Finish();
        }

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdfData, 0, 5));
        Assert.Contains("/Filter /DCTDecode", System.Text.Encoding.ASCII.GetString(pdfData));
    }

    [Fact]
    public void Write_UncompressedRgb24_ProducesValidPdf()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateRgb24TestImage(width, height);

        var image = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: PdfRasterPixelFormat.Rgb24,
            Compression: PdfRasterCompression.None,
            HorizontalDpi: 150,
            VerticalDpi: 150);

        using var outputStream = new MemoryStream();

        // Act
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            writer.AddPage(image);
            writer.Finish();
        }

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();

        Assert.True(pdfData.Length > 0);
        Assert.Contains("/ColorSpace /DeviceRGB", System.Text.Encoding.ASCII.GetString(pdfData));
    }

    [Fact]
    public void Write_MultiplePages_ProducesValidPdf()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateGray8TestImage(width, height);

        var image = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.None);

        using var outputStream = new MemoryStream();

        // Act
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            writer.AddPage(image);
            writer.AddPage(image);
            writer.AddPage(image);
            writer.Finish();
        }

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("/Count 3", pdfText);
    }

    [Fact]
    public void Dispose_AutomaticallyFinishesPdf()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        var image = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: PdfRasterPixelFormat.Gray8,
            Compression: PdfRasterCompression.None);

        using var outputStream = new MemoryStream();

        // Act
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            writer.AddPage(image);
            // Don't call Finish() - let Dispose handle it
        }

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("%%EOF", pdfText);
    }

    private static byte[] CreateGray8TestImage(int width, int height)
    {
        var data = new byte[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Create a gradient pattern
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
                data[offset] = (byte)(x * 255 / width);     // R
                data[offset + 1] = (byte)(y * 255 / height); // G
                data[offset + 2] = 128;                       // B
            }
        }
        return data;
    }
}
