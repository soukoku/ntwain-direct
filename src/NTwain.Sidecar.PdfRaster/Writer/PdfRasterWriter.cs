// PDF/raster writer
// Ported from PdfRaster.c

using System.Security.Cryptography;
using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Writer;

/// <summary>
/// PDF/raster file writer/encoder
/// </summary>
public class PdfRasterWriter : IDisposable
{
    private Stream? _stream;
    private bool _ownsStream;
    private bool _disposed;
    
    private XrefWriter _xref;
    private PdfOutputStream? _output;
    
    // Document objects
    private PdfReference? _catalogRef;
    private PdfReference? _pagesRef;
    private PdfReference? _infoRef;
    private PdfDictionary _catalog;
    private PdfDictionary _pages;
    private PdfDictionary _info;
    private PdfDictionary _trailer;
    private PdfArray _pageRefs;
    private PdfArray _fileId;
    
    // Current page state
    private PdfReference? _currentPageRef;
    private PdfDictionary? _currentPage;
    private PdfDictionary? _currentResources;
    private PdfDictionary? _currentXObjects;
    private int _currentPageWidth;
    private int _currentPageHeight;
    private int _currentStripCount;
    private List<PdfReference> _currentStrips = new();
    private List<int> _currentStripHeights = new();
    
    // Page settings (apply to next page)
    private double _xdpi = PdfRasterConstants.DefaultXDpi;
    private double _ydpi = PdfRasterConstants.DefaultYDpi;
    private int _rotation;
    private RasterPixelFormat _pixelFormat = RasterPixelFormat.Bitonal;
    private RasterCompression _compression = RasterCompression.Uncompressed;
    private bool _bitonalUncalibrated;
    private bool _calibrateGray = true;
    private bool _calibrateRgb = true;
    
    // Document metadata
    private DateTime _creationDate;
    private int _pageCount;
    
    // Colorspace and ICC profile
    private PdfValue? _rgbColorspace;
    private PdfReference? _grayIccProfileRef;
    private PdfReference? _rgbIccProfileRef;
    private byte[]? _grayIccProfile;
    private byte[]? _rgbIccProfile;
    
    /// <summary>
    /// Gets the current page count
    /// </summary>
    public int PageCount => _pageCount + (_currentPage != null ? 1 : 0);
    
    /// <summary>
    /// Gets the number of bytes written
    /// </summary>
    public long BytesWritten => _output?.Position ?? 0;
    
    /// <summary>
    /// Creates a new PDF/raster writer
    /// </summary>
    public PdfRasterWriter()
    {
        _xref = new XrefWriter();
        _catalog = new PdfDictionary();
        _pages = new PdfDictionary();
        _info = new PdfDictionary();
        _trailer = new PdfDictionary();
        _pageRefs = new PdfArray();
        _fileId = new PdfArray();
    }
    
    /// <summary>
    /// Begin writing to a stream
    /// </summary>
    /// <param name="stream">A writable stream</param>
    /// <param name="leaveOpen">If true, the stream will not be disposed when End() is called</param>
    public void Begin(Stream stream, bool leaveOpen = false)
    {
        if (_output != null)
            throw new PdfApiException("Writer is already active");
        
        ArgumentNullException.ThrowIfNull(stream);
        
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable", nameof(stream));
        
        _stream = stream;
        _ownsStream = !leaveOpen;
        _output = new PdfOutputStream(_stream, _xref);
        _creationDate = DateTime.Now;
        
        InitializeDocument();
        
        _output.WriteHeader(PdfRasterConstants.PdfVersion);
    }
    
    /// <summary>
    /// Begin writing to a file
    /// </summary>
    public void Begin(string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        Begin(stream, leaveOpen: false);
    }
    
    /// <summary>
    /// Begin writing to a memory stream
    /// </summary>
    /// <returns>The memory stream being written to</returns>
    public MemoryStream BeginToMemory()
    {
        var stream = new MemoryStream();
        Begin(stream, leaveOpen: true);
        return stream;
    }
    
