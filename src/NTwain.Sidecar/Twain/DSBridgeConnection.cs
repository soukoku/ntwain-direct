using DSBridge;
using NTwain.Sidecar.Dtos;
using SimpleIpc;

namespace NTwain.Sidecar.Twain;

internal class DSBridgeConnection : IDisposable
{
    private bool disposedValue;
    IpcParentConnection _connection;

    private DSBridgeConnection(IpcParentConnection connection)
    {
        _connection = connection;
        _connection.Disconnected += _connection_Disconnected;
    }

    private void _connection_Disconnected(object? sender, EventArgs e)
    {
        // bleh
    }

    public static async Task<DSBridgeConnection> CreateAsync(bool is64Bit = false)
    {
        var exe = is64Bit ? "DSBridge64.exe" : "DSBridge32.exe";
        var connection = await IpcParentConnection.StartChildAsync(exe);
        return new DSBridgeConnection(connection);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _connection.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal async Task<IEnumerable<ScannerInfo>> GetScannersAsync()
    {
        var request = new DSRequest
        {
            Category = "internal",
            Command = "GetSources"
        };
        await _connection.SendAsync(request);
        var resp = await _connection.ReadAsync<DSResponse>();
        if (resp != null && resp.Scanners != null) return resp.Scanners;
        return [];
    }
}
