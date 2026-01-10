// PDF date utilities
// Ported from PdfDate.c

namespace NTwain.Sidecar.PdfRaster.Utilities;

/// <summary>
/// Utilities for working with PDF date strings
/// </summary>
public static class PdfDateUtils
{
    /// <summary>
    /// Format a DateTime as a PDF date string: D:YYYYMMDDHHmmSSOHH'mm'
    /// </summary>
    public static string ToPdfDateString(DateTime dateTime)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
        return ToPdfDateString(dateTime, offset);
    }
    
    /// <summary>
    /// Format a DateTime as a PDF date string with specific timezone offset
    /// </summary>
    public static string ToPdfDateString(DateTime dateTime, TimeSpan offset)
    {
        char sign;
        TimeSpan absOffset;
        
        if (offset < TimeSpan.Zero)
        {
            sign = '-';
            absOffset = offset.Negate();
        }
        else if (offset > TimeSpan.Zero)
        {
            sign = '+';
            absOffset = offset;
        }
        else
        {
            // UTC
            return $"D:{dateTime:yyyyMMddHHmmss}Z";
        }
        
        return $"D:{dateTime:yyyyMMddHHmmss}{sign}{absOffset.Hours:D2}'{absOffset.Minutes:D2}'";
    }
    
    /// <summary>
    /// Format current local time as a PDF date string
    /// </summary>
    public static string CurrentPdfDateString()
    {
        return ToPdfDateString(DateTime.Now);
    }
    
    /// <summary>
    /// Parse a PDF date string into a DateTime
    /// </summary>
    public static DateTime? ParsePdfDateString(string pdfDate)
    {
        if (string.IsNullOrEmpty(pdfDate))
            return null;
        
        // Remove D: prefix if present
        if (pdfDate.StartsWith("D:"))
            pdfDate = pdfDate.Substring(2);
        
        try
        {
            // Minimum: YYYY (4 chars)
            if (pdfDate.Length < 4)
                return null;
            
            int year = int.Parse(pdfDate.Substring(0, 4));
            int month = 1, day = 1, hour = 0, minute = 0, second = 0;
            TimeSpan offset = TimeSpan.Zero;
            
            if (pdfDate.Length >= 6)
                month = int.Parse(pdfDate.Substring(4, 2));
            if (pdfDate.Length >= 8)
                day = int.Parse(pdfDate.Substring(6, 2));
            if (pdfDate.Length >= 10)
                hour = int.Parse(pdfDate.Substring(8, 2));
            if (pdfDate.Length >= 12)
                minute = int.Parse(pdfDate.Substring(10, 2));
            if (pdfDate.Length >= 14)
                second = int.Parse(pdfDate.Substring(12, 2));
            
            // Parse timezone offset
            if (pdfDate.Length > 14)
            {
                char tz = pdfDate[14];
                if (tz == 'Z')
                {
                    offset = TimeSpan.Zero;
                }
                else if (tz == '+' || tz == '-')
                {
                    // Parse offset: +HH'mm' or -HH'mm'
                    int offsetHours = 0, offsetMins = 0;
                    if (pdfDate.Length >= 17)
                        offsetHours = int.Parse(pdfDate.Substring(15, 2));
                    
                    int quotePos = pdfDate.IndexOf('\'', 17);
                    if (quotePos > 17 && pdfDate.Length >= quotePos + 3)
                    {
                        offsetMins = int.Parse(pdfDate.Substring(quotePos + 1, 2));
                    }
                    
                    offset = new TimeSpan(offsetHours, offsetMins, 0);
                    if (tz == '-')
                        offset = offset.Negate();
                }
            }
            
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            
            // Convert to local time if offset is specified
            if (offset != TimeSpan.Zero)
            {
                var utc = dt - offset;
                return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
            }
            
            return dt;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Format a DateTime as an XMP date string (ISO 8601)
    /// </summary>
    public static string ToXmpDateString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}

/// <summary>
/// Represents a PDF date/time value
/// </summary>
public class PdfDateTime
{
    public int Year { get; }
    public int Month { get; }
    public int Day { get; }
    public int Hour { get; }
    public int Minute { get; }
    public int Second { get; }
    public int HourOffset { get; }
    public int MinuteOffset { get; }
    
    public PdfDateTime(DateTime dt)
    {
        Year = dt.Year;
        Month = dt.Month;
        Day = dt.Day;
        Hour = dt.Hour;
        Minute = dt.Minute;
        Second = dt.Second;
        
        var offset = TimeZoneInfo.Local.GetUtcOffset(dt);
        HourOffset = (int)offset.TotalHours;
        MinuteOffset = Math.Abs(offset.Minutes);
    }
    
    public PdfDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0,
        int hourOffset = 0, int minuteOffset = 0)
    {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
        HourOffset = hourOffset;
        MinuteOffset = minuteOffset;
    }
    
    /// <summary>
    /// Create a PdfDateTime for the current local time
    /// </summary>
    public static PdfDateTime Now => new PdfDateTime(DateTime.Now);
    
    /// <summary>
    /// Convert to DateTime
    /// </summary>
    public DateTime ToDateTime()
    {
        return new DateTime(Year, Month, Day, Hour, Minute, Second);
    }
    
    /// <summary>
    /// Convert to PDF date string
    /// </summary>
    public string ToPdfString()
    {
        char sign;
        int absHourOffset;
        
        if (HourOffset < 0)
        {
            sign = '-';
            absHourOffset = -HourOffset;
        }
        else if (HourOffset > 0)
        {
            sign = '+';
            absHourOffset = HourOffset;
        }
        else if (MinuteOffset == 0)
        {
            return $"D:{Year:D4}{Month:D2}{Day:D2}{Hour:D2}{Minute:D2}{Second:D2}Z";
        }
        else
        {
            sign = '+';
            absHourOffset = 0;
        }
        
        return $"D:{Year:D4}{Month:D2}{Day:D2}{Hour:D2}{Minute:D2}{Second:D2}{sign}{absHourOffset:D2}'{MinuteOffset:D2}'";
    }
    
    public override string ToString() => ToPdfString();
}
