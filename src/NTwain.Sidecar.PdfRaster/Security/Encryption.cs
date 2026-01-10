// Encryption support interfaces and implementations
// Ported from pdfras_encryption

using System.Security.Cryptography;

namespace NTwain.Sidecar.PdfRaster.Security;

/// <summary>
/// Interface for PDF encryption/decryption
/// </summary>
public interface IEncrypter
{
    /// <summary>
    /// Gets the encryption algorithm used
    /// </summary>
    EncryptionAlgorithm Algorithm { get; }
    
    /// <summary>
    /// Gets whether metadata is encrypted
    /// </summary>
    bool EncryptMetadata { get; }
    
    /// <summary>
    /// Set the current object number and generation for encryption context
    /// </summary>
    void SetObjectContext(int objectNumber, int generation);
    
    /// <summary>
    /// Encrypt data
    /// </summary>
    byte[] Encrypt(byte[] data);
    
    /// <summary>
    /// Decrypt data
    /// </summary>
    byte[] Decrypt(byte[] data);
}

/// <summary>
/// Interface for PDF decryption
/// </summary>
public interface IDecrypter
{
    /// <summary>
    /// Gets the encryption algorithm used
    /// </summary>
    EncryptionAlgorithm Algorithm { get; }
    
    /// <summary>
    /// Gets whether metadata is encrypted
    /// </summary>
    bool MetadataEncrypted { get; }
    
    /// <summary>
    /// Set the current object number and generation for decryption context
    /// </summary>
    void SetObjectContext(int objectNumber, int generation);
    
    /// <summary>
    /// Decrypt data
    /// </summary>
    byte[] Decrypt(byte[] data);
    
    /// <summary>
    /// Validate password and get access level
    /// </summary>
    DocumentAccess ValidatePassword(string password);
}

/// <summary>
/// Encryption data from the PDF Encrypt dictionary
/// </summary>
public class EncryptionData
{
    public EncryptionAlgorithm Algorithm { get; set; }
    public int Version { get; set; }  // V value
    public int Revision { get; set; } // R value
    public int KeyLength { get; set; }
    public bool EncryptMetadata { get; set; } = true;
    public DocumentPermissions Permissions { get; set; }
    
    // Standard security handler data
    public byte[]? O { get; set; }  // Owner password hash
    public byte[]? U { get; set; }  // User password hash
    public byte[]? OE { get; set; } // Owner encryption key (R=6)
    public byte[]? UE { get; set; } // User encryption key (R=6)
    public byte[]? Perms { get; set; } // Permissions validation (R=6)
    
    // Document ID
    public byte[]? DocumentId { get; set; }
}

/// <summary>
/// RC4 encryption implementation
/// </summary>
internal class Rc4Crypter
{
    private readonly byte[] _state = new byte[256];
    
    public Rc4Crypter(byte[] key)
    {
        Initialize(key);
    }
    
    private void Initialize(byte[] key)
    {
        for (int i = 0; i < 256; i++)
            _state[i] = (byte)i;
        
        int j = 0;
        for (int i = 0; i < 256; i++)
        {
            j = (j + _state[i] + key[i % key.Length]) & 0xFF;
            (_state[i], _state[j]) = (_state[j], _state[i]);
        }
    }
    
    public byte[] Process(byte[] data)
    {
        var state = (byte[])_state.Clone();
        var result = new byte[data.Length];
        
        int i = 0, j = 0;
        for (int k = 0; k < data.Length; k++)
        {
            i = (i + 1) & 0xFF;
            j = (j + state[i]) & 0xFF;
            (state[i], state[j]) = (state[j], state[i]);
            result[k] = (byte)(data[k] ^ state[(state[i] + state[j]) & 0xFF]);
        }
        
        return result;
    }
}

/// <summary>
/// Standard security handler encryption
/// </summary>
public class StandardEncrypter : IEncrypter
{
    private readonly byte[] _encryptionKey;
    private readonly EncryptionAlgorithm _algorithm;
    private readonly bool _encryptMetadata;
    private byte[]? _objectKey;
    
    public EncryptionAlgorithm Algorithm => _algorithm;
    public bool EncryptMetadata => _encryptMetadata;
    
    public StandardEncrypter(string userPassword, string ownerPassword, 
        DocumentPermissions permissions, EncryptionAlgorithm algorithm, 
        bool encryptMetadata, byte[] documentId)
    {
        _algorithm = algorithm;
        _encryptMetadata = encryptMetadata;
        
        int keyLength = algorithm switch
        {
            EncryptionAlgorithm.Rc4_40 => 5,
            EncryptionAlgorithm.Rc4_128 => 16,
            EncryptionAlgorithm.Aes128 => 16,
            EncryptionAlgorithm.Aes256 => 32,
            _ => 16
        };
        
        _encryptionKey = ComputeEncryptionKey(userPassword, ownerPassword, 
            (uint)permissions, keyLength, documentId, encryptMetadata);
    }
    
