// PDF integer value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF integer value
/// </summary>
internal class PdfInteger : PdfValue
{
    public long Value { get; }
    
    public PdfInteger(long value)
    {
        Value = value;
    }
    
    public override PdfValueType Type => PdfValueType.Integer;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write(Value);
    }
    
    public static implicit operator long(PdfInteger i) => i.Value;
    public static implicit operator int(PdfInteger i) => (int)i.Value;
    public static implicit operator PdfInteger(long i) => new PdfInteger(i);
    public static implicit operator PdfInteger(int i) => new PdfInteger(i);
}
