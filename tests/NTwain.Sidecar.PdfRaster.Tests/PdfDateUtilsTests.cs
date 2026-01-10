using NTwain.Sidecar.PdfRaster.Utilities;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Tests for <see cref="PdfDateUtils"/> class.
/// </summary>
public class PdfDateUtilsTests
{
    [Fact]
    public void ToPdfDateString_WithLocalTime_ProducesValidFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        
        // Act
        var result = PdfDateUtils.ToPdfDateString(dt);
        
        // Assert
        Assert.StartsWith("D:20240615143045", result);
        // Should have timezone offset
        Assert.True(result.Length > 16, "Should include timezone offset");
    }

    [Fact]
    public void ToPdfDateString_WithUtcOffset_ProducesCorrectFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = TimeSpan.FromHours(5);
        
        // Act
        var result = PdfDateUtils.ToPdfDateString(dt, offset);
        
        // Assert
        Assert.Equal("D:20240615143045+05'00'", result);
    }

    [Fact]
    public void ToPdfDateString_WithNegativeOffset_ProducesCorrectFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = TimeSpan.FromHours(-8);
        
        // Act
        var result = PdfDateUtils.ToPdfDateString(dt, offset);
        
        // Assert
        Assert.Equal("D:20240615143045-08'00'", result);
    }

    [Fact]
    public void ToPdfDateString_WithZeroOffset_ProducesUtcFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = TimeSpan.Zero;
        
        // Act
        var result = PdfDateUtils.ToPdfDateString(dt, offset);
        
        // Assert
        Assert.Equal("D:20240615143045Z", result);
    }

    [Fact]
    public void ToPdfDateString_WithMinuteOffset_ProducesCorrectFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = new TimeSpan(5, 30, 0);
        
        // Act
        var result = PdfDateUtils.ToPdfDateString(dt, offset);
        
        // Assert
        Assert.Equal("D:20240615143045+05'30'", result);
    }

    [Fact]
    public void ParsePdfDateString_WithFullFormat_ReturnsCorrectDate()
    {
        // Arrange
        var pdfDate = "D:20240615143045Z";
        
        // Act
        var result = PdfDateUtils.ParsePdfDateString(pdfDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
        Assert.Equal(6, result.Value.Month);
        Assert.Equal(15, result.Value.Day);
        Assert.Equal(14, result.Value.Hour);
        Assert.Equal(30, result.Value.Minute);
        Assert.Equal(45, result.Value.Second);
    }

    [Fact]
    public void ParsePdfDateString_WithoutPrefix_ReturnsCorrectDate()
    {
        // Arrange
        var pdfDate = "20240615143045Z";
        
        // Act
        var result = PdfDateUtils.ParsePdfDateString(pdfDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
        Assert.Equal(6, result.Value.Month);
        Assert.Equal(15, result.Value.Day);
        Assert.Equal(14, result.Value.Hour);
        Assert.Equal(30, result.Value.Minute);
        Assert.Equal(45, result.Value.Second);
    }

    [Fact]
    public void ParsePdfDateString_YearOnly_ReturnsDate()
    {
        // Arrange
        var pdfDate = "D:2024";
        
        // Act
        var result = PdfDateUtils.ParsePdfDateString(pdfDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
        Assert.Equal(1, result.Value.Month);
        Assert.Equal(1, result.Value.Day);
    }

    [Fact]
    public void ParsePdfDateString_WithPositiveOffset_ReturnsDate()
    {
        // Arrange
        var pdfDate = "D:20240615143045+05'30'";
        
        // Act
        var result = PdfDateUtils.ParsePdfDateString(pdfDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
    }

    [Fact]
    public void ParsePdfDateString_WithNegativeOffset_ReturnsDate()
    {
        // Arrange
        var pdfDate = "D:20240615143045-08'00'";
        
        // Act
        var result = PdfDateUtils.ParsePdfDateString(pdfDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
    }

    [Fact]
    public void ParsePdfDateString_WithNull_ReturnsNull()
    {
        var result = PdfDateUtils.ParsePdfDateString(null!);
        Assert.Null(result);
    }

    [Fact]
    public void ParsePdfDateString_WithEmptyString_ReturnsNull()
    {
        var result = PdfDateUtils.ParsePdfDateString("");
        Assert.Null(result);
    }

    [Fact]
    public void ParsePdfDateString_WithTooShortString_ReturnsNull()
    {
        var result = PdfDateUtils.ParsePdfDateString("D:202");
        Assert.Null(result);
    }

    [Fact]
    public void ParsePdfDateString_WithInvalidFormat_ReturnsNull()
    {
        var result = PdfDateUtils.ParsePdfDateString("invalid");
        Assert.Null(result);
    }

    [Fact]
    public void CurrentPdfDateString_ReturnsValidFormat()
    {
        // Act
        var result = PdfDateUtils.CurrentPdfDateString();
        
        // Assert
        Assert.StartsWith("D:", result);
        Assert.True(result.Length >= 16, "Should have at least date and time");
    }

    [Fact]
    public void ToXmpDateString_ProducesIso8601Format()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        
        // Act
        var result = PdfDateUtils.ToXmpDateString(dt);
        
        // Assert
        Assert.StartsWith("2024-06-15T14:30:45", result);
    }

    [Fact]
    public void ToPdfDateString_RoundTrips_WithParsePdfDateString()
    {
        // Arrange
        var original = new DateTime(2024, 6, 15, 14, 30, 45);
        
        // Act
        var pdfString = PdfDateUtils.ToPdfDateString(original, TimeSpan.Zero);
        var parsed = PdfDateUtils.ParsePdfDateString(pdfString);
        
        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(original.Year, parsed.Value.Year);
        Assert.Equal(original.Month, parsed.Value.Month);
        Assert.Equal(original.Day, parsed.Value.Day);
        Assert.Equal(original.Hour, parsed.Value.Hour);
        Assert.Equal(original.Minute, parsed.Value.Minute);
        Assert.Equal(original.Second, parsed.Value.Second);
    }
}

/// <summary>
/// Tests for <see cref="PdfDateTime"/> class.
/// </summary>
public class PdfDateTimeTests
{
    [Fact]
    public void Constructor_FromDateTime_SetsProperties()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        
        // Act
        var pdfDt = new PdfDateTime(dt);
        
        // Assert
        Assert.Equal(2024, pdfDt.Year);
        Assert.Equal(6, pdfDt.Month);
        Assert.Equal(15, pdfDt.Day);
        Assert.Equal(14, pdfDt.Hour);
        Assert.Equal(30, pdfDt.Minute);
        Assert.Equal(45, pdfDt.Second);
        Assert.NotEqual(TimeSpan.Zero, pdfDt.Offset); // Should have local offset
    }

    [Fact]
    public void Constructor_FromDateTimeWithOffset_SetsProperties()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = TimeSpan.FromHours(5);
        
        // Act
        var pdfDt = new PdfDateTime(dt, offset);
        
        // Assert
        Assert.Equal(2024, pdfDt.Year);
        Assert.Equal(offset, pdfDt.Offset);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        // Arrange
        var offset = new TimeSpan(5, 30, 0);
        
        // Act
        var pdfDt = new PdfDateTime(2024, 6, 15, 14, 30, 45, offset);
        
        // Assert
        Assert.Equal(2024, pdfDt.Year);
        Assert.Equal(6, pdfDt.Month);
        Assert.Equal(15, pdfDt.Day);
        Assert.Equal(14, pdfDt.Hour);
        Assert.Equal(30, pdfDt.Minute);
        Assert.Equal(45, pdfDt.Second);
        Assert.Equal(offset, pdfDt.Offset);
    }

    [Fact]
    public void Now_ReturnsCurrentDateTime()
    {
        // Act
        var pdfDt = PdfDateTime.Now;
        var now = DateTime.Now;
        
        // Assert
        Assert.Equal(now.Year, pdfDt.Year);
        Assert.Equal(now.Month, pdfDt.Month);
        Assert.Equal(now.Day, pdfDt.Day);
    }

    [Fact]
    public void ToDateTime_ReturnsCorrectDateTime()
    {
        // Arrange
        var pdfDt = new PdfDateTime(2024, 6, 15, 14, 30, 45);
        
        // Act
        var result = pdfDt.ToDateTime();
        
        // Assert
        Assert.Equal(new DateTime(2024, 6, 15, 14, 30, 45), result);
    }

    [Fact]
    public void ToPdfString_WithPositiveOffset_ReturnsCorrectFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = new TimeSpan(5, 30, 0);
        var pdfDt = new PdfDateTime(dt, offset);
        
        // Act
        var result = pdfDt.ToPdfString();
        
        // Assert
        Assert.Equal("D:20240615143045+05'30'", result);
    }

    [Fact]
    public void ToPdfString_WithNegativeOffset_ReturnsCorrectFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var offset = TimeSpan.FromHours(-8);
        var pdfDt = new PdfDateTime(dt, offset);
        
        // Act
        var result = pdfDt.ToPdfString();
        
        // Assert
        Assert.Equal("D:20240615143045-08'00'", result);
    }

    [Fact]
    public void ToPdfString_WithZeroOffset_ReturnsUtcFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var pdfDt = new PdfDateTime(dt, TimeSpan.Zero);
        
        // Act
        var result = pdfDt.ToPdfString();
        
        // Assert
        Assert.Equal("D:20240615143045Z", result);
    }

    [Fact]
    public void ToString_ReturnsPdfString()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var pdfDt = new PdfDateTime(dt, TimeSpan.Zero);
        
        // Act
        var result = pdfDt.ToString();
        
        // Assert
        Assert.Equal(pdfDt.ToPdfString(), result);
    }

    [Fact]
    public void Struct_IsValueType()
    {
        // Assert
        Assert.True(typeof(PdfDateTime).IsValueType, "PdfDateTime should be a struct");
    }

    [Fact]
    public void Struct_CanBeCompared()
    {
        // Arrange
        var dt1 = new PdfDateTime(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        var dt2 = new PdfDateTime(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        
        // Assert - structs with same values should be equal
        Assert.Equal(dt1.Year, dt2.Year);
        Assert.Equal(dt1.Month, dt2.Month);
        Assert.Equal(dt1.ToPdfString(), dt2.ToPdfString());
    }
}
