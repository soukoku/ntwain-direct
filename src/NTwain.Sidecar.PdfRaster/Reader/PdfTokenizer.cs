// PDF tokenizer for parsing PDF syntax
// Ported from pdfrasread.c tokenizing functions

using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Low-level PDF tokenizer
/// </summary>
public class PdfTokenizer
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private long _bufferOffset;
    private int _bufferLength;
    
    public PdfTokenizer(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _buffer = new byte[PdfRasterConstants.BlockSize];
        _bufferOffset = -1;
        _bufferLength = 0;
    }
    
    public long FileSize => _stream.Length;
    
    /// <summary>
    /// Peek at character at given offset without advancing
    /// </summary>
    public int PeekChar(long offset)
    {
        if (!SeekTo(offset))
            return -1;
        
        int bufferIndex = (int)(offset - _bufferOffset);
        return _buffer[bufferIndex];
    }
    
    /// <summary>
    /// Read bytes from stream at given offset
    /// </summary>
    public int Read(long offset, byte[] buffer, int count)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        return _stream.Read(buffer, 0, count);
    }
    
    /// <summary>
    /// Seeks to given offset, returns false if at EOF
    /// </summary>
    private bool SeekTo(long offset)
    {
        if (offset < _bufferOffset || offset >= _bufferOffset + _bufferLength)
        {
            _bufferOffset = offset;
            _stream.Seek(offset, SeekOrigin.Begin);
            _bufferLength = _stream.Read(_buffer, 0, _buffer.Length);
            if (_bufferLength == 0)
                return false;
        }
        return offset < _bufferOffset + _bufferLength;
    }
    
    /// <summary>
    /// Skip whitespace characters including comments
    /// </summary>
    public bool SkipWhitespace(ref long offset)
    {
        bool inComment = false;
        
        while (true)
        {
            int ch = PeekChar(offset);
            if (ch < 0)
                return false;
            
            if (inComment)
            {
                if (ch == '\r' || ch == '\n')
                    inComment = false;
                offset++;
            }
            else if (char.IsWhiteSpace((char)ch))
            {
                offset++;
            }
            else if (ch == '%')
            {
                inComment = true;
                offset++;
            }
            else
            {
                return true;
            }
        }
    }
    
    /// <summary>
    /// Try to parse a comment (line starting with %)
    /// </summary>
    public bool TryParseComment(ref long offset, out string commentText)
    {
        commentText = string.Empty;
        
        int ch = PeekChar(offset);
        if (ch != '%')
            return false;
        
        var chars = new List<char>();
        long pos = offset + 1; // Skip the %
        
        while (true)
        {
            ch = PeekChar(pos);
            if (ch < 0 || ch == '\r' || ch == '\n')
                break;
            
            chars.Add((char)ch);
            pos++;
        }
        
        // Skip the end-of-line character(s)
        if (ch == '\r')
        {
            pos++;
            ch = PeekChar(pos);
            if (ch == '\n')
                pos++;
        }
        else if (ch == '\n')
        {
            pos++;
        }
        
        offset = pos;
        commentText = new string(chars.ToArray());
        return true;
    }
    
    /// <summary>
    /// Check if character is a PDF delimiter
    /// </summary>
    public static bool IsDelimiter(int ch)
    {
        return ch switch
        {
            '(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%' => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Try to match and consume a literal token
    /// </summary>
    public bool TryEat(ref long offset, string literal)
    {
        if (!SkipWhitespace(ref offset))
            return false;
        
        long pos = offset;
        char firstChar = literal[0];
        
        for (int i = 0; i < literal.Length; i++)
        {
            int ch = PeekChar(pos);
            if (ch < 0 || ch != literal[i])
                return false;
            pos++;
        }
        
        // Check for token break
        int next = PeekChar(pos);
        if (next >= 0)
        {
            if (firstChar == '/')
            {
                // Name must be followed by whitespace or delimiter
                if (!char.IsWhiteSpace((char)next) && !IsDelimiter(next))
                    return false;
            }
            else if (firstChar != '<' && firstChar != '>' && !IsDelimiter(firstChar))
            {
                // Regular token must be followed by whitespace or delimiter
                if (!char.IsWhiteSpace((char)next) && !IsDelimiter(next))
                    return false;
            }
        }
        
        offset = pos;
        SkipWhitespace(ref offset);
        return true;
    }
    
    /// <summary>
    /// Try to parse an unsigned long integer
    /// </summary>
    public bool TryParseULong(ref long offset, out ulong value)
    {
        value = 0;
        SkipWhitespace(ref offset);
        
        int ch = PeekChar(offset);
        if (ch < 0 || !char.IsDigit((char)ch))
            return false;
        
        while (ch >= 0 && char.IsDigit((char)ch))
        {
            value = value * 10 + (ulong)(ch - '0');
            offset++;
            ch = PeekChar(offset);
        }
        
        SkipWhitespace(ref offset);
        return true;
    }
    
    /// <summary>
    /// Try to parse a signed number (int or real)
    /// </summary>
    public bool TryParseNumber(ref long offset, out double value)
    {
        value = 0.0;
        SkipWhitespace(ref offset);
        
        long pos = offset;
        
        // Parse sign
        int sign = 1;
        int ch = PeekChar(pos);
        if (ch == '-') { sign = -1; pos++; ch = PeekChar(pos); }
        else if (ch == '+') { pos++; ch = PeekChar(pos); }
        
        // Parse integer part
        double intPart = 0;
        int digits = 0;
        while (ch >= 0 && char.IsDigit((char)ch))
        {
            intPart = intPart * 10 + (ch - '0');
            digits++;
            pos++;
            ch = PeekChar(pos);
        }
        
        // Parse fractional part
        double fraction = 0;
        int precision = 0;
        if (ch == '.')
        {
            pos++;
            ch = PeekChar(pos);
            while (ch >= 0 && char.IsDigit((char)ch))
            {
                fraction = fraction * 10 + (ch - '0');
                precision++;
                pos++;
                ch = PeekChar(pos);
            }
        }
        
        if (digits + precision == 0)
            return false;
        
        value = sign * (intPart + fraction * Math.Pow(10, -precision));
        offset = pos;
        SkipWhitespace(ref offset);
        return true;
    }
    
    /// <summary>
    /// Parse a literal string (...)
    /// </summary>
    public bool TryParseLiteralString(ref long offset, out byte[] data)
    {
        data = Array.Empty<byte>();
        
        int ch = PeekChar(offset);
        if (ch != '(')
            return false;
        
        var bytes = new List<byte>();
        int nesting = 0;
        long pos = offset;
        
        do
        {
            ch = PeekChar(pos);
            switch (ch)
            {
                case ')':
                    nesting--;
                    if (nesting > 0)
                        bytes.Add((byte)ch);
                    break;
                case '(':
                    nesting++;
                    if (nesting > 1)
                        bytes.Add((byte)ch);
                    break;
                case '\\':
                    pos++;
                    ch = PeekChar(pos);
                    switch (ch)
                    {
                        case 'n': bytes.Add((byte)'\n'); break;
                        case 'r': bytes.Add((byte)'\r'); break;
                        case 't': bytes.Add((byte)'\t'); break;
                        case 'b': bytes.Add((byte)'\b'); break;
                        case 'f': bytes.Add((byte)'\f'); break;
                        case '(': bytes.Add((byte)'('); break;
                        case ')': bytes.Add((byte)')'); break;
                        case '\\': bytes.Add((byte)'\\'); break;
                        case >= '0' and <= '7':
                            // Octal escape
                            int octal = ch - '0';
                            ch = PeekChar(pos + 1);
                            if (ch >= '0' && ch <= '7')
                            {
                                pos++;
                                octal = octal * 8 + (ch - '0');
                                ch = PeekChar(pos + 1);
                                if (ch >= '0' && ch <= '7')
                                {
                                    pos++;
                                    octal = octal * 8 + (ch - '0');
                                }
                            }
                            bytes.Add((byte)octal);
                            break;
                        default:
                            // Ignore backslash
                            break;
                    }
                    break;
                case -1:
                    return false; // Unexpected EOF
                default:
                    bytes.Add((byte)ch);
                    break;
            }
            pos++;
        } while (nesting > 0);
        
        offset = pos;
        SkipWhitespace(ref offset);
        data = bytes.ToArray();
        return true;
    }
    
    /// <summary>
    /// Parse a hex string <...>
    /// </summary>
    public bool TryParseHexString(ref long offset, out byte[] data)
    {
        data = Array.Empty<byte>();
        
        int ch = PeekChar(offset);
        if (ch != '<')
            return false;
        
        var bytes = new List<byte>();
        long pos = offset + 1;
        int nibble = -1;
        
        while (true)
        {
            ch = PeekChar(pos);
            if (ch < 0)
                return false; // Unexpected EOF
            
            if (ch == '>')
            {
                if (nibble >= 0)
                    bytes.Add((byte)(nibble << 4));
                pos++;
                break;
            }
            
            if (char.IsWhiteSpace((char)ch))
            {
                pos++;
                continue;
            }
            
            int digit;
            if (ch >= '0' && ch <= '9')
                digit = ch - '0';
            else if (ch >= 'A' && ch <= 'F')
                digit = ch - 'A' + 10;
            else if (ch >= 'a' && ch <= 'f')
                digit = ch - 'a' + 10;
            else
                return false; // Invalid character
            
            if (nibble < 0)
                nibble = digit;
            else
            {
                bytes.Add((byte)((nibble << 4) | digit));
                nibble = -1;
            }
            pos++;
        }
        
        offset = pos;
        SkipWhitespace(ref offset);
        data = bytes.ToArray();
        return true;
    }
    
    /// <summary>
    /// Parse a name /...
    /// </summary>
    public bool TryParseName(ref long offset, out string name)
    {
        name = "";
        
        int ch = PeekChar(offset);
        if (ch != '/')
            return false;
        
        var chars = new List<char>();
        long pos = offset + 1;
        
        while (true)
        {
            ch = PeekChar(pos);
            if (ch < 0 || char.IsWhiteSpace((char)ch) || IsDelimiter(ch))
                break;
            
            if (ch == '#')
            {
                // Hex escape
                int d1 = PeekChar(pos + 1);
                int d2 = PeekChar(pos + 2);
                if (d1 >= 0 && d2 >= 0)
                {
                    int h1 = HexDigit(d1);
                    int h2 = HexDigit(d2);
                    if (h1 >= 0 && h2 >= 0)
                    {
                        chars.Add((char)((h1 << 4) | h2));
                        pos += 3;
                        continue;
                    }
                }
            }
            
            chars.Add((char)ch);
            pos++;
        }
        
        offset = pos;
        SkipWhitespace(ref offset);
        name = new string(chars.ToArray());
        return true;
    }
    
    private static int HexDigit(int ch)
    {
        if (ch >= '0' && ch <= '9') return ch - '0';
        if (ch >= 'A' && ch <= 'F') return ch - 'A' + 10;
        if (ch >= 'a' && ch <= 'f') return ch - 'a' + 10;
        return -1;
    }
    
    /// <summary>
    /// Skip over a token
    /// </summary>
    public bool SkipToken(ref long offset)
    {
        if (!SkipWhitespace(ref offset))
            return false;
        
        int ch = PeekChar(offset);
        char ch0 = (char)ch;
        
        while (true)
        {
            offset++;
            int next = PeekChar(offset);
            if (next < 0)
                break;
            
            char nextCh = (char)next;
            
            if (ch0 == '/')
            {
                if (char.IsWhiteSpace(nextCh) || IsDelimiter(next))
                    break;
            }
            else if ((ch0 == '<' || ch0 == '>') && nextCh == ch0)
            {
                offset++;
                break;
            }
            else if (IsDelimiter(ch))
            {
                break;
            }
            else if (char.IsWhiteSpace(nextCh) || IsDelimiter(next))
            {
                break;
            }
        }
        
        SkipWhitespace(ref offset);
        return true;
    }
}
