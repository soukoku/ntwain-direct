// PDF keyword primitive

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// Represents a PDF keyword (obj, endobj, stream, endstream, xref, trailer, startxref, R)
/// </summary>
public class PdfKeyword : PdfValue
{
    /// <summary>
    /// The keyword text
    /// </summary>
    public string Keyword { get; }
    
    public override PdfValueType Type => PdfValueType.Keyword;
    
    public PdfKeyword(string keyword)
    {
        Keyword = keyword ?? string.Empty;
    }
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write(Keyword);
    }
    
    // Common keywords
    public static PdfKeyword Obj { get; } = new("obj");
    public static PdfKeyword EndObj { get; } = new("endobj");
    public static PdfKeyword Stream { get; } = new("stream");
    public static PdfKeyword EndStream { get; } = new("endstream");
    public static PdfKeyword Xref { get; } = new("xref");
    public static PdfKeyword Trailer { get; } = new("trailer");
    public static PdfKeyword StartXref { get; } = new("startxref");
    public static PdfKeyword R { get; } = new("R");
}
