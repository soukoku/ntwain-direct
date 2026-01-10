// PDF/raster reader
// Ported from pdfrasread.c

using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Reader;

/// <summary>
/// Error handler delegate for PDF reader
/// </summary>
public delegate void PdfReaderErrorHandler(PdfRasterReader? reader, ErrorLevel level, ReadErrorCode code, long offset, string message);

/// <summary>
/// PDF/raster file reader
/// </summary>
public class PdfRasterReader : IDisposable
{
    private Stream? _stream;
    private bool _ownsStream;
    private bool _isOpen;
    private bool _disposed;
    
    private PdfTokenizer? _tokenizer;
    private XrefTable? _xref;
    private PdfParser? _parser;
    
    private int _pageCount = -1;
    private long[]? _pageTable;
    private long _catalogPosition;
    private long _trailerPosition;
    
    private int _majorVersion;
    private int _minorVersion;
    
    // Cache for page info to avoid re-parsing
    private Dictionary<int, PageInfo> _pageInfoCache = new();
    private Dictionary<(int page, int strip), StripInfo> _stripInfoCache = new();
    
    public PdfReaderErrorHandler? ErrorHandler { get; set; }
    
    /// <summary>
    /// Gets whether a file is currently open
    /// </summary>
    public bool IsOpen => _isOpen;
    
    /// <summary>
    /// Gets the PDF/raster major version
    /// </summary>
    public int MajorVersion => _majorVersion;
    
    /// <summary>
    /// Gets the PDF/raster minor version
    /// </summary>
    public int MinorVersion => _minorVersion;
    
    /// <summary>
    /// Gets the library version
    /// </summary>
    public static string LibraryVersion => PdfRasterConstants.LibraryVersion;
    
