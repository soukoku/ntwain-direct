// Library version and constants
// Ported from PdfRaster.h and pdfrasread.c

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// PDF/raster library constants and version information
/// </summary>
public static class PdfRasterConstants
{
    /// <summary>API level for reader</summary>
    public const int ReaderApiLevel = 1;
    
    /// <summary>API level for writer</summary>
    public const int WriterApiLevel = 1;
    
    /// <summary>Library version string</summary>
    public const string LibraryVersion = "1.0.0";
    
    /// <summary>PDF/raster specification version</summary>
    public const string PdfRasterSpecVersion = "1.0";
    
    /// <summary>Maximum supported PDF/raster major version</summary>
    public const int MaxSupportedMajorVersion = 1;
    
    /// <summary>Maximum supported PDF/raster minor version</summary>
    public const int MaxSupportedMinorVersion = 0;
    
    /// <summary>PDF version used when writing</summary>
    public const string PdfVersion = "1.7";
    
    /// <summary>Default horizontal DPI</summary>
    public const double DefaultXDpi = 300.0;
    
    /// <summary>Default vertical DPI</summary>
    public const double DefaultYDpi = 300.0;
    
    /// <summary>Size of buffer for reading/parsing</summary>
    internal const int BlockSize = 1024;
    
    /// <summary>Size of tail buffer for trailer parsing</summary>
    internal const int TailSize = 64;
}
