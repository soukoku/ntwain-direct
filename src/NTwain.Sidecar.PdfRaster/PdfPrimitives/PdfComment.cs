// PDF comment primitive

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// Represents a PDF comment (line starting with %)
/// </summary>
internal class PdfComment : PdfValue
{
    /// <summary>
    /// The comment text (without the leading %)
    /// </summary>
    public string Text { get; }
    
    public override PdfValueType Type => PdfValueType.Comment;
    
    public PdfComment(string text)
    {
        Text = text ?? string.Empty;
    }
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write('%');
        writer.Write(Text);
    }
}