    private void InitializeDocument()
    {
        // Create catalog
        _catalogRef = _xref.CreateReference();
        _catalog[PdfNames.Type] = PdfNames.Catalog;
        
        // Create pages tree root
        _pagesRef = _xref.CreateReference();
        _pages[PdfNames.Type] = PdfNames.Pages;
        _pages[PdfNames.Count] = new PdfInteger(0);
        _pages[PdfNames.Kids] = _pageRefs;
        _catalog[PdfNames.Pages] = _pagesRef;
        
        // Create info dictionary
        _infoRef = _xref.CreateReference();
        _info["Producer"] = new PdfString($"NTwain.Sidecar.PdfRaster {PdfRasterConstants.LibraryVersion}");
        _info["CreationDate"] = new PdfString(FormatPdfDate(_creationDate));
        
        // Generate file ID
        var id = GenerateFileId();
        _fileId.Add(new PdfString(id, true));
        _fileId.Add(new PdfString(id, true));
        
        // Set up trailer
        _trailer[PdfNames.Root] = _catalogRef;
        _trailer[PdfNames.Info] = _infoRef;
        _trailer[PdfNames.ID] = _fileId;
    }
    
    /// <summary>
    /// Set document creator
    /// </summary>
    public void SetCreator(string creator)
    {
        _info["Creator"] = new PdfString(creator);
    }
    
    /// <summary>
    /// Set document author
    /// </summary>
    public void SetAuthor(string author)
    {
        _info["Author"] = new PdfString(author);
    }
    
    /// <summary>
    /// Set document title
    /// </summary>
    public void SetTitle(string title)
    {
        _info["Title"] = new PdfString(title);
    }
    
    /// <summary>
    /// Set document subject
    /// </summary>
    public void SetSubject(string subject)
    {
        _info["Subject"] = new PdfString(subject);
    }
    
    /// <summary>
    /// Set document keywords
    /// </summary>
    public void SetKeywords(string keywords)
    {
        _info["Keywords"] = new PdfString(keywords);
    }
    
    /// <summary>
    /// Set resolution for subsequent pages
    /// </summary>
    public void SetResolution(double xdpi, double ydpi)
    {
        _xdpi = xdpi;
        _ydpi = ydpi;
    }
    
    /// <summary>
    /// Set rotation for subsequent pages
    /// </summary>
    public void SetRotation(int degrees)
    {
        while (degrees < 0) degrees += 360;
        degrees %= 360;
        if (degrees % 90 == 0)
            _rotation = degrees;
    }
    
    /// <summary>
    /// Set pixel format for subsequent pages
    /// </summary>
    public void SetPixelFormat(RasterPixelFormat format)
    {
        _pixelFormat = format;
    }
    
    /// <summary>
    /// Set compression for subsequent pages
    /// </summary>
    public void SetCompression(RasterCompression compression)
    {
        _compression = compression;
    }
    
    /// <summary>
    /// Set whether to use uncalibrated DeviceGray for bitonal images
    /// </summary>
    public bool SetBitonalUncalibrated(bool uncal)
    {
        var previous = _bitonalUncalibrated;
        _bitonalUncalibrated = uncal;
        return previous;
    }
    
    /// <summary>
    /// Set a grayscale ICC profile for subsequent pages
    /// </summary>
    /// <param name="profileData">ICC profile data</param>
    public void SetGrayIccProfile(byte[] profileData)
    {
        _grayIccProfile = profileData;
        _grayIccProfileRef = null; // Will be created when needed
    }
    
    /// <summary>
    /// Set an RGB ICC profile for subsequent pages
    /// </summary>
    /// <param name="profileData">ICC profile data</param>
    public void SetRgbIccProfile(byte[] profileData)
    {
        _rgbIccProfile = profileData;
        _rgbIccProfileRef = null; // Will be created when needed
        _rgbColorspace = null; // Force rebuild
    }
    
