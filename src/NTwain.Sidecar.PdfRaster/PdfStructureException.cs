// PDF structure exception

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Exception thrown when PDF structure is not valid
/// </summary>
public class PdfStructureException : PdfRasterException
{
    public PdfStructureException(string message, long offset = 0) 
        : base(message, ErrorLevel.Compliance, 0, offset) { }
}
