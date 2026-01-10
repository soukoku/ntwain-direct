// Cross-reference table entry (shared between reader and writer)

namespace NTwain.Sidecar.PdfRaster.PdfObjects;

/// <summary>
/// Entry status in xref table
/// </summary>
internal enum XrefEntryStatus
{
    Free,
    InUse
}

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
    
    /// <summary>
    /// Create an in-use entry
    /// </summary>
    public static XrefEntry InUse(long offset, int generation = 0) 
        => new(offset, generation, XrefEntryStatus.InUse);
    
    /// <summary>
    /// Create a free entry
    /// </summary>
    public static XrefEntry Free(int generation = 65535) 
        => new(0, generation, XrefEntryStatus.Free);
}
