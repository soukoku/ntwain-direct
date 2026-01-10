// PDF array value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF array value
/// </summary>
public class PdfArray : PdfValue
{
    private readonly List<PdfValue> _items = new();
    
    public PdfArray() { }
    
    public PdfArray(IEnumerable<PdfValue> items)
    {
        _items.AddRange(items);
    }
    
    public override PdfValueType Type => PdfValueType.Array;
    
    public int Count => _items.Count;
    
    public PdfValue this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }
    
    public void Add(PdfValue value) => _items.Add(value);
    
    public void Insert(int index, PdfValue value) => _items.Insert(index, value);
    
    public void RemoveAt(int index) => _items.RemoveAt(index);
    
    public void Clear() => _items.Clear();
    
    public IEnumerator<PdfValue> GetEnumerator() => _items.GetEnumerator();
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write('[');
        for (int i = 0; i < _items.Count; i++)
        {
            if (i > 0) writer.Write(' ');
            _items[i].WriteTo(writer);
        }
        writer.Write(']');
    }
    
    // Helper methods for common patterns
    public static PdfArray FromDoubles(params double[] values)
    {
        var array = new PdfArray();
        foreach (var v in values)
            array.Add(new PdfReal(v));
        return array;
    }
    
    public static PdfArray FromInts(params int[] values)
    {
        var array = new PdfArray();
        foreach (var v in values)
            array.Add(new PdfInteger(v));
        return array;
    }
}
