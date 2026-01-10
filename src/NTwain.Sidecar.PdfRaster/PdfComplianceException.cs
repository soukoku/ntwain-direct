// PDF compliance exception

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Exception thrown when a PDF/raster compliance issue is detected
/// </summary>
public class PdfComplianceException : PdfRasterException
{
    public ReadErrorCode ReadError { get; }
    
    public PdfComplianceException(string message, ReadErrorCode errorCode, long offset = 0) 
        : base(message, ErrorLevel.Compliance, (int)errorCode, offset)
    {
        ReadError = errorCode;
    }
}
