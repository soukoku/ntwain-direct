// PDF parser for parsing PDF objects
// Ported from pdfrasread.c parsing functions

using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// PDF object parser
/// </summary>
internal class PdfParser
{
    private readonly PdfTokenizer _tokenizer;
    private readonly XrefTable _xref;
    
    public PdfParser(PdfTokenizer tokenizer, XrefTable xref)
    {
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        _xref = xref ?? throw new ArgumentNullException(nameof(xref));
    }
    
    /// <summary>
    /// Parse any PDF value at the given offset
    /// </summary>
    public PdfValue? ParseValue(ref long offset)
    {
        _tokenizer.SkipWhitespace(ref offset);
        
        int ch = _tokenizer.PeekChar(offset);
        if (ch < 0)
            return null;
        
        // Check for specific value types
        if (ch == '/')
            return ParseName(ref offset);
        
        if (ch == '(')
            return ParseLiteralString(ref offset);
        
        if (ch == '<')
        {
            int next = _tokenizer.PeekChar(offset + 1);
            if (next == '<')
                return ParseDictionaryOrStream(ref offset);
            return ParseHexString(ref offset);
        }
        
        if (ch == '[')
            return ParseArray(ref offset);
        
        if (ch == 't' || ch == 'f')
            return ParseBoolean(ref offset);
        
        if (ch == 'n')
            return ParseNull(ref offset);
        
        // Try number or indirect reference
        if (char.IsDigit((char)ch) || ch == '-' || ch == '+' || ch == '.')
            return ParseNumberOrReference(ref offset);
        
        return null;
    }
    
    /// <summary>
    /// Parse any PDF value at the given offset, including comments
    /// </summary>
    public PdfValue? ParseValueOrComment(ref long offset)
    {
        // Don't skip whitespace first - check for comment
        int ch = _tokenizer.PeekChar(offset);
        
        // Skip leading whitespace but not comments
        while (ch >= 0 && char.IsWhiteSpace((char)ch))
        {
            offset++;
            ch = _tokenizer.PeekChar(offset);
        }
        
        if (ch < 0)
            return null;
        
        // Check for comment
        if (ch == '%')
            return ParseComment(ref offset);
        
        // Otherwise parse as regular value
        return ParseValue(ref offset);
    }
    
    /// <summary>
    /// Parse a comment
    /// </summary>
    public PdfComment? ParseComment(ref long offset)
    {
        if (_tokenizer.TryParseComment(ref offset, out string text))
            return new PdfComment(text);
        return null;
    }
    
    private PdfName? ParseName(ref long offset)
    {
        if (_tokenizer.TryParseName(ref offset, out string name))
            return new PdfName(name);
        return null;
    }
    
    private PdfString? ParseLiteralString(ref long offset)
    {
        if (_tokenizer.TryParseLiteralString(ref offset, out byte[] data))
            return new PdfString(data, false);
        return null;
    }
    
    private PdfString? ParseHexString(ref long offset)
    {
        if (_tokenizer.TryParseHexString(ref offset, out byte[] data))
            return new PdfString(data, true);
        return null;
    }
    
    private PdfBoolean? ParseBoolean(ref long offset)
    {
        if (_tokenizer.TryEat(ref offset, "true"))
            return PdfBoolean.True;
        if (_tokenizer.TryEat(ref offset, "false"))
            return PdfBoolean.False;
        return null;
    }
    
    private PdfNull? ParseNull(ref long offset)
    {
        if (_tokenizer.TryEat(ref offset, "null"))
            return PdfValue.Null as PdfNull;
        return null;
    }
    
    private PdfValue? ParseNumberOrReference(ref long offset)
    {
        long startOffset = offset;
        
        // Try to parse as indirect reference: num gen R
        if (_tokenizer.TryParseULong(ref offset, out ulong num))
        {
            long afterNum = offset;
            if (_tokenizer.TryParseULong(ref offset, out ulong gen))
            {
                if (_tokenizer.TryEat(ref offset, "R"))
                {
                    // It's an indirect reference
                    return new PdfReference((int)num, (int)gen);
                }
            }
            
            // Not a reference, backtrack and try as plain number
            offset = startOffset;
        }
        
        // Parse as number
        if (_tokenizer.TryParseNumber(ref offset, out double value))
        {
            // Check if it's an integer
            if (value == Math.Floor(value) && value >= long.MinValue && value <= long.MaxValue)
                return new PdfInteger((long)value);
            return new PdfReal(value);
        }
        
        return null;
    }
    
    private PdfArray? ParseArray(ref long offset)
    {
        if (!_tokenizer.TryEat(ref offset, "["))
            return null;
        
        var array = new PdfArray();
        
        while (!_tokenizer.TryEat(ref offset, "]"))
        {
            var value = ParseValue(ref offset);
            if (value == null)
                return null; // Parse error
            
            array.Add(value);
        }
        
        return array;
    }
    
