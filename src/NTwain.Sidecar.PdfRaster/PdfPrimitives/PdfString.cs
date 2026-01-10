// PDF string value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF string value
/// </summary>
public class PdfString : PdfValue
{
    public byte[] Data { get; }
    public bool IsHex { get; }
    
    public PdfString(byte[] data, bool isHex = false)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        IsHex = isHex;
    }
    
    public PdfString(string text) : this(System.Text.Encoding.UTF8.GetBytes(text)) { }
    
    public override PdfValueType Type => PdfValueType.String;
    
    public string AsText() => System.Text.Encoding.UTF8.GetString(Data);
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        if (IsHex)
        {
            writer.Write('<');
            foreach (byte b in Data)
                writer.Write($"{b:X2}");
            writer.Write('>');
        }
        else
        {
            writer.Write('(');
            foreach (byte b in Data)
            {
                char c = (char)b;
                switch (c)
                {
                    case '\n': writer.Write("\\n"); break;
                    case '\r': writer.Write("\\r"); break;
                    case '\t': writer.Write("\\t"); break;
                    case '\b': writer.Write("\\b"); break;
                    case '\f': writer.Write("\\f"); break;
                    case '(': writer.Write("\\("); break;
                    case ')': writer.Write("\\)"); break;
                    case '\\': writer.Write("\\\\"); break;
                    default:
                        if (b < 32 || b > 126)
                            writer.Write($"\\{b:000}");
                        else
                            writer.Write(c);
                        break;
                }
            }
            writer.Write(')');
        }
    }
}
