// PDF error value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF error value (internal use)
/// </summary>
internal class PdfError : PdfValue
{
    public override PdfValueType Type => PdfValueType.Error;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write("<<error>>");
    }
}
