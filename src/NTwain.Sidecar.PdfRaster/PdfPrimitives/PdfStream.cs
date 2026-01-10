// PDF stream value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF stream value (dictionary + data)
/// </summary>
public class PdfStream : PdfDictionary
{
    public byte[] Data { get; set; }
    
    public PdfStream() : this([]) { }
    
    public PdfStream(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
    
    public override PdfValueType Type => PdfValueType.Stream;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        // Update length
        this[PdfNames.Length] = new PdfInteger(Data.Length);
        
        base.WriteTo(writer);
        writer.WriteLine();
        writer.Write("stream\n");
        // Note: actual binary data would be written separately
        writer.Write("endstream");
    }
}
