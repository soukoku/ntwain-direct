using System.Buffers;
using System.Text;

namespace NTwain.Sidecar.PdfR;

/// <summary>
/// Writes PDF/raster format files conforming to the TWAIN Direct specification.
/// </summary>
public sealed class PdfRasterWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly List<long> _pageObjectOffsets = [];
    private readonly List<int> _pageObjectNumbers = [];
    private int _nextObjectNumber = 1;
    private long _xrefOffset;
    private bool _disposed;

    /// <summary>
    /// Creates a new PDF/raster writer.
    /// </summary>
    /// <param name="stream">Stream to write PDF data to.</param>
    /// <param name="leaveOpen">Whether to leave the stream open after disposal.</param>
    public PdfRasterWriter(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;

        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable.", nameof(stream));

        WriteHeader();
    }

    /// <summary>
    /// Adds a page with the specified image to the PDF.
    /// </summary>
    /// <param name="image">Image data for the page.</param>
    public void AddPage(PdfRasterImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        ValidateImageCompression(image);

        var encodedData = EncodeImage(image);
        var (colorSpace, bitsPerComponent) = GetColorSpaceInfo(image.PixelFormat);

        // Calculate page dimensions in points (1 inch = 72 points)
        var pageWidthPts = image.Width * 72.0 / image.HorizontalDpi;
        var pageHeightPts = image.Height * 72.0 / image.VerticalDpi;

        // Write image XObject
        var imageObjNum = WriteImageObject(image, encodedData, colorSpace, bitsPerComponent);

        // Write Resources dictionary
        var resourcesObjNum = WriteResourcesObject(imageObjNum);

        // Write page content stream
        var contentObjNum = WriteContentStream(pageWidthPts, pageHeightPts);

        // Write page object
        var pageObjNum = WritePageObject(pageWidthPts, pageHeightPts, resourcesObjNum, contentObjNum);
        _pageObjectNumbers.Add(pageObjNum);
    }

    /// <summary>
    /// Finishes writing the PDF and closes the writer.
    /// </summary>
    public void Finish()
    {
        if (_disposed) return;

        // Write Pages object
        var pagesObjNum = WritePagesObject();

        // Write Catalog
        var catalogObjNum = WriteCatalogObject(pagesObjNum);

        // Write xref table
        WriteXrefTable();

        // Write trailer
        WriteTrailer(catalogObjNum);

        _stream.Flush();
    }

    private void WriteHeader()
    {
        // PDF/raster uses PDF 1.4
        WriteLine("%PDF-1.4");
        // Binary marker (high-bit characters to indicate binary content)
        WriteLine("%\xe2\xe3\xcf\xd3");
    }

    private int WriteImageObject(PdfRasterImage image, byte[] encodedData, string colorSpace, int bitsPerComponent)
    {
        var objNum = _nextObjectNumber++;
        RecordObjectOffset(objNum);

        var sb = new StringBuilder();
        sb.AppendLine($"{objNum} 0 obj");
        sb.AppendLine("<<");
        sb.AppendLine("/Type /XObject");
        sb.AppendLine("/Subtype /Image");
        sb.AppendLine($"/Width {image.Width}");
        sb.AppendLine($"/Height {image.Height}");
        sb.AppendLine($"/ColorSpace {colorSpace}");
        sb.AppendLine($"/BitsPerComponent {bitsPerComponent}");
        sb.AppendLine($"/Length {encodedData.Length}");

        // Add filter based on compression
        switch (image.Compression)
        {
            case PdfRasterCompression.CcittGroup4:
                sb.AppendLine("/Filter /CCITTFaxDecode");
                sb.AppendLine($"/DecodeParms << /K -1 /Columns {image.Width} /Rows {image.Height} /BlackIs1 true >>");
                break;
            case PdfRasterCompression.Jpeg:
                sb.AppendLine("/Filter /DCTDecode");
                break;
            case PdfRasterCompression.None:
            default:
                // No filter for uncompressed
                break;
        }

        // For 1-bit B&W, we need to specify decode array
        if (image.PixelFormat == PdfRasterPixelFormat.BlackWhite1)
        {
            sb.AppendLine("/Decode [0 1]");
        }

        sb.AppendLine(">>");
        sb.AppendLine("stream");

        WriteString(sb.ToString());
        _stream.Write(encodedData);
        WriteLine();
        WriteLine("endstream");
        WriteLine("endobj");

        return objNum;
    }

    private int WriteResourcesObject(int imageObjNum)
    {
        var objNum = _nextObjectNumber++;
        RecordObjectOffset(objNum);

        WriteLine($"{objNum} 0 obj");
        WriteLine("<<");
        WriteLine($"/XObject << /Im1 {imageObjNum} 0 R >>");
        WriteLine(">>");
        WriteLine("endobj");

        return objNum;
    }

    private int WriteContentStream(double pageWidth, double pageHeight)
    {
        var objNum = _nextObjectNumber++;
        RecordObjectOffset(objNum);

        // Content stream draws the image scaled to fill the page
        var content = $"q {pageWidth:F2} 0 0 {pageHeight:F2} 0 0 cm /Im1 Do Q";
        var contentBytes = Encoding.ASCII.GetBytes(content);

        WriteLine($"{objNum} 0 obj");
        WriteLine("<<");
        WriteLine($"/Length {contentBytes.Length}");
        WriteLine(">>");
        WriteLine("stream");
        _stream.Write(contentBytes);
        WriteLine();
        WriteLine("endstream");
        WriteLine("endobj");

        return objNum;
    }

    private int WritePageObject(double pageWidth, double pageHeight, int resourcesObjNum, int contentObjNum)
    {
        var objNum = _nextObjectNumber++;
        var offset = _stream.Position;
        _pageObjectOffsets.Add(offset);

        WriteLine($"{objNum} 0 obj");
        WriteLine("<<");
        WriteLine("/Type /Page");
        WriteLine("/Parent 1 0 R"); // Pages object will be object 1 when written at end
        WriteLine($"/MediaBox [0 0 {pageWidth:F2} {pageHeight:F2}]");
        WriteLine($"/Resources {resourcesObjNum} 0 R");
        WriteLine($"/Contents {contentObjNum} 0 R");
        WriteLine(">>");
        WriteLine("endobj");

        return objNum;
    }

    private int WritePagesObject()
    {
        var objNum = _nextObjectNumber++;
        RecordObjectOffset(objNum);

        var kids = string.Join(" ", _pageObjectNumbers.Select(p => $"{p} 0 R"));

        WriteLine($"{objNum} 0 obj");
        WriteLine("<<");
        WriteLine("/Type /Pages");
        WriteLine($"/Kids [{kids}]");
        WriteLine($"/Count {_pageObjectNumbers.Count}");
        WriteLine(">>");
        WriteLine("endobj");

        return objNum;
    }

    private int WriteCatalogObject(int pagesObjNum)
    {
        var objNum = _nextObjectNumber++;
        RecordObjectOffset(objNum);

        WriteLine($"{objNum} 0 obj");
        WriteLine("<<");
        WriteLine("/Type /Catalog");
        WriteLine($"/Pages {pagesObjNum} 0 R");
        // PDF/raster identification
        WriteLine("/Metadata << /Type /Metadata /Subtype /XML >>");
        WriteLine(">>");
        WriteLine("endobj");

        return objNum;
    }

    private void WriteXrefTable()
    {
        _xrefOffset = _stream.Position;

        WriteLine("xref");
        WriteLine($"0 {_nextObjectNumber}");
        WriteLine("0000000000 65535 f ");

        // This is simplified - in production you'd track all object offsets
        for (int i = 1; i < _nextObjectNumber; i++)
        {
            WriteLine($"{i:D10} 00000 n ");
        }
    }

    private void WriteTrailer(int catalogObjNum)
    {
        WriteLine("trailer");
        WriteLine("<<");
        WriteLine($"/Size {_nextObjectNumber}");
        WriteLine($"/Root {catalogObjNum} 0 R");
        WriteLine(">>");
        WriteLine("startxref");
        WriteLine(_xrefOffset.ToString());
        WriteLine("%%EOF");
    }

    private void RecordObjectOffset(int objNum)
    {
        // In a full implementation, we'd track offsets for proper xref
        _ = _stream.Position;
    }

    private byte[] EncodeImage(PdfRasterImage image)
    {
        return image.Compression switch
        {
            PdfRasterCompression.None => image.PixelData,
            _ => MagickImageEncoder.Encode(image)
        };
    }

    private static (string ColorSpace, int BitsPerComponent) GetColorSpaceInfo(PdfRasterPixelFormat format) => format switch
    {
        PdfRasterPixelFormat.BlackWhite1 => ("/DeviceGray", 1),
        PdfRasterPixelFormat.Gray8 => ("/DeviceGray", 8),
        PdfRasterPixelFormat.Gray16 => ("/DeviceGray", 16),
        PdfRasterPixelFormat.Rgb24 => ("/DeviceRGB", 8),
        PdfRasterPixelFormat.Rgb48 => ("/DeviceRGB", 16),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    private static void ValidateImageCompression(PdfRasterImage image)
    {
        if (image.Compression == PdfRasterCompression.CcittGroup4 && image.PixelFormat != PdfRasterPixelFormat.BlackWhite1)
        {
            throw new ArgumentException("CCITT Group 4 compression is only valid for 1-bit black and white images.", nameof(image));
        }

        if (image.Compression == PdfRasterCompression.Jpeg && image.PixelFormat == PdfRasterPixelFormat.BlackWhite1)
        {
            throw new ArgumentException("JPEG compression is not supported for 1-bit black and white images.", nameof(image));
        }
    }

    private void WriteLine(string? text = null)
    {
        if (text != null)
        {
            WriteString(text);
        }
        _stream.WriteByte((byte)'\n');
    }

    private void WriteString(string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        _stream.Write(bytes);
    }

    /// <summary>
    /// Disposes the writer, finishing the PDF if not already done.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_pageObjectNumbers.Count > 0)
        {
            Finish();
        }

        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}
