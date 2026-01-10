// PDF indirect reference

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF indirect reference
/// </summary>
public class PdfReference : PdfValue
{
    public int ObjectNumber { get; }
    public int Generation { get; }
    public PdfValue? ResolvedValue { get; set; }
    
    public PdfReference(int objectNumber, int generation = 0)
    {
        ObjectNumber = objectNumber;
        Generation = generation;
    }
    
    public override PdfValueType Type => PdfValueType.Reference;
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write($"{ObjectNumber} {Generation} R");
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is PdfReference other)
            return ObjectNumber == other.ObjectNumber && Generation == other.Generation;
        return false;
    }
    
    public override int GetHashCode() => HashCode.Combine(ObjectNumber, Generation);
}
