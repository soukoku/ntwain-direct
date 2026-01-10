// Digital signature information

namespace NTwain.Sidecar.PdfRaster.Security;

/// <summary>
/// Digital signature information
/// </summary>
public class DigitalSignatureInfo
{
    /// <summary>Name of the signer</summary>
    public string? Name { get; set; }
    
    /// <summary>Reason for signing</summary>
    public string? Reason { get; set; }
    
    /// <summary>Location where signed</summary>
    public string? Location { get; set; }
    
    /// <summary>Contact information of signer</summary>
    public string? ContactInfo { get; set; }
    
    /// <summary>Date/time when signed</summary>
    public DateTime? SigningTime { get; set; }
    
    /// <summary>Byte range that was signed [offset1, length1, offset2, length2]</summary>
    public long[]? ByteRange { get; set; }
    
    /// <summary>The signature data (PKCS#7)</summary>
    public byte[]? Contents { get; set; }
}
