using NTwain;
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

                //if (request.Command == "hello")
                //{
                //    var response = new DSResponse();
                //    await connection.SendAsync(response);
                //    break;
                //}
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
}
