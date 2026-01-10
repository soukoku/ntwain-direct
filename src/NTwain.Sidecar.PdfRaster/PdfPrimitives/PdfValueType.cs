// PDF value types

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// Types of PDF values
/// </summary>
public enum PdfValueType
{
    Null,
    Error,
    Boolean,
    Integer,
    Real,
    Name,
    String,
    Array,
    Dictionary,
    Stream,
    Reference,
    Comment,
    Keyword
}
