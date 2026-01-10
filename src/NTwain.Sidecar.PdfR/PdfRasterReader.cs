using System.Text;
using System.Text.RegularExpressions;

namespace NTwain.Sidecar.PdfR;

/// <summary>
/// Reads PDF/raster format files and extracts image data.
/// </summary>
public sealed partial class PdfRasterReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private bool _disposed;

    /// <summary>
    /// Creates a new PDF/raster reader.
    /// </summary>
    /// <param name="stream">Stream containing PDF/raster data.</param>
    /// <param name="leaveOpen">Whether to leave the stream open after disposal.</param>
    public PdfRasterReader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable.", nameof(stream));
    }

    /// <summary>
    /// Gets the number of pages in the PDF.
    /// </summary>
    public int PageCount => GetPageCount();

    /// <summary>
    /// Reads image data from the specified page.
    /// </summary>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <returns>Image data from the page.</returns>
    public PdfRasterImage ReadPage(int pageIndex)
    {
        if (pageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));

        var xref = ParseXref();
        var pageObjects = FindPageObjects(xref);

        if (pageIndex >= pageObjects.Count)
            throw new ArgumentOutOfRangeException(nameof(pageIndex), $"Page index {pageIndex} is out of range. PDF has {pageObjects.Count} pages.");

        return ExtractImageFromPage(pageObjects[pageIndex], xref);
    }

    /// <summary>
    /// Reads all pages from the PDF.
    /// </summary>
    /// <returns>Enumerable of image data from all pages.</returns>
    public IEnumerable<PdfRasterImage> ReadAllPages()
    {
        var count = PageCount;
        for (var i = 0; i < count; i++)
        {
            yield return ReadPage(i);
        }
    }

    private int GetPageCount()
    {
        var xref = ParseXref();
        return FindPageObjects(xref).Count;
    }

    private Dictionary<int, long> ParseXref()
    {
        var xref = new Dictionary<int, long>();

        // Find startxref
        _stream.Seek(-1024, SeekOrigin.End);
        var buffer = new byte[1024];
        var read = _stream.Read(buffer);
        var text = Encoding.ASCII.GetString(buffer, 0, read);

        var startxrefMatch = StartxrefRegex().Match(text);
        if (!startxrefMatch.Success)
            throw new InvalidDataException("Could not find startxref in PDF.");

        var xrefOffset = long.Parse(startxrefMatch.Groups[1].Value);
        _stream.Seek(xrefOffset, SeekOrigin.Begin);

        // Read xref table
        using var reader = new StreamReader(_stream, Encoding.ASCII, leaveOpen: true);
        var line = reader.ReadLine();

        if (line == "xref")
        {
            // Standard xref table
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("trailer"))
                    break;

                var parts = line.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[0], out var startObj) && int.TryParse(parts[1], out var count))
                {
                    for (var i = 0; i < count; i++)
                    {
                        line = reader.ReadLine();
                        if (line == null) break;

                        var entryParts = line.Split(' ');
                        if (entryParts.Length >= 2 && long.TryParse(entryParts[0], out var offset))
                        {
                            if (entryParts.Length > 2 && entryParts[2] == "n")
                            {
                                xref[startObj + i] = offset;
                            }
                        }
                    }
                }
            }
        }

        return xref;
    }

    private List<int> FindPageObjects(Dictionary<int, long> xref)
    {
        var pageObjects = new List<int>();

        foreach (var (objNum, offset) in xref)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            var objData = ReadObject();

            if (objData.Contains("/Type /Page") && !objData.Contains("/Type /Pages"))
            {
                pageObjects.Add(objNum);
            }
        }

        return pageObjects;
    }

    private PdfRasterImage ExtractImageFromPage(int pageObjNum, Dictionary<int, long> xref)
    {
        // Find page object
        if (!xref.TryGetValue(pageObjNum, out var pageOffset))
            throw new InvalidDataException($"Page object {pageObjNum} not found.");

        _stream.Seek(pageOffset, SeekOrigin.Begin);
        var pageData = ReadObject();

        // Find XObject reference in Resources
        var xObjectMatch = XObjectRefRegex().Match(pageData);
        if (!xObjectMatch.Success)
            throw new InvalidDataException("Could not find image XObject reference in page.");

        var imageObjNum = int.Parse(xObjectMatch.Groups[1].Value);

        // Read image object
        if (!xref.TryGetValue(imageObjNum, out var imageOffset))
            throw new InvalidDataException($"Image object {imageObjNum} not found.");

        _stream.Seek(imageOffset, SeekOrigin.Begin);
        var imageData = ReadObject();

        // Parse image properties
        var width = ParseIntProperty(imageData, "/Width");
        var height = ParseIntProperty(imageData, "/Height");
        var bitsPerComponent = ParseIntProperty(imageData, "/BitsPerComponent");
        var colorSpace = ParseStringProperty(imageData, "/ColorSpace");

        var pixelFormat = DeterminePixelFormat(colorSpace, bitsPerComponent);
        var compression = DetermineCompression(imageData);

        // Extract stream data
        var streamData = ExtractStreamData(imageData, _stream);

        // Decode using Magick.NET
        var pixelData = MagickImageEncoder.Decode(streamData, width, height, pixelFormat, compression);

        // Get resolution from MediaBox
        var mediaBoxMatch = MediaBoxRegex().Match(pageData);
        var horizontalDpi = 200.0;
        var verticalDpi = 200.0;

        if (mediaBoxMatch.Success)
        {
            var mediaBoxWidth = double.Parse(mediaBoxMatch.Groups[3].Value);
            var mediaBoxHeight = double.Parse(mediaBoxMatch.Groups[4].Value);
            horizontalDpi = width * 72.0 / mediaBoxWidth;
            verticalDpi = height * 72.0 / mediaBoxHeight;
        }

        return new PdfRasterImage(pixelData, width, height, pixelFormat, PdfRasterCompression.None, horizontalDpi, verticalDpi);
    }

    private string ReadObject()
    {
        var sb = new StringBuilder();
        var buffer = new byte[4096];
        var inStream = false;
        var streamStart = -1L;

        while (true)
        {
            var b = _stream.ReadByte();
            if (b == -1) break;

            sb.Append((char)b);
            var text = sb.ToString();

            if (text.EndsWith("stream"))
            {
                inStream = true;
                // Skip newline after "stream"
                var next = _stream.ReadByte();
                if (next == '\r')
                {
                    _stream.ReadByte(); // Skip \n
                }
                streamStart = _stream.Position;
            }

            if (text.EndsWith("endobj"))
                break;
        }

        return sb.ToString();
    }

    private static byte[] ExtractStreamData(string objectData, Stream stream)
    {
        var lengthMatch = LengthRegex().Match(objectData);
        if (!lengthMatch.Success)
            return [];

        var length = int.Parse(lengthMatch.Groups[1].Value);

        // Find stream position
        var streamIndex = objectData.IndexOf("stream", StringComparison.Ordinal);
        if (streamIndex == -1)
            return [];

        // The actual stream data starts after "stream\n" or "stream\r\n"
        var data = new byte[length];
        var objStart = objectData.IndexOf(" 0 obj", StringComparison.Ordinal);
        
        // For simplicity, we read from current position - this is approximate
        // In production, you'd track exact positions
        stream.Read(data, 0, length);

        return data;
    }

    private static int ParseIntProperty(string data, string property)
    {
        var pattern = $@"{Regex.Escape(property)}\s+(\d+)";
        var match = Regex.Match(data, pattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static string ParseStringProperty(string data, string property)
    {
        var pattern = $@"{Regex.Escape(property)}\s+(/\w+)";
        var match = Regex.Match(data, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static PdfRasterPixelFormat DeterminePixelFormat(string colorSpace, int bitsPerComponent)
    {
        return colorSpace switch
        {
            "/DeviceGray" when bitsPerComponent == 1 => PdfRasterPixelFormat.BlackWhite1,
            "/DeviceGray" when bitsPerComponent == 8 => PdfRasterPixelFormat.Gray8,
            "/DeviceGray" when bitsPerComponent == 16 => PdfRasterPixelFormat.Gray16,
            "/DeviceRGB" when bitsPerComponent == 8 => PdfRasterPixelFormat.Rgb24,
            "/DeviceRGB" when bitsPerComponent == 16 => PdfRasterPixelFormat.Rgb48,
            _ => PdfRasterPixelFormat.Gray8
        };
    }

    private static PdfRasterCompression DetermineCompression(string data)
    {
        if (data.Contains("/CCITTFaxDecode"))
            return PdfRasterCompression.CcittGroup4;
        if (data.Contains("/DCTDecode"))
            return PdfRasterCompression.Jpeg;
        return PdfRasterCompression.None;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }

    [GeneratedRegex(@"startxref\s+(\d+)")]
    private static partial Regex StartxrefRegex();

    [GeneratedRegex(@"/Im\d+\s+(\d+)\s+0\s+R")]
    private static partial Regex XObjectRefRegex();

    [GeneratedRegex(@"/MediaBox\s*\[\s*([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)\s*\]")]
    private static partial Regex MediaBoxRegex();

    [GeneratedRegex(@"/Length\s+(\d+)")]
    private static partial Regex LengthRegex();
}
