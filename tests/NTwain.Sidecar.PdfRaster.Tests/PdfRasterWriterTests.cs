using NTwain.Sidecar.PdfRaster;
using NTwain.Sidecar.PdfRaster.Writer;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Tests for <see cref="PdfRasterWriter"/>.
/// </summary>
public class PdfRasterWriterTests
{
    [Fact]
    public void Begin_WithNullStream_ThrowsArgumentNullException()
    {
        using var writer = new PdfRasterWriter();
        Assert.Throws<ArgumentNullException>(() => writer.Begin(null!));
    }

    [Fact]
    public void Begin_WithNonWritableStream_ThrowsArgumentException()
    {
        using var stream = new MemoryStream([], writable: false);
        using var writer = new PdfRasterWriter();

        Assert.Throws<ArgumentException>(() => writer.Begin(stream));
    }

    [Fact]
    public void WriteStrip_WithoutStartPage_ThrowsException()
    {
        using var stream = new MemoryStream();
        using var writer = new PdfRasterWriter();
        writer.Begin(stream, leaveOpen: true);

        Assert.Throws<PdfApiException>(() => writer.WriteStrip(10, new byte[100]));
    }

    [Fact]
    public void Write_UncompressedGray8_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateGray8TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        writer.SetCompression(RasterCompression.Uncompressed);
        writer.SetResolution(200, 200);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();