    /// <summary>
    /// Opens a PDF/raster stream for reading
    /// </summary>
    /// <param name="stream">A readable, seekable stream</param>
    /// <param name="leaveOpen">If true, the stream will not be disposed when the reader is closed</param>
    public bool Open(Stream stream, bool leaveOpen = false)
    {
        if (_isOpen)
            throw new PdfApiException("Reader is already open");
        
        ArgumentNullException.ThrowIfNull(stream);
        
        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(stream));
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));
        
        _stream = stream;
        _ownsStream = !leaveOpen;
        _tokenizer = new PdfTokenizer(_stream);
        _xref = new XrefTable();
        _parser = new PdfParser(_tokenizer, _xref);
        
        try
        {
            if (!ParseTrailer())
            {
                Close();
                return false;
            }
            
            _isOpen = true;
            return true;
        }
        catch
        {
            Close();
            throw;
        }
    }
    
    /// <summary>
    /// Opens a file by path
    /// </summary>
    public bool Open(string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Open(stream, leaveOpen: false);
    }
    
    /// <summary>
    /// Closes the currently open file
    /// </summary>
    public void Close()
    {
        _isOpen = false;
        _pageTable = null;
        _xref?.Clear();
        _pageCount = -1;
        _catalogPosition = 0;
        _trailerPosition = 0;
        _pageInfoCache.Clear();
        _stripInfoCache.Clear();
        
        if (_ownsStream)
        {
            _stream?.Dispose();
        }
        _stream = null;
        _tokenizer = null;
        _parser = null;
    }
    
    /// <summary>
    /// Gets the number of pages in the document
    /// </summary>
    public int PageCount
    {
        get
        {
            EnsureOpen();
            return _pageCount;
        }
    }
    
    /// <summary>
    /// Gets page information for the specified page
    /// </summary>
    public PageInfo GetPageInfo(int pageIndex)
    {
        EnsureOpen();
        
        if (pageIndex < 0 || pageIndex >= _pageCount)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        
        // Check cache
        if (_pageInfoCache.TryGetValue(pageIndex, out var cached))
            return cached;
        
        var info = new PageInfo
        {
            Offset = _pageTable![pageIndex]
        };
        
        // Parse page dictionary
        long offset = info.Offset;
        
        // Get MediaBox
        var mediaBox = _parser!.DictionaryLookup(offset, "MediaBox");
        if (mediaBox is PdfArray boxArray && boxArray.Count == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                if (boxArray[i] is PdfInteger intVal)
                    info.MediaBox[i] = intVal.Value;
                else if (boxArray[i] is PdfReal realVal)
                    info.MediaBox[i] = realVal.Value;
            }
        }
        
        // Get rotation
        var rotation = _parser.DictionaryLookup(offset, "Rotate");
        if (rotation is PdfInteger rotInt)
            info.Rotation = (int)(rotInt.Value % 360);
        
        // Get strip information from XObject resources
        var resources = _parser.DictionaryLookup(offset, "Resources");
        if (resources is PdfDictionary resDict)
        {
            var xobject = resDict["XObject"];
            if (xobject is PdfDictionary xobjDict)
            {
                ParseStripsFromXObjects(info, xobjDict, pageIndex);
            }
            else if (xobject is PdfReference xobjRef && 
                     _xref!.TryGetObjectOffset(xobjRef.ObjectNumber, out long xobjOffset))
            {
                var parsed = _parser.ParseObjectAtOffset(xobjOffset);
                if (parsed is PdfDictionary parsedDict)
                {
                    ParseStripsFromXObjects(info, parsedDict, pageIndex);
                }
            }
        }
        
        // Calculate DPI from MediaBox and pixel dimensions
        if (info.MediaBox[2] > 0 && info.MediaBox[3] > 0 && info.Width > 0 && info.Height > 0)
        {
            info.XDpi = info.Width * 72.0 / (info.MediaBox[2] - info.MediaBox[0]);
            info.YDpi = info.Height * 72.0 / (info.MediaBox[3] - info.MediaBox[1]);
        }
        
        _pageInfoCache[pageIndex] = info;
        return info;
    }
    
    /// <summary>
    /// Parse strips from XObject dictionary
    /// </summary>
    private void ParseStripsFromXObjects(PageInfo pageInfo, PdfDictionary xobjDict, int pageIndex)
    {
        var stripKeys = xobjDict.Keys
            .Where(k => k.StartsWith("strip"))
            .OrderBy(k => 
            {
                if (int.TryParse(k.AsSpan(5), out int n))
                    return n;
                return int.MaxValue;
            })
            .ToList();
        
        pageInfo.StripCount = stripKeys.Count;
        int totalHeight = 0;
        int maxStripSize = 0;
        int stripIndex = 0;
        
        foreach (var key in stripKeys)
        {
            var stripRef = xobjDict[key];
            if (stripRef is PdfReference sRef && _xref!.TryGetObjectOffset(sRef.ObjectNumber, out long stripOffset))
            {
                var stripInfo = ParseStripObject(stripOffset);
                if (stripInfo != null)
                {
                    // Cache strip info
                    _stripInfoCache[(pageIndex, stripIndex)] = stripInfo;
                    
                    // First strip determines page format
                    if (stripIndex == 0)
                    {
                        pageInfo.Width = stripInfo.Width;
                        pageInfo.Format = stripInfo.Format;
                        pageInfo.Colorspace = stripInfo.Colorspace;
                    }
                    
                    totalHeight += stripInfo.Height;
                    maxStripSize = Math.Max(maxStripSize, stripInfo.RawSize);
                    stripIndex++;
                }
            }
        }
        
        pageInfo.Height = totalHeight;
        pageInfo.MaxStripSize = maxStripSize;
    }
    
    /// <summary>
    /// Parse a strip image XObject
    /// </summary>
    private StripInfo? ParseStripObject(long offset)
    {
        var info = new StripInfo
        {
            Position = offset
        };
        
        // Skip object definition header
        long pos = offset;
        _tokenizer!.SkipWhitespace(ref pos);
        _tokenizer.TryParseULong(ref pos, out _); // object number
        _tokenizer.TryParseULong(ref pos, out _); // generation
        _tokenizer.TryEat(ref pos, "obj");
        
        // Parse the dictionary
        var dictValue = _parser!.ParseDictionaryOrStream(ref pos);
        if (dictValue is not PdfStream stream)
            return null;
        
        // Get width
        if (stream["Width"] is PdfInteger w)
            info.Width = (int)w.Value;
        
        // Get height
        if (stream["Height"] is PdfInteger h)
            info.Height = (int)h.Value;
        
        // Get bits per component
        int bpc = 8;
        if (stream["BitsPerComponent"] is PdfInteger bpcVal)
            bpc = (int)bpcVal.Value;
        
        // Get colorspace
        var colorspace = stream["ColorSpace"];
        info.Colorspace = ParseColorspace(colorspace);
        
        // Determine pixel format
        info.Format = DeterminePixelFormat(info.Colorspace, bpc);
        
        // Get compression from filter
        var filter = stream["Filter"];
        info.Compression = ParseCompression(filter);
        
        // Get stream data info
        info.RawSize = stream.Data.Length;
        
        // Store the actual data position in the file
        // This is calculated from the stream object position
        info.DataPosition = offset;
        
        return info;
    }
    
    /// <summary>
    /// Parse colorspace value
    /// </summary>
    private ColorspaceInfo ParseColorspace(PdfValue? colorspace)
    {
        var info = new ColorspaceInfo();
        
        if (colorspace is PdfName name)
        {
            info.Style = name.Value switch
            {
                "DeviceGray" => ColorspaceStyle.DeviceGray,
                "DeviceRGB" => ColorspaceStyle.DeviceRgb,
                "CalGray" => ColorspaceStyle.CalGray,
                "CalRGB" => ColorspaceStyle.CalRgb,
                _ => ColorspaceStyle.DeviceGray
            };
        }
        else if (colorspace is PdfArray array && array.Count > 0)
        {
            if (array[0] is PdfName csName)
            {
                info.Style = csName.Value switch
                {
                    "CalGray" => ColorspaceStyle.CalGray,
                    "CalRGB" => ColorspaceStyle.CalRgb,
                    "ICCBased" => ColorspaceStyle.IccBased,
                    _ => ColorspaceStyle.DeviceGray
                };
                
                // Parse colorspace parameters
                if (array.Count > 1 && array[1] is PdfDictionary csDict)
                {
                    ParseColorspaceParams(info, csDict);
                }
            }
        }
        
        return info;
    }
    
    /// <summary>
    /// Parse colorspace dictionary parameters
    /// </summary>
    private void ParseColorspaceParams(ColorspaceInfo info, PdfDictionary dict)
    {
        // White point
        if (dict["WhitePoint"] is PdfArray wp && wp.Count >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (wp[i] is PdfReal r)
                    info.WhitePoint[i] = r.Value;
                else if (wp[i] is PdfInteger n)
                    info.WhitePoint[i] = n.Value;
            }
        }
        
        // Black point
        if (dict["BlackPoint"] is PdfArray bp && bp.Count >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (bp[i] is PdfReal r)
                    info.BlackPoint[i] = r.Value;
                else if (bp[i] is PdfInteger n)
                    info.BlackPoint[i] = n.Value;
            }
        }
        
        // Gamma
        if (dict["Gamma"] is PdfReal g)
            info.Gamma = g.Value;
        else if (dict["Gamma"] is PdfInteger gi)
            info.Gamma = gi.Value;
    }
    
    /// <summary>
    /// Determine pixel format from colorspace and bits per component
    /// </summary>
    private static RasterPixelFormat DeterminePixelFormat(ColorspaceInfo colorspace, int bpc)
    {
        bool isGray = colorspace.Style is ColorspaceStyle.DeviceGray or ColorspaceStyle.CalGray;
        bool isRgb = colorspace.Style is ColorspaceStyle.DeviceRgb or ColorspaceStyle.CalRgb;
        
        return (isGray, bpc) switch
        {
            (true, 1) => RasterPixelFormat.Bitonal,
            (true, 8) => RasterPixelFormat.Gray8,
            (true, 16) => RasterPixelFormat.Gray16,
            (false, 8) when isRgb => RasterPixelFormat.Rgb24,
            (false, 16) when isRgb => RasterPixelFormat.Rgb48,
            _ => RasterPixelFormat.Gray8
        };
    }
    
    /// <summary>
    /// Parse compression from filter value
    /// </summary>
    private static RasterCompression ParseCompression(PdfValue? filter)
    {
        if (filter == null)
            return RasterCompression.Uncompressed;
        
        string filterName = filter switch
        {
            PdfName name => name.Value,
            PdfArray array when array.Count > 0 && array[0] is PdfName n => n.Value,
            _ => ""
        };
        
        return filterName switch
        {
            "DCTDecode" => RasterCompression.Jpeg,
            "CCITTFaxDecode" => RasterCompression.CcittGroup4,
            "FlateDecode" => RasterCompression.Flate,
            _ => RasterCompression.Uncompressed
        };
    }
    
    /// <summary>
    /// Gets strip information
    /// </summary>
    public StripInfo GetStripInfo(int pageIndex, int stripIndex)
    {
        EnsureOpen();
        
        // Ensure page is parsed first
        var pageInfo = GetPageInfo(pageIndex);
        if (stripIndex < 0 || stripIndex >= pageInfo.StripCount)
            throw new ArgumentOutOfRangeException(nameof(stripIndex));
        
        // Get from cache (populated during GetPageInfo)
        if (_stripInfoCache.TryGetValue((pageIndex, stripIndex), out var cached))
            return cached;
        
        throw new PdfRasterException($"Strip {stripIndex} info not found for page {pageIndex}");
    }
    
    /// <summary>
    /// Reads raw (compressed) strip data
    /// </summary>
    public byte[] ReadStripRaw(int pageIndex, int stripIndex)
    {
        EnsureOpen();
        
        var stripInfo = GetStripInfo(pageIndex, stripIndex);
        
        // Re-parse to get stream data
        long pos = stripInfo.Position;
        _tokenizer!.SkipWhitespace(ref pos);
        _tokenizer.TryParseULong(ref pos, out _);
        _tokenizer.TryParseULong(ref pos, out _);
        _tokenizer.TryEat(ref pos, "obj");
        
        var value = _parser!.ParseDictionaryOrStream(ref pos);
        if (value is PdfStream stream)
        {
            return stream.Data;
        }
        
        throw new PdfRasterException($"Failed to read strip {stripIndex} data from page {pageIndex}");
    }
    
    /// <summary>
    /// Reads and decodes strip data to raw pixels
    /// </summary>
    public byte[] ReadStripDecoded(int pageIndex, int stripIndex)
    {
        var stripInfo = GetStripInfo(pageIndex, stripIndex);
        var rawData = ReadStripRaw(pageIndex, stripIndex);
        
        int bpc = ImageDecoder.GetBitsPerComponent(stripInfo.Format);
        int components = ImageDecoder.GetComponents(stripInfo.Format);
        
        return ImageDecoder.Decode(rawData, stripInfo.Compression, 
            stripInfo.Width, stripInfo.Height, bpc, components);
    }
    
    /// <summary>
    /// Reads an entire page as raw decoded pixel data
    /// </summary>
    public byte[] ReadPagePixels(int pageIndex)
    {
        var pageInfo = GetPageInfo(pageIndex);
        
        int bpc = ImageDecoder.GetBitsPerComponent(pageInfo.Format);
        int components = ImageDecoder.GetComponents(pageInfo.Format);
        int totalSize = ImageDecoder.CalculateRawSize(pageInfo.Width, pageInfo.Height, bpc, components);
        
        var result = new byte[totalSize];
        int offset = 0;
        
        for (int i = 0; i < pageInfo.StripCount; i++)
        {
            var stripData = ReadStripDecoded(pageIndex, i);
            Array.Copy(stripData, 0, result, offset, stripData.Length);
            offset += stripData.Length;
        }
        
        return result;
    }
    
    /// <summary>
    /// Reads raw strip data
    /// </summary>
    [Obsolete("Use ReadStripRaw instead")]
    public int ReadStrip(int pageIndex, int stripIndex, byte[] buffer)
    {
        var data = ReadStripRaw(pageIndex, stripIndex);
        if (data.Length > buffer.Length)
            throw new ArgumentException("Buffer is too small for strip data", nameof(buffer));
        
        Array.Copy(data, buffer, data.Length);
        return data.Length;
    }
    
    /// <summary>
    /// Gets the pixel format for a page
    /// </summary>
    public RasterPixelFormat GetPageFormat(int pageIndex)
    {
        return GetPageInfo(pageIndex).Format;
    }
    
    /// <summary>
    /// Gets the width of a page in pixels
    /// </summary>
    public int GetPageWidth(int pageIndex)
    {
        return GetPageInfo(pageIndex).Width;
    }
    
    /// <summary>
    /// Gets the height of a page in pixels
    /// </summary>
    public int GetPageHeight(int pageIndex)
    {
        return GetPageInfo(pageIndex).Height;
    }
    
    /// <summary>
    /// Gets the horizontal DPI of a page
    /// </summary>
    public double GetPageXDpi(int pageIndex)
    {
        return GetPageInfo(pageIndex).XDpi;
    }
    
    /// <summary>
    /// Gets the vertical DPI of a page
    /// </summary>
    public double GetPageYDpi(int pageIndex)
    {
        return GetPageInfo(pageIndex).YDpi;
    }
    
    /// <summary>
    /// Gets the rotation of a page
    /// </summary>
    public int GetPageRotation(int pageIndex)
    {
        return GetPageInfo(pageIndex).Rotation;
    }
    
    /// <summary>
    /// Gets the number of strips on a page
    /// </summary>
    public int GetStripCount(int pageIndex)
    {
        return GetPageInfo(pageIndex).StripCount;
    }
    
    /// <summary>
    /// Gets the maximum strip size on a page
    /// </summary>
    public int GetMaxStripSize(int pageIndex)
    {
        return GetPageInfo(pageIndex).MaxStripSize;
    }
    
    /// <summary>
    /// Check if a stream appears to be a valid PDF/raster file
    /// </summary>
    public static bool Recognize(Stream stream, out int majorVersion, out int minorVersion)
    {
        majorVersion = -1;
        minorVersion = -1;
        
        if (!stream.CanRead || !stream.CanSeek)
            return false;
        
        // Read header
        var header = new byte[32];
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Read(header, 0, 32) < 8)
            return false;
        
        // Check for %PDF-
        if (header[0] != '%' || header[1] != 'P' || header[2] != 'D' || header[3] != 'F' || header[4] != '-')
            return false;
        
        // Read tail
        var tail = new byte[PdfRasterConstants.TailSize + 1];
        long tailStart = Math.Max(0, stream.Length - PdfRasterConstants.TailSize);
        stream.Seek(tailStart, SeekOrigin.Begin);
        int tailLen = stream.Read(tail, 0, PdfRasterConstants.TailSize);
        
        // Look for %%EOF
        string tailStr = System.Text.Encoding.ASCII.GetString(tail, 0, tailLen);
        if (!tailStr.Contains("%%EOF"))
            return false;
        
        // Look for %PDF-raster-
        int tagIndex = tailStr.LastIndexOf("%PDF-raster-");
        if (tagIndex < 0)
            return false;
        
        // Parse version
        int versionStart = tagIndex + 12;
        if (versionStart >= tailLen)
            return false;
        
        // Parse major.minor
        int i = versionStart;
        int major = 0;
        while (i < tailLen && char.IsDigit((char)tail[i]))
        {
            major = major * 10 + (tail[i] - '0');
            i++;
        }
        
        if (i >= tailLen || tail[i] != '.')
            return false;
        i++;
        
        int minor = 0;
        while (i < tailLen && char.IsDigit((char)tail[i]))
        {
            minor = minor * 10 + (tail[i] - '0');
            i++;
        }
        
        majorVersion = major;
        minorVersion = minor;
        
        return major >= 1 && major <= PdfRasterConstants.MaxSupportedMajorVersion;
    }
    
    private void EnsureOpen()
    {
        if (!_isOpen)
            throw new PdfApiException("Reader is not open");
    }
    
    private bool ParseTrailer()
    {
        if (_tokenizer == null || _stream == null)
            return false;
        
        // Read tail to find startxref and %%EOF
        var tail = new byte[PdfRasterConstants.TailSize + 1];
        long tailStart = Math.Max(0, _stream.Length - PdfRasterConstants.TailSize);
        _stream.Seek(tailStart, SeekOrigin.Begin);
        int tailLen = _stream.Read(tail, 0, PdfRasterConstants.TailSize);
        string tailStr = System.Text.Encoding.ASCII.GetString(tail, 0, tailLen);
        
        // Find %%EOF
        if (!tailStr.Contains("%%EOF"))
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.FileEofMarker, tailStart, "%%EOF not found");
            return false;
        }
        
        // Find startxref
        int startxrefIndex = tailStr.LastIndexOf("startxref");
        if (startxrefIndex < 0)
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.FileStartxref, tailStart, "startxref not found");
            return false;
        }
        
        // Parse startxref offset
        long xrefOffset = ParseStartXref(tailStr, startxrefIndex + 9);
        if (xrefOffset < 0)
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.FileBadStartxref, tailStart, "Invalid startxref value");
            return false;
        }
        
        // PDF-raster tag is written BEFORE the xref table, so we need to read 
        // backwards from xrefOffset to find it
        if (!FindAndParsePdfRasterTag(xrefOffset))
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.FilePdfrasterTag, xrefOffset, "PDF-raster tag not found");
            return false;
        }
        
        // Read xref table
        if (!ReadXrefTable(xrefOffset))
            return false;
        
        // Find and parse trailer dictionary
        long offset = xrefOffset;
        
        // Skip xref table
        _tokenizer.SkipWhitespace(ref offset);
        _tokenizer.TryEat(ref offset, "xref");
        
        // Skip header and entries
        _tokenizer.TryParseULong(ref offset, out _);
        _tokenizer.TryParseULong(ref offset, out ulong numEntries);
        offset += (long)numEntries * 20;
        
        // Find trailer
        if (!_tokenizer.TryEat(ref offset, "trailer"))
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.Trailer, offset, "trailer keyword not found");
            return false;
        }
        
        _trailerPosition = offset;
        
        // Get Root (catalog)
        var root = _parser!.DictionaryLookup(offset, "Root");
        if (root is PdfReference rootRef)
        {
            if (_xref!.TryGetObjectOffset(rootRef.ObjectNumber, out _catalogPosition))
            {
                // Parse page tree
                return ParsePageTree();
            }
        }
        
        ReportError(ErrorLevel.Compliance, ReadErrorCode.Root, offset, "Root entry not found");
        return false;
    }
    
    /// <summary>
    /// Find and parse PDF-raster tag which appears before the xref table
    /// </summary>
    private bool FindAndParsePdfRasterTag(long xrefOffset)
    {
        if (_stream == null)
            return false;
        
        // Read a chunk before xrefOffset to find the PDF-raster tag
        // The tag is typically on the line immediately before xref
        const int searchSize = 256;
        long searchStart = Math.Max(0, xrefOffset - searchSize);
        int bytesToRead = (int)(xrefOffset - searchStart);
        
        var buffer = new byte[bytesToRead];
        _stream.Seek(searchStart, SeekOrigin.Begin);
        int bytesRead = _stream.Read(buffer, 0, bytesToRead);
        
        string searchStr = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
        
        // Look for %PDF-raster-
        int tagIndex = searchStr.LastIndexOf("%PDF-raster-");
        if (tagIndex < 0)
            return false;
        
        // Parse version
        ParsePdfRasterVersion(searchStr, tagIndex + 12);
        return true;
    }
    
    private void ParsePdfRasterVersion(string tail, int offset)
    {
        int i = offset;
        _majorVersion = 0;
        while (i < tail.Length && char.IsDigit(tail[i]))
        {
            _majorVersion = _majorVersion * 10 + (tail[i] - '0');
            i++;
        }
        if (i < tail.Length && tail[i] == '.')
        {
            i++;
            _minorVersion = 0;
            while (i < tail.Length && char.IsDigit(tail[i]))
            {
                _minorVersion = _minorVersion * 10 + (tail[i] - '0');
                i++;
            }
        }
    }
    
    private long ParseStartXref(string tail, int offset)
    {
        // Skip whitespace
        while (offset < tail.Length && char.IsWhiteSpace(tail[offset]))
            offset++;
        
        long value = 0;
        while (offset < tail.Length && char.IsDigit(tail[offset]))
        {
            value = value * 10 + (tail[offset] - '0');
            offset++;
        }
        return value;
    }
    
    private bool ReadXrefTable(long offset)
    {
        if (_tokenizer == null || _stream == null)
            return false;
        
        if (!_tokenizer.TryEat(ref offset, "xref"))
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.Xref, offset, "xref keyword not found");
            return false;
        }
        
        if (!_tokenizer.TryParseULong(ref offset, out ulong firstObj) ||
            !_tokenizer.TryParseULong(ref offset, out ulong numEntries))
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.XrefHeader, offset, "Invalid xref header");
            return false;
        }
        
        if (firstObj != 0)
        {
            ReportError(ErrorLevel.Compliance, ReadErrorCode.XrefObjectZero, offset, "First object must be 0");
            return false;
        }
        
        // Read entries
        int entrySize = 20;
        var buffer = new byte[entrySize * (int)numEntries];
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.Read(buffer, 0, buffer.Length);
        
        _xref = XrefTable.Parse(buffer, 0, (int)numEntries);
        return true;
    }
    
    private bool ParsePageTree()
    {
        if (_parser == null || _xref == null)
            return false;
        
        // Get Pages from catalog
        var pages = _parser.DictionaryLookup(_catalogPosition, "Pages");
        if (pages is PdfReference pagesRef && _xref.TryGetObjectOffset(pagesRef.ObjectNumber, out long pagesOffset))
        {
            // Get page count
            var count = _parser.DictionaryLookup(pagesOffset, "Count");
            if (count is PdfInteger countInt)
            {
                _pageCount = (int)countInt.Value;
                _pageTable = new long[_pageCount];
                
                // Walk the page tree
                int pageIndex = 0;
                WalkPageTree(pagesOffset, ref pageIndex);
                
                return pageIndex == _pageCount;
            }
        }
        
        ReportError(ErrorLevel.Compliance, ReadErrorCode.CatPages, _catalogPosition, "Pages not found in catalog");
        return false;
    }
    
    private void WalkPageTree(long nodeOffset, ref int pageIndex)
    {
        if (_parser == null || _xref == null || _pageTable == null)
            return;
        
        var type = _parser.DictionaryLookup(nodeOffset, "Type");
        if (type is PdfName typeName)
        {
            if (typeName.Value == "Page")
            {
                // Leaf node - it's a page
                if (pageIndex < _pageTable.Length)
                {
                    _pageTable[pageIndex] = nodeOffset;
                    pageIndex++;
                }
            }
            else if (typeName.Value == "Pages")
            {
                // Internal node - enumerate kids
                var kids = _parser.DictionaryLookup(nodeOffset, "Kids");
                if (kids is PdfArray kidsArray)
                {
                    foreach (var kid in kidsArray)
                    {
                        if (kid is PdfReference kidRef && 
                            _xref.TryGetObjectOffset(kidRef.ObjectNumber, out long kidOffset))
                        {
                            WalkPageTree(kidOffset, ref pageIndex);
                        }
                    }
                }
            }
        }
    }
    
    private void ReportError(ErrorLevel level, ReadErrorCode code, long offset, string message)
    {
        ErrorHandler?.Invoke(this, level, code, offset, message);
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Close();
        }
    }
}
