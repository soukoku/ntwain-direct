// PDF value base class

namespace NTwain.Sidecar.PdfRaster.PdfPrimitives;

/// <summary>
/// Base class for all PDF values
/// </summary>
internal abstract class PdfValue
{
    public abstract PdfValueType Type { get; }
    
    public virtual bool IsNull => Type == PdfValueType.Null;
    public virtual bool IsError => Type == PdfValueType.Error;
    public virtual bool IsBoolean => Type == PdfValueType.Boolean;
    public virtual bool IsInteger => Type == PdfValueType.Integer;
    public virtual bool IsReal => Type == PdfValueType.Real;
    public virtual bool IsNumber => IsInteger || IsReal;
    public virtual bool IsName => Type == PdfValueType.Name;
    public virtual bool IsString => Type == PdfValueType.String;
    public virtual bool IsArray => Type == PdfValueType.Array;
    public virtual bool IsDictionary => Type == PdfValueType.Dictionary;
    public virtual bool IsStream => Type == PdfValueType.Stream;
    public virtual bool IsReference => Type == PdfValueType.Reference;
    public virtual bool IsComment => Type == PdfValueType.Comment;
    
    public static PdfNull Null { get; } = new PdfNull();
    public static PdfError Error { get; } = new PdfError();
    
    public abstract void WriteTo(System.IO.TextWriter writer);
    
    public override string ToString()
    {
        using var writer = new System.IO.StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
