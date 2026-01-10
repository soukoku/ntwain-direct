// Error codes for PDF/raster operations
// Ported from pdfrasread.h

namespace NTwain.Sidecar.PdfRaster;

/// <summary>
/// Error reporting levels
/// </summary>
public enum ErrorLevel
{
    /// <summary>Informational - useful to know but not bad news</summary>
    Info = 0,
    
    /// <summary>Warning - a potential problem but execution can continue</summary>
    Warning = 1,
    
    /// <summary>Compliance - a violation of the PDF/raster specification was detected</summary>
    Compliance = 2,
    
    /// <summary>API - bad parameter in call to this library</summary>
    Api = 3,
    
    /// <summary>Memory - memory allocation failed</summary>
    Memory = 4,
    
    /// <summary>IO - PDF read or write failed unexpectedly</summary>
    IO = 5,
    
    /// <summary>Limit - a built-in limitation of this library was exceeded</summary>
    Limit = 6,
    
    /// <summary>Internal - an 'impossible' internal state has been detected</summary>
    Internal = 7,
    
    /// <summary>Other - none of the above, and the current API call cannot complete</summary>
    Other = 8
}

/// <summary>
/// Detailed error codes for PDF/raster reader
/// </summary>
public enum ReadErrorCode
{
    Ok = 0,
    ApiBadReader,
    ApiApiLevel,
    ApiNullParam,
    ApiAlreadyOpen,
    ApiNotOpen,
    ApiNoSuchPage,
    ApiNoSuchStrip,
    StripBufferSize,
    InternalXrefSize,
    InternalXrefTable,
    MemoryMalloc,
    FileEofMarker,
    FileStartxref,
    FileBadStartxref,
    FilePdfrasterTag,
    FileTagSol,
    FileBadTag,
    FileTooMajor,
    FileTooMinor,
    LitstrEof,
    HexstrChar,
    Xref,
    XrefHeader,
    XrefObjectZero,
    XrefNumrefs,
    Trailer,
    TrailerDict,
    Root,
    CatType,
    CatPages,
    PagesCount,
    PageCounts,
    PageType,
    PageType2,
    PagesExtra,
    PageKids,
    PageKidsArray,
    PageKidsEnd,
    PageRotation,
    PageMediabox,
    StripRead,
    XrefTable,
    XrefEntry,
    XrefEntryZero,
    XrefGen0,
    ObjDef,
    NoSuchXref,
    GenZero,
    Dictionary,
    DictNameKey,
    DictObjstm,
    DictEof,
    DictValue,
    StreamCrlf,
    StreamLinebreak,
    StreamLength,
    StreamLengthInt,
    StreamEndstream,
    ObjectEof,
    Stream,
    Object,
    MediaboxArray,
    MediaboxElements,
    Resources,
    Xobject,
    XobjectDict,
    XobjectEntry,
    StripRef,
    StripDict,
    StripMissing,
    StripTypeXobject,
    StripSubtype,
    StripBitspercomponent,
    StripCsBpc,
    StripHeight,
    StripWidth,
    StripWidthSame,
    StripFormatSame,
    StripColorspaceSame,
    StripDepthSame,
    StripColorspace,
    StripLength,
    ValidColorspace,
    CalgrayDict,
    GammaNumber,
    Gamma22,
    CalrgbDict,
    Matrix,
    MatrixElement,
    MatrixTooLong,
    MatrixTooShort,
    Whitepoint,
    Blackpoint,
    IccProfile,
    IccprofileRead,
    ColorspaceArray,
    FieldsNotInAf,
    VNotInField,
    ByterangeNotFound,
    ContentsInDsNotFound,
    BadStringBegin,
    EncryptFilterNotFound,
    BadNameBegin,
    BadBooleanValue,
    BadEncryptDictionary,
    NoDocumentId,
    ArrayBadSyntax,
    EncryptionBadPassword,
    EncryptionBadSecurityType,
    EncryptionNoRecipients
}