    /// <summary>
    /// Load ICC profile from file
    /// </summary>
    public static byte[] LoadIccProfile(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }
    
    /// <summary>
    /// Start a new page
    /// </summary>
    public void StartPage(int width)
    {
        EnsureActive();
        
        // End current page if any
        if (_currentPage != null)
            EndPage();
        
        _currentPageWidth = width;
        _currentPageHeight = 0;
        _currentStripCount = 0;
        _currentStrips.Clear();
        _currentStripHeights.Clear();
        
        // Create page dictionary
        _currentPage = new PdfDictionary();
        _currentPageRef = _xref.CreateReference(_currentPage);
        
        _currentPage[PdfNames.Type] = PdfNames.Page;
        _currentPage[PdfNames.Parent] = _pagesRef;
        
        // Set initial media box (will be updated in EndPage)
        double w = _currentPageWidth / _xdpi * 72;
        _currentPage[PdfNames.MediaBox] = PdfArray.FromDoubles(0, 0, w, 0);
        
        // Setup resources
        _currentResources = new PdfDictionary();
        _currentXObjects = new PdfDictionary();
        _currentResources[PdfNames.XObject] = _currentXObjects;
        _currentPage[PdfNames.Resources] = _currentResources;
        
        // Rotation if specified
        if (_rotation != 0)
        {
            _currentPage[PdfNames.Rotate] = new PdfInteger(_rotation);
        }
    }
    
    /// <summary>
    /// Write a strip of image data
    /// </summary>
    public void WriteStrip(int rows, byte[] data)
    {
        WriteStrip(rows, data, 0, data.Length);
    }
    
    /// <summary>
    /// Write a strip of image data
    /// </summary>
    public void WriteStrip(int rows, byte[] data, int offset, int count)
    {
        if (_currentPage == null)
            throw new PdfApiException("No page is open");
        
        // Prepare data (compress if needed)
        byte[] streamData;
        if (_compression == RasterCompression.Flate)
        {
            streamData = RasterUtilities.CompressFlate(data, offset, count);
        }
        else
        {
            streamData = new byte[count];
            Array.Copy(data, offset, streamData, 0, count);
        }
        
        // Create image XObject stream
        var stream = new PdfStream(streamData);
        
        // Set up image dictionary
        stream[PdfNames.Type] = PdfNames.XObject;
        stream[PdfNames.Subtype] = PdfNames.Image;
        stream[PdfNames.Width] = new PdfInteger(_currentPageWidth);
        stream[PdfNames.Height] = new PdfInteger(rows);
        stream[PdfNames.BitsPerComponent] = new PdfInteger(GetBitsPerComponent());
        stream[PdfNames.ColorSpace] = GetColorspace();
        
        // Add filter and decode params if compressed
        var filter = GetFilter();
        if (filter != null)
        {
            stream[PdfNames.Filter] = filter;
            
            // Add decode parameters for CCITT
            if (_compression == RasterCompression.CcittGroup4)
            {
                var decodeParms = new PdfDictionary();
                decodeParms[PdfNames.K] = new PdfInteger(-1); // Group 4
                decodeParms[PdfNames.Columns] = new PdfInteger(_currentPageWidth);
                decodeParms[PdfNames.Rows] = new PdfInteger(rows);
                decodeParms[PdfNames.BlackIs1] = PdfBoolean.True;
                stream[PdfNames.DecodeParms] = decodeParms;
            }
        }
        
        // Add to xref and write
        var imageRef = _xref.CreateReference(stream);
        _output!.WriteReferenceDefinition(imageRef);
        
        // Add to page resources
        _currentXObjects![$"strip{_currentStripCount}"] = imageRef;
        _currentStrips.Add(imageRef);
        _currentStripHeights.Add(rows);
        
        _currentPageHeight += rows;
        _currentStripCount++;
    }
    
