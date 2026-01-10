using NTwain.Data;

namespace DSBridge;

public record DSResponse
{
    /// <summary>
    /// Main result code from TWAIN operation.
    /// </summary>
    public STS STS { get; set; }
}
