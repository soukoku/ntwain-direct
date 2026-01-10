// PDF name value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF name value (e.g., /Type, /Page)
/// </summary>
internal class PdfName : PdfValue
{
    public string Value { get; }
    
    public PdfName(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public override PdfValueType Type => PdfValueType.Name;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write('/');
        foreach (char c in Value)
        {
            if (c < 33 || c > 126 || c == '#' || c == '/' || c == '%' || 
                c == '(' || c == ')' || c == '<' || c == '>' || c == '[' || c == ']' || c == '{' || c == '}')
            {
                writer.Write($"#{(int)c:X2}");
            }
            else
            {
                writer.Write(c);
            }
        }
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is PdfName other)
            return Value == other.Value;
        return false;
    }
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static implicit operator string(PdfName n) => n.Value;
    public static implicit operator PdfName(string s) => new PdfName(s);
}
