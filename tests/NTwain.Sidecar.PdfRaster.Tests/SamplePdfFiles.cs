using NTwain.Sidecar.PdfRaster;
using NTwain.Sidecar.PdfRaster.Reader;
using Xunit;

namespace NTwain.Sidecar.PdfRaster.Tests;

/// <summary>
/// Provides paths to sample PDF files for testing.
/// </summary>
public static class SamplePdfFiles
{
    private static readonly string SamplePdfsFolder = Path.Combine(AppContext.BaseDirectory, "SamplePdfs");

    /// <summary>
    /// 1-bit black and white with CCITT Group 4 compression.
    /// </summary>
    public static string Bw1Ccitt => Path.Combine(SamplePdfsFolder, "BW1 CCITT.pdf");

    /// <summary>
    /// 1-bit black and white uncompressed.
    /// </summary>
    public static string Bw1Uncompressed => Path.Combine(SamplePdfsFolder, "BW1 Uncompressed.pdf");

    /// <summary>
    /// 16-bit grayscale uncompressed.
    /// </summary>
    public static string Gray16Uncompressed => Path.Combine(SamplePdfsFolder, "Gray16 Uncompressed.pdf");

    /// <summary>
    /// 8-bit grayscale with calibrated color space.
    /// </summary>
    public static string Gray8Calibrated => Path.Combine(SamplePdfsFolder, "Gray8 Calibrated.pdf");

    /// <summary>
    /// 8-bit grayscale with device color space.
    /// </summary>
    public static string Gray8Device => Path.Combine(SamplePdfsFolder, "Gray8 Device.pdf");

    /// <summary>
    /// 8-bit grayscale uncompressed.
    /// </summary>
    public static string Gray8Uncompressed => Path.Combine(SamplePdfsFolder, "Gray8 Uncompressed.pdf");

    /// <summary>
    /// 24-bit RGB uncompressed.
    /// </summary>
    public static string Rgb24Uncompressed => Path.Combine(SamplePdfsFolder, "RGB24 Uncompressed.pdf");

    /// <summary>
    /// 24-bit RGB (compressed).
    /// </summary>
    public static string Rgb24 => Path.Combine(SamplePdfsFolder, "RGB24.pdf");

    /// <summary>
    /// Gets all sample PDF file paths.
    /// </summary>
    public static IEnumerable<string> All
    {
        get
        {
            yield return Bw1Ccitt;
            yield return Bw1Uncompressed;
            yield return Gray16Uncompressed;
            yield return Gray8Calibrated;
            yield return Gray8Device;
            yield return Gray8Uncompressed;
            yield return Rgb24Uncompressed;
            yield return Rgb24;
        }
    }

    /// <summary>
    /// Gets sample PDF paths as xUnit theory data.
    /// </summary>
    public static TheoryData<string> AllAsTheoryData
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var path in All)
            {
                data.Add(path);
            }
            return data;
        }
    }

    /// <summary>
    /// Gets sample PDF paths that exist as xUnit theory data.
    /// </summary>
    public static TheoryData<string> AllExistingAsTheoryData
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var path in All)
            {
                if (File.Exists(path))
                {
                    data.Add(path);
                }
            }
            return data;
        }
    }
}