    /// <summary>
    /// End the current page
    /// </summary>
    public void EndPage()
    {
        if (_currentPage == null)
            return;
        
        // Update media box with final height
        double w = _currentPageWidth / _xdpi * 72.0;
        double h = _currentPageHeight / _ydpi * 72.0;
        _currentPage[PdfNames.MediaBox] = PdfArray.FromDoubles(0, 0, w, h);
        
        // Generate page contents
        var contentsStream = CreateContentsStream();
        var contentsRef = _xref.CreateReference(contentsStream);
        _currentPage[PdfNames.Contents] = contentsRef;
        
        // Write contents stream
        _output!.WriteReferenceDefinition(contentsRef);
        
        // Write page object
        _xref.SetValue(_currentPageRef!.ObjectNumber, _currentPage);
        _output.WriteReferenceDefinition(_currentPageRef);
        
        // Add page to pages array
        _pageRefs.Add(_currentPageRef);
        _pageCount++;
        
        // Clear current page
        _currentPage = null;
        _currentPageRef = null;
        _currentResources = null;
        _currentXObjects = null;
    }
    
    /// <summary>
    /// End the document
    /// </summary>
    public void End()
    {
        EnsureActive();
        
        // End current page if any
        EndPage();
        
        // Update page count in pages dictionary
        _pages[PdfNames.Count] = new PdfInteger(_pageCount);
        
        // Write pages
        _xref.SetValue(_pagesRef!.ObjectNumber, _pages);
        _output!.WriteReferenceDefinition(_pagesRef);
        
        // Write catalog
        _xref.SetValue(_catalogRef!.ObjectNumber, _catalog);
        _output.WriteReferenceDefinition(_catalogRef);
        
        // Write info
        _xref.SetValue(_infoRef!.ObjectNumber, _info);
        _output.WriteReferenceDefinition(_infoRef);
        
        // Write PDF-raster signature
        _output.WritePdfRasterSignature();
        
        // Write xref table
        long xrefOffset = _output.WriteXrefTable();
        
        // Update trailer size
        _trailer[PdfNames.Size] = new PdfInteger(_xref.Count);
        
        // Write trailer
        _output.WriteTrailer(_trailer, xrefOffset);
        
        _output.Flush();
        
        // Clean up
        if (_ownsStream)
        {
            _stream?.Dispose();
        }
        _stream = null;
        _output = null;
    }
    
    private int GetBitsPerComponent() => RasterUtilities.GetBitsPerComponent(_pixelFormat);
    
    private PdfValue GetColorspace()
    {
        // Check if ICC profile should be used
        bool isGray = _pixelFormat is RasterPixelFormat.Bitonal or RasterPixelFormat.Gray8 or RasterPixelFormat.Gray16;
        bool isRgb = _pixelFormat is RasterPixelFormat.Rgb24 or RasterPixelFormat.Rgb48;
        
        if (isGray && _grayIccProfile != null)
        {
            return BuildIccBasedColorspace(_grayIccProfile, 1, ref _grayIccProfileRef);
        }
        
        if (isRgb && _rgbIccProfile != null)
        {
            return BuildIccBasedColorspace(_rgbIccProfile, 3, ref _rgbIccProfileRef);
        }
        
        // Fall back to Cal or Device colorspaces
        return _pixelFormat switch
        {
            RasterPixelFormat.Bitonal => _bitonalUncalibrated
                ? PdfNames.DeviceGray
                : (_calibrateGray ? BuildCalGrayColorspace() : PdfNames.DeviceGray),
            RasterPixelFormat.Gray8 or RasterPixelFormat.Gray16 => _calibrateGray
                ? BuildCalGrayColorspace()
                : PdfNames.DeviceGray,
            RasterPixelFormat.Rgb24 or RasterPixelFormat.Rgb48 => _calibrateRgb 
                ? BuildCalRgbColorspace()
                : PdfNames.DeviceRGB,
            _ => PdfNames.DeviceGray
        };
    }
    
