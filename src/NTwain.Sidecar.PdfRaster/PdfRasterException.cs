// Base exception for PDF/raster operations

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Base exception for all PDF/raster errors
/// </summary>
public class PdfRasterException : Exception
{
    public ErrorLevel Level { get; }
    public int ErrorCode { get; }
    public long Offset { get; }
    
    public PdfRasterException(string message, ErrorLevel level = ErrorLevel.Other, int errorCode = 0, long offset = 0)
        : base(message)
    {
        Level = level;
        ErrorCode = errorCode;
        Offset = offset;
    }
    
    public PdfRasterException(string message, Exception innerException, ErrorLevel level = ErrorLevel.Other, int errorCode = 0, long offset = 0)
        : base(message, innerException)
    {
        Level = level;
        ErrorCode = errorCode;
        Offset = offset;
    }
}
