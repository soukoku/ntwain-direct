using NTwain.Data;
using NTwain.Sidecar.Dtos;

namespace DSBridge;

public record DSResponse
{
    public required string Command { get; init; }


    /// <summary>
    /// Main result code from TWAIN operation.
    /// </summary>
    public STS STS { get; set; }


    public IList<ScannerInfo>? Scanners { get; set; }
}
