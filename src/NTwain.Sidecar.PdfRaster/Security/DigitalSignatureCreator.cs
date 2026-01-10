// Digital signature creator

using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NTwain.Sidecar.PdfRaster.PdfPrimitives;

namespace NTwain.Sidecar.PdfRaster.Security;

/// <summary>
/// Digital signature creator for PDF documents
/// </summary>
internal class DigitalSignatureCreator
{
    private readonly X509Certificate2 _certificate;
    private readonly DigitalSignatureInfo _info;
    
    /// <summary>
    /// Create a new signature creator with a certificate
    /// </summary>
    public DigitalSignatureCreator(X509Certificate2 certificate, DigitalSignatureInfo? info = null)
    {
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        _info = info ?? new DigitalSignatureInfo();
        
        if (!_certificate.HasPrivateKey)
            throw new ArgumentException("Certificate must have a private key", nameof(certificate));
    }
    
    /// <summary>
    /// Load certificate from PFX/PKCS#12 file
    /// </summary>
    public static X509Certificate2 LoadCertificate(string pfxPath, string password)
    {
        return new X509Certificate2(pfxPath, password, 
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
    }
    
    /// <summary>
    /// Sign data and return PKCS#7 signature
    /// </summary>
    public byte[] Sign(byte[] data)
    {
        // Create content info from data
        var contentInfo = new ContentInfo(data);
        
        // Create signed CMS
        var cms = new SignedCms(contentInfo, true); // detached = true
        
        // Create signer
        var signer = new CmsSigner(_certificate)
        {
            IncludeOption = X509IncludeOption.WholeChain
        };
        
        // Add signing time attribute
        var signingTime = new Pkcs9SigningTime(DateTime.Now);
        signer.SignedAttributes.Add(signingTime);
        
        // Sign the data
        cms.ComputeSignature(signer);
        
        // Return encoded signature
        return cms.Encode();
    }
    
    /// <summary>
    /// Create signature dictionary for PDF
    /// </summary>
    public PdfDictionary CreateSignatureDictionary(byte[] signature)
    {
        var dict = new PdfDictionary();
        
        dict["Type"] = new PdfName("Sig");
        dict["Filter"] = new PdfName("Adobe.PPKLite");
        dict["SubFilter"] = new PdfName("adbe.pkcs7.detached");
        dict["Contents"] = new PdfString(signature, true);
        
        // Add optional fields
        if (!string.IsNullOrEmpty(_info.Name))
            dict["Name"] = new PdfString(_info.Name);
        
        if (!string.IsNullOrEmpty(_info.Reason))
            dict["Reason"] = new PdfString(_info.Reason);
        
        if (!string.IsNullOrEmpty(_info.Location))
            dict["Location"] = new PdfString(_info.Location);
        
        if (!string.IsNullOrEmpty(_info.ContactInfo))
            dict["ContactInfo"] = new PdfString(_info.ContactInfo);
        
        // Add signing time
        var signingTime = _info.SigningTime ?? DateTime.Now;
        dict["M"] = new PdfString(Utilities.PdfDateUtils.ToPdfDateString(signingTime));
        
        return dict;
    }
    
    /// <summary>
    /// Gets the certificate used for signing
    /// </summary>
    public X509Certificate2 Certificate => _certificate;
    
    /// <summary>
    /// Gets or sets the signature information
    /// </summary>
    public DigitalSignatureInfo Info => _info;
}
