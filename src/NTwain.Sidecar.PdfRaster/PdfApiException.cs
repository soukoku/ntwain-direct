// PDF API exception

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Exception thrown when API is misused
/// </summary>
public class PdfApiException : PdfRasterException
{
    public PdfApiException(string message) 
        : base(message, ErrorLevel.Api) { }
}
