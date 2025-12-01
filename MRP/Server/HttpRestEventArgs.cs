using MRP.System;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using MRP.Handlers;
using MRP.Server.Ext;

namespace MRP.Server;

public class HttpRestEventArgs : EventArgs
{
    public HttpRestEventArgs(HttpListenerContext context)
    {
        Context = context;

        Method = HttpMethod.Parse(context.Request.HttpMethod);
        Path = context.Request.Url?.AbsolutePath ?? string.Empty;
        Query = context.Request.ParseQuery();

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"\nReceived: {Method} {Path}");

        if (context.Request.HasEntityBody)
        {
            using Stream input = context.Request.InputStream;
            using StreamReader re = new(input, context.Request.ContentEncoding);
            Body = re.ReadToEnd();
            Content = JsonNode.Parse(Body)?.AsObject() ?? new JsonObject();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Body);
        }
        else
        {
            Body = string.Empty;
            Content = new JsonObject();
        }
    }

    public HttpListenerContext Context { get; }
    public HttpMethod Method { get; }
    public string Path { get; }
    public IReadOnlyDictionary<string, string?> Query { get; private set; }
    public string Body { get; }
    public JsonObject Content { get; }
    public bool Responded { get; set; }
    public Session? Session
    {
        get
        {
            string token = Context.Request.Headers["Authorization"] ?? string.Empty;
            if (token.ToLower().StartsWith("bearer "))
            {
                token = token[7..].Trim();
            }
            else { return null; }

            return Session.Get(token);
        }
    }

    public void Respond(HttpStatusCode code, JsonObject json)
    {
        var resp = Context.Response;
        var data = Encoding.UTF8.GetBytes(json.ToString());

        resp.StatusCode = (int)code;
        resp.ContentType = "application/json";
        resp.ContentLength64 = data.Length;

        resp.OutputStream.Write(data);
        resp.OutputStream.Close();

        Responded = true;
    }
}