        Assert.True(pdfData.Length > 0, "PDF should have content");
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdfData, 0, 5));
        Assert.Contains("%%EOF", System.Text.Encoding.ASCII.GetString(pdfData));
        File.WriteAllBytes("UncompressedGray8.pdf", outputStream.ToArray());
    }

    [Fact]
    public void Write_FlateCompressedGray8_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateGray8TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        writer.SetCompression(RasterCompression.Flate);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/FlateDecode", pdfText);

        File.WriteAllBytes("FlateCompressedGray8.pdf", pdfData);
        
    }

    [Fact]
    public void Write_UncompressedRgb24_ProducesValidPdf()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateRgb24TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Rgb24);
        writer.SetCompression(RasterCompression.Uncompressed);
        writer.SetResolution(150, 150);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("/CalRGB", pdfText);
        File.WriteAllBytes("UncompressedRgb24.pdf", outputStream.ToArray());
    }

    [Fact]
    public void Write_JpegCompressedRgb24_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateRgb24TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Rgb24);
        writer.SetCompression(RasterCompression.Jpeg);
        writer.SetJpegQuality(85);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/DCTDecode", pdfText);
        Assert.Contains("%%EOF", pdfText);
        File.WriteAllBytes("JpegCompressedRgb24.pdf", pdfData);
    }

    [Fact]
    public void Write_JpegCompressedRgb24_WithLowQuality_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateRgb24TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Rgb24);
        writer.SetCompression(RasterCompression.Jpeg);
        writer.SetJpegQuality(50);
        writer.SetResolution(200, 200);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.Contains("/DCTDecode", pdfText);
        File.WriteAllBytes("JpegCompressedRgb24_LowQuality.pdf", pdfData);
    }

    [Fact]
    public void Write_JpegCompressedRgb24_MultipleStrips_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var stripHeight = 25;
        var numStrips = 4;
        var stripData = CreateRgb24TestImage(width, stripHeight);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Rgb24);
        writer.SetCompression(RasterCompression.Jpeg);
        writer.SetJpegQuality(85);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        for (int i = 0; i < numStrips; i++)
        {
            writer.WriteStrip(stripHeight, stripData);
        }
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.Contains("/strip0", pdfText);
        Assert.Contains("/strip3", pdfText);
        Assert.Contains("/DCTDecode", pdfText);
        File.WriteAllBytes("JpegCompressedRgb24_MultipleStrips.pdf", pdfData);
    }

    [Fact]
    public void Write_FlateCompressedRgb24_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateRgb24TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Rgb24);
        writer.SetCompression(RasterCompression.Flate);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/FlateDecode", pdfText);
        Assert.Contains("/CalRGB", pdfText);
        File.WriteAllBytes("FlateCompressedRgb24.pdf", pdfData);
    }

    [Fact]
    public void Write_UncompressedBitonal_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Bitonal);
        writer.SetCompression(RasterCompression.Uncompressed);
        writer.SetResolution(200, 200);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0, "PDF should have content");
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/BitsPerComponent 1", pdfText);
        Assert.Contains("%%EOF", pdfText);
        File.WriteAllBytes("UncompressedBitonal.pdf", pdfData);
    }

    [Fact]
    public void Write_FlateCompressedBitonal_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Bitonal);
        writer.SetCompression(RasterCompression.Flate);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/BitsPerComponent 1", pdfText);
        Assert.Contains("/FlateDecode", pdfText);
        File.WriteAllBytes("FlateCompressedBitonal.pdf", pdfData);
    }

    [Fact]
    public void Write_CcittCompressedBitonal_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var pixelData = CreateBitonalTestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Bitonal);
        writer.SetCompression(RasterCompression.CcittGroup4);
        writer.SetResolution(300, 300);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfData = outputStream.ToArray();
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfData);

        Assert.True(pdfData.Length > 0);
        Assert.StartsWith("%PDF-", pdfText[..5]);
        Assert.Contains("/BitsPerComponent 1", pdfText);
        Assert.Contains("/CCITTFaxDecode", pdfText);
        File.WriteAllBytes("CcittCompressedBitonal.pdf", pdfData);
    }

    [Fact]
    public void Write_BitonalMultipleStrips_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var stripHeight = 25;
        var numStrips = 4;
        var stripData = CreateBitonalTestImage(width, stripHeight);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Bitonal);
        writer.SetCompression(RasterCompression.Uncompressed);
        writer.SetResolution(200, 200);
        
        writer.StartPage(width);
        for (int i = 0; i < numStrips; i++)
        {
            writer.WriteStrip(stripHeight, stripData);
        }
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("/strip0", pdfText);
        Assert.Contains("/strip3", pdfText);
        Assert.Contains("/BitsPerComponent 1", pdfText);
        File.WriteAllBytes("BitonalMultipleStrips.pdf", outputStream.ToArray());
    }

    [Fact]
    public void Write_MultiplePages_ProducesValidPdf()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pixelData = CreateGray8TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        writer.SetCompression(RasterCompression.Uncompressed);
        
        for (int i = 0; i < 3; i++)
        {
            writer.StartPage(width);
            writer.WriteStrip(height, pixelData);
            writer.EndPage();
        }
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("/Count 3", pdfText);
        File.WriteAllBytes("MultiplePages.pdf", outputStream.ToArray());
    }

    [Fact]
    public void Write_MultipleStrips_ProducesValidPdf()
    {
        // Arrange
        var width = 100;
        var stripHeight = 25;
        var numStrips = 4;
        var stripData = CreateGray8TestImage(width, stripHeight);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        writer.SetCompression(RasterCompression.Uncompressed);
        
        writer.StartPage(width);
        for (int i = 0; i < numStrips; i++)
        {
            writer.WriteStrip(stripHeight, stripData);
        }
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        // Should have strip0, strip1, strip2, strip3 XObjects
        Assert.Contains("/strip0", pdfText);
        Assert.Contains("/strip3", pdfText);
        File.WriteAllBytes("MultipleStripes.pdf", outputStream.ToArray());
    }

    [Fact]
    public void PageCount_AfterAddingPages_ReturnsCorrectCount()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act & Assert
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        
        Assert.Equal(0, writer.PageCount);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        Assert.Equal(1, writer.PageCount); // Page in progress counts
        
        writer.EndPage();
        Assert.Equal(1, writer.PageCount);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        Assert.Equal(2, writer.PageCount);
        
        writer.End();
    }

    [Fact]
    public void SetMetadata_IncludesInPdf()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        using var outputStream = new MemoryStream();
        using var writer = new PdfRasterWriter();

        // Act
        writer.Begin(outputStream, leaveOpen: true);
        writer.SetCreator("Test Creator");
        writer.SetAuthor("Test Author");
        writer.SetTitle("Test Title");
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        outputStream.Position = 0;
        var pdfText = System.Text.Encoding.ASCII.GetString(outputStream.ToArray());

        Assert.Contains("/Creator", pdfText);
        Assert.Contains("/Author", pdfText);
        Assert.Contains("/Title", pdfText);
    }

    [Fact]
    public void BeginToMemory_ReturnsUsableStream()
    {
        // Arrange
        var width = 10;
        var height = 10;
        var pixelData = CreateGray8TestImage(width, height);

        using var writer = new PdfRasterWriter();

        // Act
        var stream = writer.BeginToMemory();
        writer.SetPixelFormat(RasterPixelFormat.Gray8);
        writer.StartPage(width);
        writer.WriteStrip(height, pixelData);
        writer.EndPage();
        writer.End();

        // Assert
        Assert.True(stream.Length > 0);
        stream.Position = 0;
        var header = new byte[5];
        stream.Read(header, 0, 5);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(header));
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
}
