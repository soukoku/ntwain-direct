//using NTwain.Sidecar.PdfRaster;
//using NTwain.Sidecar.PdfRaster.Reader;
//using NTwain.Sidecar.PdfRaster.Writer;
//using Xunit;

//namespace NTwain.Sidecar.PdfRaster.Tests;

///// <summary>
///// Tests for <see cref="PdfRasterReader"/>.
///// </summary>
//public class PdfRasterReaderTests
//{
//    [Fact]
//    public void PageCount_BeforeOpen_ThrowsException()
//    {
//        // Arrange
//        using var reader = new PdfRasterReader();

//        // Act & Assert
//        Assert.Throws<PdfApiException>(() => _ = reader.PageCount);
//    }

//    [Fact]
//    public void Close_ReleasesResources()
//    {
//        // Arrange - Create a valid PDF/raster using writer
//        using var pdfStream = CreateValidPdfRasterStream();
        
//        using var reader = new PdfRasterReader();
//        var opened = reader.Open(pdfStream, leaveOpen: true);
//        Assert.True(opened, "Should open valid PDF/raster");
//        Assert.True(reader.IsOpen);

//        // Act
//        reader.Close();

//        // Assert
//        Assert.False(reader.IsOpen);
//    }

//    [Fact]
//    public void Open_WithValidPdfRaster_Succeeds()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();

//        // Act
//        using var reader = new PdfRasterReader();
//        var result = reader.Open(pdfStream, leaveOpen: true);

//        // Assert
//        Assert.True(result, "Open should succeed for valid PDF/raster");
//        Assert.True(reader.IsOpen);
//    }

//    [Fact]
//    public void PageCount_WithValidPdfRaster_ReturnsCorrectCount()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var count = reader.PageCount;

//        // Assert
//        Assert.Equal(1, count);
//    }

//    [Fact]
//    public void GetPageInfo_WithValidPdfRaster_ReturnsValidInfo()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream(width: 100, height: 80);
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var pageInfo = reader.GetPageInfo(0);

//        // Assert
//        Assert.NotNull(pageInfo);
//        Assert.Equal(100, pageInfo.Width);
//        Assert.Equal(80, pageInfo.Height);
//        Assert.True(pageInfo.StripCount >= 1, "Should have at least one strip");
//    }

//    [Fact]
//    public void ReadPagePixels_WithValidPdfRaster_ReturnsData()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var pixels = reader.ReadPagePixels(0);

//        // Assert
//        Assert.NotNull(pixels);
//        Assert.True(pixels.Length > 0, "Pixel data should not be empty");
//    }

//    [Fact]
//    public void GetPageInfo_ReturnsPositiveDpi()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var pageInfo = reader.GetPageInfo(0);

//        // Assert
//        Assert.True(pageInfo.XDpi > 0, "XDpi should be positive");
//        Assert.True(pageInfo.YDpi > 0, "YDpi should be positive");
//    }

//    [Fact]
//    public void GetPageFormat_WithGray8_ReturnsGray8Format()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream(format: RasterPixelFormat.Gray8);
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var format = reader.GetPageFormat(0);

//        // Assert
//        Assert.Equal(RasterPixelFormat.Gray8, format);
//    }

//    [Fact]
//    public void GetPageFormat_WithRgb24_ReturnsRgb24Format()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream(format: RasterPixelFormat.Rgb24);
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act
//        var format = reader.GetPageFormat(0);

//        // Assert
//        Assert.Equal(RasterPixelFormat.Rgb24, format);
//    }

//    [Fact]
//    public void GetPageInfo_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act & Assert
//        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetPageInfo(-1));
//    }

//    [Fact]
//    public void GetPageInfo_WithIndexOutOfRange_ThrowsArgumentOutOfRangeException()
//    {
//        // Arrange
//        using var pdfStream = CreateValidPdfRasterStream();
//        using var reader = new PdfRasterReader();
//        reader.Open(pdfStream, leaveOpen: true);

//        // Act & Assert
//        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetPageInfo(999));
//    }

//    [Fact]
//    public void Open_WithNonSeekableStream_ThrowsArgumentException()
//    {
//        // Arrange
//        using var reader = new PdfRasterReader();
//        using var nonSeekableStream = new NonSeekableStream();

//        // Act & Assert
//        Assert.Throws<ArgumentException>(() => reader.Open(nonSeekableStream));
//    }

//    // Helper method to create a valid PDF/raster stream for testing
//    private static MemoryStream CreateValidPdfRasterStream(
//        int width = 50, 
//        int height = 50, 
//        RasterPixelFormat format = RasterPixelFormat.Gray8)
//    {
//        var pixelData = CreateTestImage(width, height, format);
        
//        var pdfStream = new MemoryStream();
//        using (var writer = new PdfRasterWriter())
//        {
//            writer.Begin(pdfStream, leaveOpen: true);
//            writer.SetPixelFormat(format);
//            writer.SetCompression(RasterCompression.Uncompressed);
//            writer.SetResolution(200, 200);
            
//            writer.StartPage(width);
//            writer.WriteStrip(height, pixelData);
//            writer.EndPage();
//            writer.End();
//        }
        
//        pdfStream.Position = 0;
//        return pdfStream;
//    }

//    private static byte[] CreateTestImage(int width, int height, RasterPixelFormat format)
//    {
//        var bytesPerPixel = format switch
//        {
//            RasterPixelFormat.Bitonal => 0,
//            RasterPixelFormat.Gray8 => 1,
//            RasterPixelFormat.Gray16 => 2,
//            RasterPixelFormat.Rgb24 => 3,
//            RasterPixelFormat.Rgb48 => 6,
//            _ => 1
//        };

//        if (format == RasterPixelFormat.Bitonal)
//        {
//            var bytesPerRow = (width + 7) / 8;
//            var data = new byte[bytesPerRow * height];
//            for (var y = 0; y < height; y++)
//            {
//                for (var x = 0; x < width; x++)
//                {
//                    if ((x + y) % 2 == 0)
//                    {
//                        var byteIndex = y * bytesPerRow + x / 8;
//                        var bitIndex = 7 - (x % 8);
//                        data[byteIndex] |= (byte)(1 << bitIndex);
//                    }
//                }
//            }
//            return data;
//        }

//        var pixelData = new byte[width * height * bytesPerPixel];
//        for (var i = 0; i < pixelData.Length; i++)
//        {
//            pixelData[i] = (byte)(i % 256);
//        }
//        return pixelData;
//    }

//    private class NonSeekableStream : Stream
//    {
//        public override bool CanRead => true;
//        public override bool CanSeek => false;
//        public override bool CanWrite => false;
//        public override long Length => 0;
//        public override long Position { get => 0; set { } }
//        public override void Flush() { }
//        public override int Read(byte[] buffer, int offset, int count) => 0;
//        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
//        public override void SetLength(long value) { }
//        public override void Write(byte[] buffer, int offset, int count) { }
//    }
//}
