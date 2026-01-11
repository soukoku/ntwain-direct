using DSBridge;
using NTwain.Data;
using NTwain.Sidecar.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NTwain.Sidecar.Twain;

/// <summary>
/// Utility class to enumerate both 32/64-bit TWAIN sources.
/// </summary>
static class SourceEnumerator
{
    public static async Task<IEnumerable<ScannerInfo>> GetAllSourcesAsync()
    {
        var tasks = await Task.WhenAll(Get32BitSourcesAsync(), Get64BitSourcesAsync());
        return tasks.SelectMany(t => t);
    }

    // this is done by starting up separate DSBridge processes for each architecture

    async static Task<IEnumerable<ScannerInfo>> Get32BitSourcesAsync()
    {
        using var bridge = await DSBridgeConnection.CreateAsync(is64Bit: false);
        return await bridge.GetScannersAsync();
    }

    async static Task<IEnumerable<ScannerInfo>> Get64BitSourcesAsync()
    {
        using var bridge = await DSBridgeConnection.CreateAsync(is64Bit: true);
        return await bridge.GetScannersAsync();
    }
}
