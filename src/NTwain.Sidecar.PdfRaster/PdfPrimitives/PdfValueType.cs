// PDF value type enumeration

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// Type of PDF value
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
    Reference
}
