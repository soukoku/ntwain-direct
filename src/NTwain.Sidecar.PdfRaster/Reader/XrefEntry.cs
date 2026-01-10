// Cross-reference table entry

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Entry in the cross-reference table
/// </summary>
public readonly struct XrefEntry
{
    public long Offset { get; }
    public int Generation { get; }
    public XrefEntryStatus Status { get; }
    
    public XrefEntry(long offset, int generation, XrefEntryStatus status)
    {
        Offset = offset;
        Generation = generation;
        Status = status;
    }
    
    public bool IsInUse => Status == XrefEntryStatus.InUse;
    public bool IsFree => Status == XrefEntryStatus.Free;
}
