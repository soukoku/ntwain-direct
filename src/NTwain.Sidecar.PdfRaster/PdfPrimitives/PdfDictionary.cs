// PDF dictionary value

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// PDF dictionary value
/// </summary>
internal class PdfDictionary : PdfValue
{
    private readonly Dictionary<string, PdfValue> _entries = new();
    
    public PdfDictionary() { }
    
    public override PdfValueType Type => PdfValueType.Dictionary;
    
    public int Count => _entries.Count;
    
    public IEnumerable<string> Keys => _entries.Keys;
    
    public PdfValue? this[string key]
    {
        get => _entries.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value == null || value.IsNull)
                _entries.Remove(key);
            else
                _entries[key] = value;
        }
    }
    
    public PdfValue? this[PdfName key]
    {
        get => this[key.Value];
        set => this[key.Value] = value;
    }
    
    public void Add(string key, PdfValue value) => _entries[key] = value;
    
    public void Add(PdfName key, PdfValue value) => _entries[key.Value] = value;
    
    public bool ContainsKey(string key) => _entries.ContainsKey(key);
    
    public bool ContainsKey(PdfName key) => _entries.ContainsKey(key.Value);
    
    public bool TryGetValue(string key, out PdfValue? value)
    {
        if (_entries.TryGetValue(key, out var val))
        {
            value = val;
            return true;
        }
        value = null;
        return false;
    }
    
    public bool TryGetValue(PdfName key, out PdfValue? value) => TryGetValue(key.Value, out value);
    
    public void Remove(string key) => _entries.Remove(key);
    
    public void Remove(PdfName key) => _entries.Remove(key.Value);
    
    public void Clear() => _entries.Clear();
    
    public override void WriteTo(System.IO.TextWriter writer)
    {
        writer.Write("<<");
        foreach (var kvp in _entries)
        {
            writer.Write('/');
            writer.Write(kvp.Key);
            writer.Write(' ');
            kvp.Value.WriteTo(writer);
            writer.Write(' ');
        }
        writer.Write(">>");
    }
    
    // Type-safe getters
    public bool? GetBoolean(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfBoolean b)
            return b.Value;
        return null;
    }
    
    public long? GetInteger(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfInteger i)
            return i.Value;
        return null;
    }
    
    public double? GetNumber(string key)
    {
        if (TryGetValue(key, out var value))
        {
            if (value is PdfInteger i) return i.Value;
            if (value is PdfReal r) return r.Value;
        }
        return null;
    }
    
    public string? GetName(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfName n)
            return n.Value;
        return null;
    }
    
    public string? GetString(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfString s)
            return s.AsText();
        return null;
    }
    
    public PdfArray? GetArray(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfArray a)
            return a;
        return null;
    }
    
    public PdfDictionary? GetDictionary(string key)
    {
        if (TryGetValue(key, out var value) && value is PdfDictionary d)
            return d;
        return null;
    }
}
