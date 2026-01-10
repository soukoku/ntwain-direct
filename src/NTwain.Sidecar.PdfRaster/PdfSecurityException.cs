// PDF security exception

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Exception thrown when encryption/decryption fails
/// </summary>
public class PdfSecurityException : PdfRasterException
{
    public PdfSecurityException(string message) 
        : base(message, ErrorLevel.Other) { }
}
