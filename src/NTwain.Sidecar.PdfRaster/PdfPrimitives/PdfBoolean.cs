// PDF boolean value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF boolean value
/// </summary>
internal class PdfBoolean : PdfValue
{
    public bool Value { get; }
    
    public PdfBoolean(bool value)
    {
        Value = value;
    }
    
    public override PdfValueType Type => PdfValueType.Boolean;
    
    public static PdfBoolean True { get; } = new PdfBoolean(true);
    public static PdfBoolean False { get; } = new PdfBoolean(false);
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write(Value ? "true" : "false");
    }
    
    public static implicit operator bool(PdfBoolean b) => b.Value;
    public static implicit operator PdfBoolean(bool b) => b ? True : False;
}
