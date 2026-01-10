// Cross-reference table for PDF
// Ported from pdfrasread.c xref handling

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// PDF cross-reference table
/// </summary>
internal class XrefTable
{
    private readonly List<XrefEntry> _entries = new();
    
    public int Count => _entries.Count;
    
    public XrefEntry this[int objectNumber]
    {
        get
        {
            if (objectNumber < 0 || objectNumber >= _entries.Count)
                throw new ArgumentOutOfRangeException(nameof(objectNumber));
            return _entries[objectNumber];
        }
    }
    
    /// <summary>
    /// Add an entry to the xref table
    /// </summary>
    public void Add(long offset, int generation, XrefEntryStatus status)
    {
        _entries.Add(new XrefEntry(offset, generation, status));
    }
    
    /// <summary>
    /// Clear the table
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }
    
    /// <summary>
    /// Try to get the file offset for an object
    /// </summary>
    public bool TryGetObjectOffset(int objectNumber, out long offset)
    {
        if (objectNumber < 0 || objectNumber >= _entries.Count)
        {
            offset = 0;
            return false;
        }
        
        var entry = _entries[objectNumber];
        if (!entry.IsInUse)
        {
            offset = 0;
            return false;
        }
        
        offset = entry.Offset;
        return true;
    }
    
    /// <summary>
    /// Parse an xref table from the given data
    /// </summary>
    public static XrefTable Parse(byte[] data, int offset, int numEntries)
    {
        var table = new XrefTable();
        
        // Each entry is exactly 20 bytes
        const int entrySize = 20;
        
        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = offset + i * entrySize;
            
            // Parse offset (10 digits)
            long objOffset = ParseLongFromBytes(data, entryOffset, 10);
            
            // Parse generation (5 digits after space)
            int gen = (int)ParseLongFromBytes(data, entryOffset + 11, 5);
            
            // Parse status (1 char after space)
            char status = (char)data[entryOffset + 17];
            
            var entryStatus = status == 'n' ? XrefEntryStatus.InUse : XrefEntryStatus.Free;
            table.Add(objOffset, gen, entryStatus);
        }
        
        return table;
    }
    
    private static long ParseLongFromBytes(byte[] data, int offset, int length)
    {
        long value = 0;
        for (int i = 0; i < length; i++)
        {
            char c = (char)data[offset + i];
            if (char.IsDigit(c))
                value = value * 10 + (c - '0');
        }
        return value;
    }
}
