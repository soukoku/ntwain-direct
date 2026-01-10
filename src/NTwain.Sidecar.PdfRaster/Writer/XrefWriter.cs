// PDF cross-reference table for writing
// Ported from PdfXrefTable.h/c

using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Writer;

/// <summary>
/// Entry in the writer's cross-reference table
/// </summary>
internal class XrefWriterEntry
{
    public int ObjectNumber { get; }
    public int Generation { get; }
    public long Offset { get; set; }
    public bool IsFree { get; set; }
    public PdfValue? Value { get; set; }
    
    public XrefWriterEntry(int objectNumber, int generation = 0)
    {
        ObjectNumber = objectNumber;
        Generation = generation;
    }
}

/// <summary>
/// Cross-reference table for PDF writing
/// </summary>
internal class XrefWriter
{
    private readonly List<XrefWriterEntry> _entries = new();
    private int _nextObjectNumber = 1;
    
    public XrefWriter()
    {
        // Object 0 is always free
        var entry0 = new XrefWriterEntry(0, 65535) { IsFree = true };
        _entries.Add(entry0);
    }
    
    public int Count => _entries.Count;
    
    /// <summary>
    /// Allocate a new object number and create a reference
    /// </summary>
    public PdfReference CreateReference(PdfValue? value = null)
    {
        var entry = new XrefWriterEntry(_nextObjectNumber, 0)
        {
            Value = value
        };
        _entries.Add(entry);
        _nextObjectNumber++;
        return new PdfReference(entry.ObjectNumber, 0);
    }
    
    /// <summary>
    /// Record the file offset for an object
    /// </summary>
    public void SetObjectOffset(int objectNumber, long offset)
    {
        var entry = _entries.FirstOrDefault(e => e.ObjectNumber == objectNumber);
        if (entry != null)
            entry.Offset = offset;
    }
    
    /// <summary>
    /// Get the value associated with an object
    /// </summary>
    public PdfValue? GetValue(int objectNumber)
    {
        return _entries.FirstOrDefault(e => e.ObjectNumber == objectNumber)?.Value;
    }
    
    /// <summary>
    /// Set the value for an object
    /// </summary>
    public void SetValue(int objectNumber, PdfValue value)
    {
        var entry = _entries.FirstOrDefault(e => e.ObjectNumber == objectNumber);
        if (entry != null)
            entry.Value = value;
    }
    
    /// <summary>
    /// Get all entries for writing the xref table
    /// </summary>
    public IEnumerable<XrefWriterEntry> GetEntries() => _entries.OrderBy(e => e.ObjectNumber);
}
