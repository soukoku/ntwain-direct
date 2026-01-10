// PDF output stream for writing
// Ported from PdfStreaming.h/c

using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Writer;

/// <summary>
/// PDF output stream writer
/// </summary>
internal class PdfOutputStream
{
    private readonly Stream _stream;
    private readonly XrefWriter _xref;
    
    public PdfOutputStream(Stream stream, XrefWriter xref)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _xref = xref ?? throw new ArgumentNullException(nameof(xref));
    }
    
    public long Position => _stream.Position;
    
    /// <summary>
    /// Write the PDF header
    /// </summary>
    public void WriteHeader(string pdfVersion = "1.7")
    {
        Write($"%PDF-{pdfVersion}\n");
        // Binary comment to mark this as binary
        Write("%\xE2\xE3\xCF\xD3\n");
    }
    
    /// <summary>
    /// Write a string
    /// </summary>
    public void Write(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        _stream.Write(bytes, 0, bytes.Length);
    }
    
    /// <summary>
    /// Write bytes
    /// </summary>
    public void Write(byte[] data, int offset, int count)
    {
        _stream.Write(data, offset, count);
    }
    
    /// <summary>
    /// Write a PDF value
    /// </summary>
    public void WriteValue(PdfValue value)
    {
        switch (value)
        {
            case PdfNull:
                Write("null");
                break;
                
            case PdfBoolean b:
                Write(b.Value ? "true" : "false");
                break;
                
            case PdfInteger i:
                Write(i.Value.ToString());
                break;
                
            case PdfReal r:
                Write(r.Value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                break;
                
            case PdfName n:
                WriteName(n.Value);
                break;
                
            case PdfString s:
                WriteString(s);
                break;
                
            case PdfArray a:
                WriteArray(a);
                break;
                
            case PdfStream s:
                WriteStream(s);
                break;
                
            case PdfDictionary d:
                WriteDictionary(d);
                break;
                
            case PdfReference r:
                Write($"{r.ObjectNumber} {r.Generation} R");
                break;
        }
    }
    
    /// <summary>
    /// Write a name value
    /// </summary>
    private void WriteName(string name)
    {
        Write("/");
        foreach (char c in name)
        {
            if (c < 33 || c > 126 || c == '#' || c == '/' || c == '%' ||
                c == '(' || c == ')' || c == '<' || c == '>' || c == '[' || c == ']')
            {
                Write($"#{(int)c:X2}");
            }
            else
            {
                Write(c.ToString());
            }
        }
    }
    
    /// <summary>
    /// Write a string value
    /// </summary>
    private void WriteString(PdfString str)
    {
        if (str.IsHex)
        {
            Write("<");
            foreach (byte b in str.Data)
                Write($"{b:X2}");
            Write(">");
        }
        else
        {
            Write("(");
            foreach (byte b in str.Data)
            {
                char c = (char)b;
                switch (c)
                {
                    case '\n': Write("\\n"); break;
                    case '\r': Write("\\r"); break;
                    case '\t': Write("\\t"); break;
                    case '(': Write("\\("); break;
                    case ')': Write("\\)"); break;
                    case '\\': Write("\\\\"); break;
                    default:
                        if (b < 32 || b > 126)
                            Write($"\\{b:000}");
                        else
                            Write(c.ToString());
                        break;
                }
            }
            Write(")");
        }
    }
    
    /// <summary>
    /// Write an array
    /// </summary>
    private void WriteArray(PdfArray array)
    {
        Write("[");
        for (int i = 0; i < array.Count; i++)
        {
            if (i > 0) Write(" ");
            WriteValue(array[i]);
        }
        Write("]");
    }
    
    /// <summary>
    /// Write a dictionary
    /// </summary>
    private void WriteDictionary(PdfDictionary dict)
    {
        Write("<<");
        foreach (var key in dict.Keys)
        {
            WriteName(key);
            Write(" ");
            WriteValue(dict[key]!);
            Write(" ");
        }
        Write(">>");
    }
    
    /// <summary>
    /// Write a stream
    /// </summary>
    private void WriteStream(PdfStream stream)
    {
        // Update length
        stream[PdfNames.Length] = new PdfInteger(stream.Data.Length);
        
        WriteDictionary(stream);
        Write("\nstream\n");
        Write(stream.Data, 0, stream.Data.Length);
        Write("\nendstream");
    }
    
    /// <summary>
    /// Write an object definition
    /// </summary>
    public void WriteObject(int objectNumber, int generation, PdfValue value)
    {
        // Record position in xref
        _xref.SetObjectOffset(objectNumber, Position);
        
        Write($"{objectNumber} {generation} obj\n");
        WriteValue(value);
        Write("\nendobj\n");
    }
    
    /// <summary>
    /// Write an indirect reference and its definition
    /// </summary>
    public void WriteReferenceDefinition(PdfReference reference)
    {
        var value = _xref.GetValue(reference.ObjectNumber);
        if (value != null)
        {
            WriteObject(reference.ObjectNumber, reference.Generation, value);
        }
    }
    
    /// <summary>
    /// Write the xref table
    /// </summary>
    public long WriteXrefTable()
    {
        long xrefOffset = Position;
        Write("xref\n");
        Write($"0 {_xref.Count}\n");
        
        foreach (var entry in _xref.GetEntries())
        {
            if (entry.IsFree)
            {
                Write($"{0:D10} {entry.Generation:D5} f \n");
            }
            else
            {
                Write($"{entry.Offset:D10} {entry.Generation:D5} n \n");
            }
        }
        
        return xrefOffset;
    }
    
    /// <summary>
    /// Write the trailer dictionary
    /// </summary>
    public void WriteTrailer(PdfDictionary trailer, long xrefOffset)
    {
        Write("trailer\n");
        WriteDictionary(trailer);
        Write("\nstartxref\n");
        Write($"{xrefOffset}\n");
        Write("%%EOF\n");
    }
    
    /// <summary>
    /// Write PDF/raster signature before startxref
    /// </summary>
    public void WritePdfRasterSignature()
    {
        Write($"%PDF-raster-{PdfRasterConstants.PdfRasterSpecVersion}\n");
    }
    
    public void Flush()
    {
        _stream.Flush();
    }
}
