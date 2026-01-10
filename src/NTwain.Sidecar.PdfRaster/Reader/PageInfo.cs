// Page and strip information structures
// Ported from pdfrasread.c info structures

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Information about an ICC color profile
/// </summary>
public class IccProfile
{
    /// <summary>File position of the profile data</summary>
    public long DataPosition { get; set; }
    
    /// <summary>Length of the profile data</summary>
    public long DataLength { get; set; }
    
    /// <summary>The profile data (if loaded)</summary>
    public byte[]? Data { get; set; }
}

/// <summary>
/// Colorspace information
/// </summary>
public class ColorspaceInfo
{
    /// <summary>Style/type of colorspace</summary>
    public ColorspaceStyle Style { get; set; }
    
    /// <summary>Bits per component</summary>
    public int BitsPerComponent { get; set; }
    
    /// <summary>White point [X, Y, Z]</summary>
    public double[] WhitePoint { get; set; } = new double[3];
    
    /// <summary>Black point [X, Y, Z]</summary>
    public double[] BlackPoint { get; set; } = new double[3];
    
    /// <summary>Gamma exponent</summary>
    public double Gamma { get; set; } = 1.0;
    
    /// <summary>CalRGB matrix (9 values)</summary>
    public double[] Matrix { get; set; } = new double[9];
    
    /// <summary>ICC profile (if ICCBased colorspace)</summary>
    public IccProfile? IccProfile { get; set; }
    
    /// <summary>
    /// Check if two colorspaces are equal
    /// </summary>
    public bool Equals(ColorspaceInfo? other)
    {
        if (other == null) return false;
        if (Style != other.Style) return false;
        if (BitsPerComponent != other.BitsPerComponent) return false;
        if (Math.Abs(Gamma - other.Gamma) > 0.00001) return false;
        
        for (int i = 0; i < 3; i++)
        {
            if (Math.Abs(WhitePoint[i] - other.WhitePoint[i]) > 0.00001) return false;
            if (Math.Abs(BlackPoint[i] - other.BlackPoint[i]) > 0.00001) return false;
        }
        
        if (IccProfile != null && other.IccProfile != null)
        {
            if (IccProfile.DataPosition != other.IccProfile.DataPosition) return false;
            if (IccProfile.DataLength != other.IccProfile.DataLength) return false;
        }
        else if (IccProfile != null || other.IccProfile != null)
        {
            return false;
        }
        
        return true;
    }
}

/// <summary>
/// Information about a single page
/// </summary>
public class PageInfo
{
    /// <summary>File offset of the page object</summary>
    public long Offset { get; set; }
    
    /// <summary>MediaBox [llx, lly, urx, ury]</summary>
    public double[] MediaBox { get; set; } = new double[4];
    
    /// <summary>Pixel format of the page image</summary>
    public RasterPixelFormat Format { get; set; }
    
    /// <summary>Colorspace information</summary>
    public ColorspaceInfo Colorspace { get; set; } = new();
    
    /// <summary>Width in pixels</summary>
    public int Width { get; set; }
    
    /// <summary>Height in pixels</summary>
    public int Height { get; set; }
    
    /// <summary>Rotation in degrees clockwise (0, 90, 180, 270)</summary>
    public int Rotation { get; set; }
    
    /// <summary>Horizontal resolution in DPI</summary>
    public double XDpi { get; set; }
    
    /// <summary>Vertical resolution in DPI</summary>
    public double YDpi { get; set; }
    
    /// <summary>Number of strips on this page</summary>
    public int StripCount { get; set; }
    
    /// <summary>Maximum raw strip size in bytes</summary>
    public int MaxStripSize { get; set; }
}

/// <summary>
/// Information about a single strip
/// </summary>
public class StripInfo
{
    /// <summary>File position of the strip stream dictionary</summary>
    public long Position { get; set; }
    
    /// <summary>File position of the strip data</summary>
    public long DataPosition { get; set; }
    
    /// <summary>Size of raw (compressed) strip data</summary>
    public int RawSize { get; set; }
    
    /// <summary>Compression used</summary>
    public RasterCompression Compression { get; set; }
    
    /// <summary>Pixel format</summary>
    public RasterPixelFormat Format { get; set; }
    
    /// <summary>Colorspace information</summary>
    public ColorspaceInfo Colorspace { get; set; } = new();
    
    /// <summary>Width in pixels</summary>
    public int Width { get; set; }
    
    /// <summary>Height in pixels (of this strip)</summary>
    public int Height { get; set; }
    
    /// <summary>CCITT K parameter (-1 for Group 4, 0 for Group 3 1D, >0 for Group 3 2D)</summary>
    public int CcittK { get; set; } = -1;
    
    /// <summary>Whether black pixels are encoded as 1 (true) or 0 (false)</summary>
    public bool BlackIs1 { get; set; } = true;
}

/// <summary>
/// CCITT decode parameters
/// </summary>
public class CcittDecodeParams
{
    /// <summary>K parameter: -1 for Group 4, 0 for Group 3 1D, positive for Group 3 2D</summary>
    public int K { get; set; } = -1;
    
    /// <summary>Number of columns (width in pixels)</summary>
    public int Columns { get; set; }
    
    /// <summary>Number of rows (height in pixels)</summary>
    public int Rows { get; set; }
    
    /// <summary>Whether black is 1 (true) or 0 (false)</summary>
    public bool BlackIs1 { get; set; } = true;
    
    /// <summary>End of line markers present</summary>
    public bool EndOfLine { get; set; }
    
    /// <summary>Byte aligned rows</summary>
    public bool EncodedByteAlign { get; set; }
    
    /// <summary>End of block marker present</summary>
    public bool EndOfBlock { get; set; } = true;
}