    /// <summary>
    /// Parse a dictionary or stream
    /// </summary>
    public PdfValue? ParseDictionaryOrStream(ref long offset)
    {
        if (!_tokenizer.TryEat(ref offset, "<<"))
            return null;
        
        var dict = new PdfDictionary();
        
        while (!_tokenizer.TryEat(ref offset, ">>"))
        {
            // Parse key (must be a name)
            int ch = _tokenizer.PeekChar(offset);
            if (ch != '/')
                return null; // Invalid dictionary key
            
            var keyName = ParseName(ref offset);
            if (keyName == null)
                return null;
            
            // Parse value
            var value = ParseValue(ref offset);
            if (value == null)
                return null;
            
            dict.Add(keyName.Value, value);
        }
        
        // Check if it's a stream
        if (IsStream(offset, out long streamDataOffset, out long streamLength))
        {
            var stream = new PdfStream
            {
                Data = new byte[streamLength]
            };
            
            // Copy dictionary entries
            foreach (var key in dict.Keys)
                stream[key] = dict[key];
            
            // Read stream data
            _tokenizer.Read(streamDataOffset, stream.Data, (int)streamLength);
            
            // Skip past endstream
            offset = streamDataOffset + streamLength;
            _tokenizer.SkipWhitespace(ref offset);
            _tokenizer.TryEat(ref offset, "endstream");
            
            return stream;
        }
        
        return dict;
    }
    
    private bool IsStream(long offset, out long dataOffset, out long dataLength)
    {
        dataOffset = 0;
        dataLength = 0;
        
        long pos = offset;
        
        // Check for "stream" keyword
        if (_tokenizer.PeekChar(pos) != 's' ||
            _tokenizer.PeekChar(pos + 1) != 't' ||
            _tokenizer.PeekChar(pos + 2) != 'r' ||
            _tokenizer.PeekChar(pos + 3) != 'e' ||
            _tokenizer.PeekChar(pos + 4) != 'a' ||
            _tokenizer.PeekChar(pos + 5) != 'm')
        {
            return false;
        }
        
        pos += 6;
        
        // Must be followed by CRLF or LF
        int ch = _tokenizer.PeekChar(pos);
        if (ch == '\r')
        {
            pos++;
            ch = _tokenizer.PeekChar(pos);
            if (ch != '\n')
                return false;
        }
        else if (ch != '\n')
        {
            return false;
        }
        pos++;
        
        dataOffset = pos;
        
        // Need to find length from dictionary - caller should provide it
        // For now, we can't determine length here
        return false;
    }
    
    /// <summary>
    /// Look up a key in a dictionary at the given offset
    /// </summary>
    public PdfValue? DictionaryLookup(long dictOffset, string key)
    {
        long offset = dictOffset;
        
        if (!_tokenizer.TryEat(ref offset, "<<"))
            return null;
        
        while (!_tokenizer.TryEat(ref offset, ">>"))
        {
            // Parse key
            if (_tokenizer.TryParseName(ref offset, out string foundKey))
            {
                if (foundKey == key)
                {
                    // Found it, parse and return the value
                    var value = ParseValue(ref offset);
                    
                    // Resolve indirect reference if needed
                    if (value is PdfReference reference)
                    {
                        if (_xref.TryGetObjectOffset((int)reference.ObjectNumber, out long objOffset))
                        {
                            return ParseObjectAtOffset(objOffset);
                        }
                    }
                    
                    return value;
                }
                
                // Skip this value
                SkipObject(ref offset);
            }
            else
            {
                return null; // Parse error
            }
        }
        
        return null; // Key not found
    }
    
    /// <summary>
    /// Parse an object definition at the given offset (after "obj" keyword)
    /// </summary>
    public PdfValue? ParseObjectAtOffset(long offset)
    {
        // Skip past "num gen obj"
        _tokenizer.SkipWhitespace(ref offset);
        _tokenizer.TryParseULong(ref offset, out _); // object number
        _tokenizer.TryParseULong(ref offset, out _); // generation
        _tokenizer.TryEat(ref offset, "obj");
        
        return ParseValue(ref offset);
    }
    
    /// <summary>
    /// Skip over any PDF object
    /// </summary>
    public bool SkipObject(ref long offset)
    {
        _tokenizer.SkipWhitespace(ref offset);
        
        int ch = _tokenizer.PeekChar(offset);
        if (ch < 0)
            return false;
        
        if (ch == '(')
        {
            _tokenizer.TryParseLiteralString(ref offset, out _);
            return true;
        }
        
        if (ch == '<')
        {
            int next = _tokenizer.PeekChar(offset + 1);
            if (next == '<')
            {
                // Dictionary or stream
                ParseDictionaryOrStream(ref offset);
                return true;
            }
            _tokenizer.TryParseHexString(ref offset, out _);
            return true;
        }
        
        if (ch == '[')
        {
            ParseArray(ref offset);
            return true;
        }
        
        // Number, reference, name, boolean, null
        if (char.IsDigit((char)ch) || ch == '-' || ch == '+')
        {
            // Could be number or indirect reference
            ParseNumberOrReference(ref offset);
            return true;
        }
        
        return _tokenizer.SkipToken(ref offset);
    }
}
