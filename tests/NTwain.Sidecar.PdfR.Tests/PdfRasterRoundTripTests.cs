using Xunit;

namespace NTwain.Sidecar.PdfR.Tests;

/// <summary>
/// Round-trip tests that write and read PDF/raster files.
/// </summary>
public class PdfRasterRoundTripTests
{
    [Theory]
    [InlineData(PdfRasterPixelFormat.Gray8, PdfRasterCompression.None)]
    [InlineData(PdfRasterPixelFormat.Gray8, PdfRasterCompression.Jpeg)]
    [InlineData(PdfRasterPixelFormat.Rgb24, PdfRasterCompression.None)]
    [InlineData(PdfRasterPixelFormat.Rgb24, PdfRasterCompression.Jpeg)]
    public void RoundTrip_PreservesDimensions(PdfRasterPixelFormat format, PdfRasterCompression compression)
    {
        // Arrange
        var width = 100;
        var height = 80;
        var pixelData = CreateTestImage(width, height, format);

        var originalImage = new PdfRasterImage(
            pixelData,
            Width: width,
            Height: height,
            PixelFormat: format,
            Compression: compression,
            HorizontalDpi: 200,
            VerticalDpi: 200);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter(pdfStream, leaveOpen: true))
        {
            writer.AddPage(originalImage);
            writer.Finish();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader(pdfStream);
        var readImage = reader.ReadPage(0);

        // Assert
        Assert.Equal(width, readImage.Width);
        Assert.Equal(height, readImage.Height);
    }

    [Fact]
    public void RoundTrip_MultiplePages_PreservesPageCount()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pageCount = 5;
        var pixelData = CreateTestImage(width, height, PdfRasterPixelFormat.Gray8);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter(pdfStream, leaveOpen: true))
        {
            for (var i = 0; i < pageCount; i++)
            {
                var image = new PdfRasterImage(
                    pixelData,
                    Width: width,
                    Height: height,
                    PixelFormat: PdfRasterPixelFormat.Gray8,
                    Compression: PdfRasterCompression.None);
                writer.AddPage(image);
            }
            writer.Finish();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader(pdfStream);

        // Assert
        Assert.Equal(pageCount, reader.PageCount);
    }

    [Theory]
    [MemberData(nameof(GetSamplePdfsForRoundTrip))]
    public void RoundTrip_ReadAndRewrite_ProducesValidPdf(string sourcePdfPath)
    {
        // Arrange - Read original
        PdfRasterImage originalImage;
        using (var sourceStream = File.OpenRead(sourcePdfPath))
        using (var reader = new PdfRasterReader(sourceStream))
        {
            originalImage = reader.ReadPage(0);
        }

        using var outputStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter(outputStream, leaveOpen: true))
        {
            // Write with no compression to simplify comparison
            var imageToWrite = new PdfRasterImage(
                originalImage.PixelData,
                originalImage.Width,
                originalImage.Height,
                originalImage.PixelFormat,
                PdfRasterCompression.None,
                originalImage.HorizontalDpi,
                originalImage.VerticalDpi);
            writer.AddPage(imageToWrite);
            writer.Finish();
        }

        // Act - Read back
        outputStream.Position = 0;
        using var finalReader = new PdfRasterReader(outputStream);
        var finalImage = finalReader.ReadPage(0);

        // Assert
        Assert.Equal(originalImage.Width, finalImage.Width);
        Assert.Equal(originalImage.Height, finalImage.Height);
        Assert.Equal(originalImage.PixelFormat, finalImage.PixelFormat);
    }

    public static TheoryData<string> GetSamplePdfsForRoundTrip()
    {
        var data = new TheoryData<string>();
        foreach (var path in SamplePdfFiles.All)
        {
            if (File.Exists(path))
            {
                data.Add(path);
            }
        }
        return data;
    }

    private static byte[] CreateTestImage(int width, int height, PdfRasterPixelFormat format)
    {
        var bytesPerPixel = format switch
        {
            PdfRasterPixelFormat.BlackWhite1 => 0, // Special handling
            PdfRasterPixelFormat.Gray8 => 1,
            PdfRasterPixelFormat.Gray16 => 2,
            PdfRasterPixelFormat.Rgb24 => 3,
            PdfRasterPixelFormat.Rgb48 => 6,
            _ => 1
        };

        if (format == PdfRasterPixelFormat.BlackWhite1)
        {
            var bytesPerRow = (width + 7) / 8;
            var data = new byte[bytesPerRow * height];
            // Create a checkerboard pattern
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
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

        var pixelData = new byte[width * height * bytesPerPixel];
        for (var i = 0; i < pixelData.Length; i++)
        {
            pixelData[i] = (byte)(i % 256);
        }
        return pixelData;
    }
}