    public void SetObjectContext(int objectNumber, int generation)
    {
        // Compute object-specific key
        using var md5 = MD5.Create();
        
        var input = new byte[_encryptionKey.Length + 5];
        Array.Copy(_encryptionKey, input, _encryptionKey.Length);
        input[_encryptionKey.Length] = (byte)(objectNumber & 0xFF);
        input[_encryptionKey.Length + 1] = (byte)((objectNumber >> 8) & 0xFF);
        input[_encryptionKey.Length + 2] = (byte)((objectNumber >> 16) & 0xFF);
        input[_encryptionKey.Length + 3] = (byte)(generation & 0xFF);
        input[_encryptionKey.Length + 4] = (byte)((generation >> 8) & 0xFF);
        
        if (_algorithm == EncryptionAlgorithm.Aes128 || _algorithm == EncryptionAlgorithm.Aes256)
        {
            // Add "sAlT" for AES
            var aesInput = new byte[input.Length + 4];
            Array.Copy(input, aesInput, input.Length);
            aesInput[input.Length] = (byte)'s';
            aesInput[input.Length + 1] = (byte)'A';
            aesInput[input.Length + 2] = (byte)'l';
            aesInput[input.Length + 3] = (byte)'T';
            input = aesInput;
        }
        
        var hash = md5.ComputeHash(input);
        
        // Key length is min(n + 5, 16) for RC4
        int keyLen = Math.Min(_encryptionKey.Length + 5, 16);
        _objectKey = new byte[keyLen];
        Array.Copy(hash, _objectKey, keyLen);
    }
    
    public byte[] Encrypt(byte[] data)
    {
        if (_objectKey == null)
            throw new InvalidOperationException("Object context not set");
        
        if (_algorithm == EncryptionAlgorithm.Rc4_40 || _algorithm == EncryptionAlgorithm.Rc4_128)
        {
            var rc4 = new Rc4Crypter(_objectKey);
            return rc4.Process(data);
        }
        else
        {
            return EncryptAes(data);
        }
    }
    
    public byte[] Decrypt(byte[] data)
    {
        if (_objectKey == null)
            throw new InvalidOperationException("Object context not set");
        
        // RC4 is symmetric
        if (_algorithm == EncryptionAlgorithm.Rc4_40 || _algorithm == EncryptionAlgorithm.Rc4_128)
        {
            var rc4 = new Rc4Crypter(_objectKey);
            return rc4.Process(data);
        }
        else
        {
            return DecryptAes(data);
        }
    }
    
    private byte[] EncryptAes(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _objectKey!;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        // Generate random IV
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
        
        // Prepend IV
        var result = new byte[aes.IV.Length + encrypted.Length];
        Array.Copy(aes.IV, result, aes.IV.Length);
        Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        
        return result;
    }
    
    private byte[] DecryptAes(byte[] data)
    {
        if (data.Length < 16)
            return data; // Too short to have IV
        
        using var aes = Aes.Create();
        aes.Key = _objectKey!;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        // Extract IV from first 16 bytes
        var iv = new byte[16];
        Array.Copy(data, iv, 16);
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
    }
    
    private static byte[] ComputeEncryptionKey(string userPassword, string ownerPassword,
        uint permissions, int keyLength, byte[] documentId, bool encryptMetadata)
    {
        // Simplified key computation (full implementation would follow PDF spec)
        using var md5 = MD5.Create();
        
        // Pad password to 32 bytes
        var paddedPassword = PadPassword(userPassword);
        
        // Compute O value
        var oValue = ComputeOwnerKey(ownerPassword, keyLength);
        
        // Compute encryption key
        var input = new List<byte>();
        input.AddRange(paddedPassword);
        input.AddRange(oValue);
        input.Add((byte)(permissions & 0xFF));
        input.Add((byte)((permissions >> 8) & 0xFF));
        input.Add((byte)((permissions >> 16) & 0xFF));
        input.Add((byte)((permissions >> 24) & 0xFF));
        input.AddRange(documentId);
        
        if (!encryptMetadata)
        {
            input.Add(0xFF);
            input.Add(0xFF);
            input.Add(0xFF);
            input.Add(0xFF);
        }
        
        var hash = md5.ComputeHash(input.ToArray());
        
        // For 128-bit keys, hash 50 more times
        if (keyLength > 5)
        {
            for (int i = 0; i < 50; i++)
            {
                hash = md5.ComputeHash(hash, 0, keyLength);
            }
        }
        
        var key = new byte[keyLength];
        Array.Copy(hash, key, keyLength);
        return key;
    }
    
    private static readonly byte[] PasswordPadding = 
    {
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
        0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
        0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A
    };
    
    private static byte[] PadPassword(string password)
    {
        var bytes = System.Text.Encoding.Latin1.GetBytes(password ?? "");
        var padded = new byte[32];
        int len = Math.Min(bytes.Length, 32);
        Array.Copy(bytes, padded, len);
        Array.Copy(PasswordPadding, 0, padded, len, 32 - len);
        return padded;
    }
    
    private static byte[] ComputeOwnerKey(string ownerPassword, int keyLength)
    {
        using var md5 = MD5.Create();
        var paddedOwner = PadPassword(ownerPassword);
        var hash = md5.ComputeHash(paddedOwner);
        
        if (keyLength > 5)
        {
            for (int i = 0; i < 50; i++)
            {
                hash = md5.ComputeHash(hash);
            }
        }
        
        var key = new byte[keyLength];
        Array.Copy(hash, key, keyLength);
        
        // Encrypt padded user password with RC4
        var rc4 = new Rc4Crypter(key);
        return rc4.Process(paddedOwner);
    }
}
