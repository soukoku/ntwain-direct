// Cross-reference table entry

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Entry in the cross-reference table
/// </summary>
internal readonly struct XrefEntry(long offset, int generation, XrefEntryStatus status)
{
    public long Offset { get; } = offset;
    public int Generation { get; } = generation;
    public XrefEntryStatus Status { get; } = status;
    
    public bool IsInUse => Status == XrefEntryStatus.InUse;
    public bool IsFree => Status == XrefEntryStatus.Free;
}
