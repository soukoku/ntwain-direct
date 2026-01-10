using Xunit;

namespace NTwain.Sidecar.PdfR.Tests;

/// <summary>
/// Tests for <see cref="PdfRasterReader"/>.
/// </summary>
public class PdfRasterReaderTests
{
    [Fact]
    public void Constructor_WithNullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PdfRasterReader(null!));
    }

    [Fact]
    public void Constructor_WithNonReadableStream_ThrowsArgumentException()
    {
        using var stream = new MemoryStream([], writable: true);
        stream.Close();

        Assert.Throws<ArgumentException>(() => new PdfRasterReader(stream));
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void ReadPage_WithSamplePdf_ReturnsValidImage(string pdfPath)
    {
        // Arrange
        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.NotNull(image);
        Assert.True(image.Width > 0, "Width should be positive");
        Assert.True(image.Height > 0, "Height should be positive");
        Assert.NotNull(image.PixelData);
        Assert.True(image.PixelData.Length > 0, "PixelData should not be empty");
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void PageCount_WithSamplePdf_ReturnsAtLeastOne(string pdfPath)
    {
        // Arrange
        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var count = reader.PageCount;

        // Assert
        Assert.True(count >= 1, "PDF should have at least one page");
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void ReadAllPages_WithSamplePdf_ReturnsAllPages(string pdfPath)
    {
        // Arrange
        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);
        var expectedCount = reader.PageCount;

        // Act
        var pages = reader.ReadAllPages().ToList();

        // Assert
        Assert.Equal(expectedCount, pages.Count);
        Assert.All(pages, page =>
        {
            Assert.NotNull(page);
            Assert.True(page.Width > 0);
            Assert.True(page.Height > 0);
        });
    }

    [Fact]
    public void ReadPage_WithBw1Ccitt_ReturnsBlackWhiteFormat()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Bw1Ccitt;
        if (!File.Exists(pdfPath)) return; // Skip if file not available

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.Equal(PdfRasterPixelFormat.BlackWhite1, image.PixelFormat);
    }

    [Fact]
    public void ReadPage_WithBw1Uncompressed_ReturnsBlackWhiteFormat()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Bw1Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.Equal(PdfRasterPixelFormat.BlackWhite1, image.PixelFormat);
    }

    [Fact]
    public void ReadPage_WithGray8_ReturnsGray8Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Gray8Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.Equal(PdfRasterPixelFormat.Gray8, image.PixelFormat);
    }

    [Fact]
    public void ReadPage_WithGray16_ReturnsGray16Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Gray16Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.Equal(PdfRasterPixelFormat.Gray16, image.PixelFormat);
    }

    [Fact]
    public void ReadPage_WithRgb24_ReturnsRgb24Format()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.Rgb24Uncompressed;
        if (!File.Exists(pdfPath)) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.Equal(PdfRasterPixelFormat.Rgb24, image.PixelFormat);
    }

    [Fact]
    public void ReadPage_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadPage(-1));
    }

    [Fact]
    public void ReadPage_WithIndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pdfPath = SamplePdfFiles.All.FirstOrDefault(File.Exists);
        if (pdfPath == null) return;

        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadPage(999));
    }

    [Theory]
    [MemberData(nameof(GetAllSamplePdfs))]
    public void ReadPage_ReturnsPositiveDpi(string pdfPath)
    {
        // Arrange
        using var stream = File.OpenRead(pdfPath);
        using var reader = new PdfRasterReader(stream);

        // Act
        var image = reader.ReadPage(0);

        // Assert
        Assert.True(image.HorizontalDpi > 0, "HorizontalDpi should be positive");
        Assert.True(image.VerticalDpi > 0, "VerticalDpi should be positive");
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
            throw new Exception("What happened?");
        }
        return data;
    }
}
