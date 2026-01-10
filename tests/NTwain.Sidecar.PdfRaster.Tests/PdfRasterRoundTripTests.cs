using NTwain.Sidecar.PdfRaster;
using NTwain.Sidecar.PdfRaster.Reader;
using NTwain.Sidecar.PdfRaster.Writer;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Round-trip tests that write and read PDF/raster files.
/// </summary>
public class PdfRasterRoundTripTests
{
    [Theory]
    [InlineData(RasterPixelFormat.Gray8, RasterCompression.Uncompressed)]
    [InlineData(RasterPixelFormat.Gray8, RasterCompression.Flate)]
    [InlineData(RasterPixelFormat.Rgb24, RasterCompression.Uncompressed)]
    [InlineData(RasterPixelFormat.Rgb24, RasterCompression.Flate)]
    public void RoundTrip_PreservesDimensions(RasterPixelFormat format, RasterCompression compression)
    {
        // Arrange
        var width = 100;
        var height = 80;
        var pixelData = CreateTestImage(width, height, format);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(format);
            writer.SetCompression(compression);
            writer.SetResolution(200, 200);
            
            writer.StartPage(width);
            writer.WriteStrip(height, pixelData);
            writer.EndPage();
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened, "Reader should open the writer's output");
        
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.Equal(width, pageInfo.Width);
        Assert.Equal(height, pageInfo.Height);
    }

    [Fact]
    public void RoundTrip_MultiplePages_PreservesPageCount()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var pageCount = 5;
        var pixelData = CreateTestImage(width, height, RasterPixelFormat.Gray8);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(RasterPixelFormat.Gray8);
            writer.SetCompression(RasterCompression.Uncompressed);
            
            for (var i = 0; i < pageCount; i++)
            {
                writer.StartPage(width);
                writer.WriteStrip(height, pixelData);
                writer.EndPage();
            }
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened, "Reader should open the writer's output");

        // Assert
        Assert.Equal(pageCount, reader.PageCount);
    }

    [Fact]
    public void RoundTrip_MultipleStrips_PreservesData()
    {
        // Arrange
        var width = 100;
        var stripHeight = 25;
        var numStrips = 4;
        var totalHeight = stripHeight * numStrips;

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(RasterPixelFormat.Gray8);
            writer.SetCompression(RasterCompression.Uncompressed);
            
            writer.StartPage(width);
            for (int i = 0; i < numStrips; i++)
            {
                var stripData = CreateTestImage(width, stripHeight, RasterPixelFormat.Gray8);
                writer.WriteStrip(stripHeight, stripData);
            }
            writer.EndPage();
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened, "Reader should open the writer's output");
        
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.Equal(width, pageInfo.Width);
        Assert.Equal(totalHeight, pageInfo.Height);
        Assert.Equal(numStrips, pageInfo.StripCount);
    }

    [Fact]
    public void RoundTrip_PreservesResolution()
    {
        // Arrange
        var width = 100;
        var height = 100;
        var xdpi = 300.0;
        var ydpi = 300.0;
        var pixelData = CreateTestImage(width, height, RasterPixelFormat.Gray8);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(RasterPixelFormat.Gray8);
            writer.SetCompression(RasterCompression.Uncompressed);
            writer.SetResolution(xdpi, ydpi);
            
            writer.StartPage(width);
            writer.WriteStrip(height, pixelData);
            writer.EndPage();
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened, "Reader should open the writer's output");
        
        var pageInfo = reader.GetPageInfo(0);

        // Assert - DPI should be close (within 1%)
        Assert.True(Math.Abs(pageInfo.XDpi - xdpi) / xdpi < 0.01, 
            $"XDpi should be close to {xdpi}, was {pageInfo.XDpi}");
        Assert.True(Math.Abs(pageInfo.YDpi - ydpi) / ydpi < 0.01,
            $"YDpi should be close to {ydpi}, was {pageInfo.YDpi}");
    }

    [Fact]
    public void RoundTrip_PreservesPixelFormat_Gray8()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var format = RasterPixelFormat.Gray8;
        var pixelData = CreateTestImage(width, height, format);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(format);
            writer.SetCompression(RasterCompression.Uncompressed);
            
            writer.StartPage(width);
            writer.WriteStrip(height, pixelData);
            writer.EndPage();
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened);
        
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.Equal(format, pageInfo.Format);
    }

    [Fact]
    public void RoundTrip_PreservesPixelFormat_Rgb24()
    {
        // Arrange
        var width = 50;
        var height = 50;
        var format = RasterPixelFormat.Rgb24;
        var pixelData = CreateTestImage(width, height, format);

        using var pdfStream = new MemoryStream();

        // Act - Write
        using (var writer = new PdfRasterWriter())
        {
            writer.Begin(pdfStream, leaveOpen: true);
            writer.SetPixelFormat(format);
            writer.SetCompression(RasterCompression.Uncompressed);
            
            writer.StartPage(width);
            writer.WriteStrip(height, pixelData);
            writer.EndPage();
            writer.End();
        }

        // Act - Read
        pdfStream.Position = 0;
        using var reader = new PdfRasterReader();
        var opened = reader.Open(pdfStream, leaveOpen: true);
        Assert.True(opened);
        
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.Equal(format, pageInfo.Format);
    }

    private static byte[] CreateTestImage(int width, int height, RasterPixelFormat format)
    {
        var bytesPerPixel = format switch
        {
            RasterPixelFormat.Bitonal => 0,
            RasterPixelFormat.Gray8 => 1,
            RasterPixelFormat.Gray16 => 2,
            RasterPixelFormat.Rgb24 => 3,
            RasterPixelFormat.Rgb48 => 6,
            _ => 1
        };

        if (format == RasterPixelFormat.Bitonal)
        {
            var bytesPerRow = (width + 7) / 8;
            var data = new byte[bytesPerRow * height];
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
