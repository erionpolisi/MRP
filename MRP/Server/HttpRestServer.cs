using System.Net;

namespace MRP.Server;

public class HttpRestServer
{
    private readonly HttpListener _listener = new();

    public event EventHandler<HttpRestEventArgs>? RequestReceived;
    public bool Running { get; private set; }

    public HttpRestServer(int port)
    {
        _listener.Prefixes.Add($"http://+:{port}/");
    }

    public void Run()
    {
        _listener.Start();
        Running = true;
        Console.WriteLine("[Server] Running...");

        while (Running)
        {
            var ctx = _listener.GetContext();

            _ = Task.Run(() =>
            {
                var args = new HttpRestEventArgs(ctx);
                RequestReceived?.Invoke(this, args);

                if (!args.Responded)
                    args.Respond(HttpStatusCode.NotFound,
                        new() { ["success"] = false, ["msg"] = "Not found" });
            });
        }
    }

    public void Stop()
    {
        Running = false;
        _listener.Close();
    }
}