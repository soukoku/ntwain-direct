using NTwain;
using NTwain.Data;
using NTwain.Sidecar.Dtos;
using SimpleIpc;

namespace DSBridge;

internal class Program
{
    static async Task Main(string[] args)
    {
        await using var connection = await IpcChildConnection.CreateAndWaitForConnectionAsync(args);
        using var twain = new TwainAppSession();
        // Subscribe to disconnection event
        connection.Disconnected += (sender, e) =>
        {
            Console.Error.WriteLine("Parent disconnected! Shutting down...");
        };
        try
        {
            // Use DisconnectedToken to automatically cancel operations when parent exits
            while (!connection.DisconnectedToken.IsCancellationRequested)
            {
                twain.OpenDsm();

                var request = await connection.ReadAsync<DSRequest>();

                if (request is null)
                {
                    // Connection closed normally
                    break;
                }

                if (request.Category == "internal")
                {
                    await HandleInternalCommandAsync(connection, twain, request);
                }
            }
        }
        catch (OperationCanceledException) when (connection.DisconnectedToken.IsCancellationRequested)
        {
            // Parent exited, exit gracefully
        }
        finally
        {
            twain.CloseDsm();
        }
        Console.Error.WriteLine("DSBridge exiting.");
    }

    private static async Task HandleInternalCommandAsync(IpcChildConnection connection, TwainAppSession twain, DSRequest request)
    {
        switch (request.Command)
        {
            case "GetSources":
                var sources = twain.GetSources();
                var scannerInfos = new List<ScannerInfo>();
                foreach (var source in sources)
                {
                    scannerInfos.Add(new ScannerInfo
                    {
                        Id = $"twain{(Environment.Is64BitProcess ? "64" : "32")}-{source.Id}",
                        Name = source.ProductName,
                        Manufacturer = source.Manufacturer,
                        DriverVersion = $"{source.Version.MajorNum}.{source.Version.MinorNum}",
                        ConnectionType = "usb",
                        Model = source.ProductFamily,
                        TwainVersion = $"{source.ProtocolMajor}.{source.ProtocolMinor}",
                        Online = true // todo: figure it out
                    });
                }
                var response = new DSResponse
                {
                    Category = request.Category,
                    Command = request.Command,
                    STS = new STS() { RC = TWRC.SUCCESS },
                    Scanners = scannerInfos
                };
                await connection.SendAsync(response);
                break;
        }
    }
}
