// PDF real (floating-point) value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF real (floating-point) value
/// </summary>
internal class PdfReal : PdfValue
{
    public double Value { get; }
    
    public PdfReal(double value)
    {
        Value = value;
    }
    
    public override PdfValueType Type => PdfValueType.Real;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        // Use format that produces reasonable precision without scientific notation
        writer.Write(Value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
    }
    
    public static implicit operator double(PdfReal r) => r.Value;
    public static implicit operator PdfReal(double d) => new PdfReal(d);
}
