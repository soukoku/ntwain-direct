// PDF signature reader

namespace NTwain.Sidecar.PdfRaster.Security;

/// <summary>
/// Helper for reading digital signatures from PDF
/// </summary>
public class PdfSignatureReader
{
    /// <summary>
    /// Check if a PDF dictionary represents a signature
    /// </summary>
    internal static bool IsSignatureDictionary(PdfPrimitives.PdfDictionary dict)
    {
        var type = dict["Type"];
        var filter = dict["Filter"];
        
        return type is PdfPrimitives.PdfName typeName && 
               typeName.Value == "Sig" &&
               filter is PdfPrimitives.PdfName filterName &&
               filterName.Value.StartsWith("Adobe.PPK");
    }

    /// <summary>
    /// Extract signature information from dictionary
    /// </summary>
    internal static DigitalSignatureInfo? ExtractSignatureInfo(PdfPrimitives.PdfDictionary dict)
    {
        if (!IsSignatureDictionary(dict))
            return null;
        
        var info = new DigitalSignatureInfo();
        
        // Extract name
        if (dict["Name"] is PdfPrimitives.PdfString nameStr)
            info.Name = nameStr.AsText();
        
        // Extract reason
        if (dict["Reason"] is PdfPrimitives.PdfString reasonStr)
            info.Reason = reasonStr.AsText();
        
        // Extract location
        if (dict["Location"] is PdfPrimitives.PdfString locationStr)
            info.Location = locationStr.AsText();
        
        // Extract contact info
        if (dict["ContactInfo"] is PdfPrimitives.PdfString contactStr)
            info.ContactInfo = contactStr.AsText();
        
        // Extract signing time
        if (dict["M"] is PdfPrimitives.PdfString timeStr)
            info.SigningTime = Utilities.PdfDateUtils.ParsePdfDateString(timeStr.AsText());
        
        // Extract byte range
        if (dict["ByteRange"] is PdfPrimitives.PdfArray byteRangeArray && byteRangeArray.Count == 4)
        {
            info.ByteRange = new long[4];
            for (int i = 0; i < 4; i++)
            {
                if (byteRangeArray[i] is PdfPrimitives.PdfInteger intVal)
                    info.ByteRange[i] = intVal.Value;
            }
        }
        
        // Extract contents (signature data)
        if (dict["Contents"] is PdfPrimitives.PdfString contentsStr)
            info.Contents = contentsStr.Data;
        
        return info;
    }
    
    /// <summary>
    /// Read and validate data specified by ByteRange
    /// </summary>
    public static byte[]? ReadSignedData(Stream stream, long[] byteRange)
    {
        if (byteRange.Length != 4)
            return null;
        
        try
        {
            // ByteRange format: [offset1, length1, offset2, length2]
            // We need to read data from offset1 for length1 bytes, 
            // then from offset2 for length2 bytes
            
            var data = new byte[byteRange[1] + byteRange[3]];
            
            // Read first range
            stream.Seek(byteRange[0], SeekOrigin.Begin);
            int read1 = stream.Read(data, 0, (int)byteRange[1]);
            if (read1 != byteRange[1])
                return null;
            
            // Read second range
            stream.Seek(byteRange[2], SeekOrigin.Begin);
            int read2 = stream.Read(data, (int)byteRange[1], (int)byteRange[3]);
            if (read2 != byteRange[3])
                return null;
            
            return data;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Validate a signature from a PDF
    /// </summary>
    public static bool ValidateSignature(Stream stream, DigitalSignatureInfo sigInfo)
    {
        if (sigInfo.Contents == null || sigInfo.ByteRange == null)
            return false;
        
        // Read the signed data
        var signedData = ReadSignedData(stream, sigInfo.ByteRange);
        if (signedData == null)
            return false;
        
        // Validate the signature
        return DigitalSignatureValidator.Validate(sigInfo.Contents, signedData);
    }
}
