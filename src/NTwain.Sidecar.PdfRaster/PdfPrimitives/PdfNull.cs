// PDF null value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF null value
/// </summary>
internal class PdfNull : PdfValue
{
    public override PdfValueType Type => PdfValueType.Null;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write("null");
    }
}
