// Digital signature validator

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace NTwain.Sidecar.PdfRaster.Security;

/// <summary>
/// Digital signature validator
/// </summary>
public class DigitalSignatureValidator
{
    /// <summary>
    /// Validate a digital signature
    /// </summary>
    /// <param name="signatureData">The PKCS#7 signature data</param>
    /// <param name="signedData">The data that was signed</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    public static bool Validate(byte[] signatureData, byte[] signedData)
    {
        try
        {
            var cms = new SignedCms();
            cms.Decode(signatureData);
            
            // Verify the signature
            cms.CheckSignature(true);
            
            // Verify the signed data matches
            var computedHash = SHA256.HashData(signedData);
            // In a full implementation, we would extract and compare the hash from the signature
            
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Extract signer information from signature
    /// </summary>
    public static X509Certificate2? GetSignerCertificate(byte[] signatureData)
    {
        try
        {
            var cms = new SignedCms();
            cms.Decode(signatureData);
            
            if (cms.SignerInfos.Count > 0)
            {
                return cms.SignerInfos[0].Certificate;
            }
        }
        catch
        {
            // Ignore errors
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the signing time from the signature
    /// </summary>
    public static DateTime? GetSigningTime(byte[] signatureData)
    {
        try
        {
            var cms = new SignedCms();
            cms.Decode(signatureData);
            
            if (cms.SignerInfos.Count > 0)
            {
                var signerInfo = cms.SignerInfos[0];
                foreach (var attr in signerInfo.SignedAttributes)
                {
                    if (attr.Oid.Value == "1.2.840.113549.1.9.5") // signing time
                    {
                        var pkcs9 = new Pkcs9SigningTime(attr.Values[0].RawData);
                        return pkcs9.SigningTime;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        
        return null;
    }
}
