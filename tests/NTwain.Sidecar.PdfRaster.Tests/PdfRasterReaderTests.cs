using NTwain.Sidecar.PdfRaster;
using NTwain.Sidecar.PdfRaster.Reader;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Tests for <see cref="PdfRasterReader"/>.
/// </summary>
public class PdfRasterReaderTests
{
    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void Open_WithSamplePdf_Succeeds(string pdfPath)
    {
        // Arrange & Act
        using var reader = new PdfRasterReader();
        var result = reader.Open(pdfPath);

        // Assert
        Assert.True(result, "Open should succeed for valid PDF/raster files");
        Assert.True(reader.IsOpen);
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void PageCount_WithSamplePdf_ReturnsAtLeastOne(string pdfPath)
    {
        // Arrange
        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var count = reader.PageCount;

        // Assert
        Assert.True(count >= 1, "PDF should have at least one page");
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void GetPageInfo_WithSamplePdf_ReturnsValidInfo(string pdfPath)
    {
        // Arrange
        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.NotNull(pageInfo);
        Assert.True(pageInfo.Width > 0, "Width should be positive");
        Assert.True(pageInfo.Height > 0, "Height should be positive");
        Assert.True(pageInfo.StripCount >= 1, "Should have at least one strip");
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void ReadPagePixels_WithSamplePdf_ReturnsData(string pdfPath)
    {
        // Arrange
        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var pixels = reader.ReadPagePixels(0);

        // Assert
        Assert.NotNull(pixels);
        Assert.True(pixels.Length > 0, "Pixel data should not be empty");
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void GetPageInfo_ReturnsPositiveDpi(string pdfPath)
    {
        // Arrange
        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var pageInfo = reader.GetPageInfo(0);

        // Assert
        Assert.True(pageInfo.XDpi > 0, "XDpi should be positive");
        Assert.True(pageInfo.YDpi > 0, "YDpi should be positive");
    }

    [Fact]
    public void Open_WithBw1Ccitt_ReturnsBitonalFormat()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Bw1Ccitt;
        if (!File.Exists(pdfPath)) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var format = reader.GetPageFormat(0);

        // Assert
        Assert.Equal(RasterPixelFormat.Bitonal, format);
    }

    [Fact]
    public void Open_WithBw1Uncompressed_ReturnsBitonalFormat()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Bw1Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var format = reader.GetPageFormat(0);

        // Assert
        Assert.Equal(RasterPixelFormat.Bitonal, format);
    }

    [Fact]
    public void Open_WithGray8_ReturnsGray8Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Gray8Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var format = reader.GetPageFormat(0);

        // Assert
        Assert.Equal(RasterPixelFormat.Gray8, format);
    }

    [Fact]
    public void Open_WithGray16_ReturnsGray16Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Gray16Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var format = reader.GetPageFormat(0);

        // Assert
        Assert.Equal(RasterPixelFormat.Gray16, format);
    }

    [Fact]
    public void Open_WithRgb24_ReturnsRgb24Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Rgb24Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act
        var format = reader.GetPageFormat(0);

        // Assert
        Assert.Equal(RasterPixelFormat.Rgb24, format);
    }

    [Fact]
    public void GetPageInfo_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetPageInfo(-1));
    }

    [Fact]
    public void GetPageInfo_WithIndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetPageInfo(999));
    }

    [Fact]
    public void PageCount_BeforeOpen_ThrowsException()
    {
        // Arrange
        using var reader = new PdfRasterReader();

        // Act & Assert
        Assert.Throws<PdfApiException>(() => _ = reader.PageCount);
    }

    [Fact]
    public void Recognize_WithValidPdfRaster_ReturnsTrue()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var stream = File.OpenRead(pdfPath);

        // Act
        var result = PdfRasterReader.Recognize(stream, out var major, out var minor);

        // Assert
        Assert.True(result);
        Assert.True(major >= 1);
        Assert.True(minor >= 0);
    }

    [Fact]
    public void Close_ReleasesResources()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var reader = new PdfRasterReader();
        reader.Open(pdfPath);
        Assert.True(reader.IsOpen);

        // Act
        reader.Close();

        // Assert
        Assert.False(reader.IsOpen);
    }

    public static TheoryData<string> GetAllSamplePdfs()
    {
        var data = new TheoryData<string>();
        foreach (var path in SamplePdfFiles.All)
        {
            if (File.Exists(path))
            {
                data.Add(path);
            }
        }
        if (data.Count == 0)
        {
            throw new InvalidOperationException("No sample PDF files found");
        }
        return data;
    }
}
