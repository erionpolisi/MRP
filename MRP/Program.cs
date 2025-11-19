using MRP.Server;
using MRP.Handlers;

namespace MRP;

internal static class Program
{
    static void Main(string[] args)
    {
        var server = new HttpRestServer(8080);
        server.RequestReceived += Handler.HandleEvent;
        server.Run();
    }
}