    private PdfValue BuildIccBasedColorspace(byte[] profileData, int components, ref PdfReference? profileRef)
    {
        // Create ICC profile stream if not already created
        if (profileRef == null)
        {
            var profileStream = new PdfStream(profileData);
            profileStream[PdfNames.N] = new PdfInteger(components);
            
            // Set alternate colorspace
            if (components == 1)
                profileStream[PdfNames.Alternate] = PdfNames.DeviceGray;
            else if (components == 3)
                profileStream[PdfNames.Alternate] = PdfNames.DeviceRGB;
            
            profileRef = _xref.CreateReference(profileStream);
            _output!.WriteReferenceDefinition(profileRef);
        }
        
        // ICCBased colorspace: [ /ICCBased stream-ref ]
        var array = new PdfArray();
        array.Add(PdfNames.ICCBased);
        array.Add(profileRef);
        
        return array;
    }
    
    private PdfValue BuildCalGrayColorspace()
    {
        // CalGray colorspace with D65 white point
        var dict = new PdfDictionary();
        dict["WhitePoint"] = PdfArray.FromDoubles(0.9505, 1.0, 1.089);
        dict["Gamma"] = new PdfReal(2.2);
        
        var array = new PdfArray();
        array.Add(PdfNames.CalGray);
        array.Add(dict);
        
        return array;
    }
    
    private PdfValue BuildCalRgbColorspace()
    {
        if (_rgbColorspace != null)
            return _rgbColorspace;
        
        // Default to CalRGB with sRGB-like parameters
        var dict = new PdfDictionary();
        dict["WhitePoint"] = PdfArray.FromDoubles(0.9505, 1.0, 1.089);
        dict["Gamma"] = PdfArray.FromDoubles(2.2, 2.2, 2.2);
        
        var array = new PdfArray();
        array.Add(PdfNames.CalRGB);
        array.Add(dict);
        
        _rgbColorspace = array;
        return array;
    }
    
    private PdfValue? GetFilter()
    {
        return _compression switch
        {
            RasterCompression.Jpeg => PdfNames.DCTDecode,
            RasterCompression.CcittGroup4 => PdfNames.CCITTFaxDecode,
            RasterCompression.Flate => PdfNames.FlateDecode,
            _ => null
        };
    }
    
    private PdfStream CreateContentsStream()
    {
        var content = new System.Text.StringBuilder();
        
        double w = _currentPageWidth / _xdpi * 72.0;
        double totalH = _currentPageHeight / _ydpi * 72.0;
        double y = totalH;
        
        // Draw each strip from top to bottom using actual strip heights
        for (int i = 0; i < _currentStrips.Count; i++)
        {
            double stripH = _currentStripHeights[i] / _ydpi * 72.0;
            
            content.AppendLine("q");
            content.AppendLine($"{w:0.######} 0 0 {stripH:0.######} 0 {y - stripH:0.######} cm");
            content.AppendLine($"/strip{i} Do");
            content.AppendLine("Q");
            
            y -= stripH;
        }
        
        var data = System.Text.Encoding.ASCII.GetBytes(content.ToString());
        return new PdfStream(data);
    }
    
    private byte[] GenerateFileId()
    {
        // Generate a unique file ID based on creation time and random data
        using var md5 = MD5.Create();
        var input = System.Text.Encoding.UTF8.GetBytes(
            $"{_creationDate:O}{Guid.NewGuid()}"
        );
        return md5.ComputeHash(input);
    }
    
    private static string FormatPdfDate(DateTime dt)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(dt);
        char sign = offset >= TimeSpan.Zero ? '+' : '-';
        var absOffset = offset.Duration();
        return $"D:{dt:yyyyMMddHHmmss}{sign}{absOffset.Hours:D2}'{absOffset.Minutes:D2}'";
    }
    
    private void EnsureActive()
    {
        if (_output == null)
            throw new PdfApiException("Writer is not active");
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_ownsStream)
            {
                _stream?.Dispose();
            }
        }
    }
}
