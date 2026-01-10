// Common enumerations for PDF/raster
// Ported from pdfras_reader.h and PdfRaster.h

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Pixel formats for raster images
/// </summary>
public enum RasterPixelFormat
{
    /// <summary>Null/invalid format</summary>
    Null = 0,
    
    /// <summary>1-bit per pixel, 0=black (DeviceGray or CalGray)</summary>
    Bitonal = 1,
    
    /// <summary>8-bit per pixel, 0=black (CalGray)</summary>
    Gray8 = 2,
    
    /// <summary>16-bit per pixel, 0=black (CalGray)</summary>
    Gray16 = 3,
    
    /// <summary>24-bit per pixel, sRGB (ICCBased or CalRGB)</summary>
    Rgb24 = 4,
    
    /// <summary>48-bit per pixel, sRGB (ICCBased or CalRGB)</summary>
    Rgb48 = 5
}

/// <summary>
/// Compression modes for raster images
/// </summary>
public enum RasterCompression
{
    /// <summary>Null/invalid compression</summary>
    Null = 0,
    
    /// <summary>Uncompressed (/Filter null)</summary>
    Uncompressed = 1,
    
    /// <summary>JPEG baseline (DCTDecode)</summary>
    Jpeg = 2,
    
    /// <summary>CCITT Group 4 (CCITTFaxDecode)</summary>
    CcittGroup4 = 3,
    
    /// <summary>Flate/zlib compression (FlateDecode)</summary>
    Flate = 4
}

/// <summary>
/// Security types for PDF documents
/// </summary>
public enum SecurityType
{
    /// <summary>Unknown or error occurred</summary>
    Unknown = 0,
    
    /// <summary>Document is unencrypted</summary>
    Unencrypted = 1,
    
    /// <summary>Document is encrypted by password security</summary>
    StandardSecurity = 2,
    
    /// <summary>Document is encrypted by certificate security</summary>
    PublicKeySecurity = 3
}

/// <summary>
/// Encryption algorithms
/// </summary>
public enum EncryptionAlgorithm
{
    /// <summary>Undefined algorithm</summary>
    Undefined = 0,
    
    /// <summary>RC4 with 40-bit key</summary>
    Rc4_40 = 1,
    
    /// <summary>RC4 with 128-bit key</summary>
    Rc4_128 = 2,
    
    /// <summary>AES with 128-bit key</summary>
    Aes128 = 3,
    
    /// <summary>AES with 256-bit key</summary>
    Aes256 = 4
}

/// <summary>
/// Document permissions flags
/// </summary>
[Flags]
public enum DocumentPermissions : uint
{
    /// <summary>Unknown permissions</summary>
    Unknown = 0,
    
    /// <summary>Allow printing</summary>
    Print = 1 << 2,
    
    /// <summary>Allow modifying the document</summary>
    Modify = 1 << 3,
    
    /// <summary>Allow copying text and graphics</summary>
    Copy = 1 << 4,
    
    /// <summary>Allow adding annotations</summary>
    AddAnnotations = 1 << 5,
    
    /// <summary>Allow filling in form fields</summary>
    FillForms = 1 << 8,
    
    /// <summary>Allow extracting content for accessibility</summary>
    ExtractForAccessibility = 1 << 9,
    
    /// <summary>Allow assembling the document</summary>
    Assemble = 1 << 10,
    
    /// <summary>Allow high-resolution printing</summary>
    PrintHighRes = 1 << 11
}

/// <summary>
/// Document access levels
/// </summary>
public enum DocumentAccess
{
    /// <summary>No access</summary>
    None = 0,
    
    /// <summary>User-level access</summary>
    User = 1,
    
    /// <summary>Owner-level access</summary>
    Owner = 2
}

/// <summary>
/// Colorspace styles
/// </summary>
public enum ColorspaceStyle
{
    /// <summary>CalGray colorspace</summary>
    CalGray,
    
    /// <summary>DeviceGray colorspace</summary>
    DeviceGray,
    
    /// <summary>CalRGB colorspace</summary>
    CalRgb,
    
    /// <summary>DeviceRGB colorspace</summary>
    DeviceRgb,
    
    /// <summary>ICCBased colorspace</summary>
    IccBased
}
