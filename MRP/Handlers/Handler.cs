using System.Reflection;
using MRP.Server;

namespace MRP.Handlers;
public abstract class Handler : IHandler
{
    private static List<IHandler>? _handlers;

    private static List<IHandler> LoadHandlers()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IHandler)) && !t.IsAbstract)
            .Select(t => (IHandler)Activator.CreateInstance(t)!)
            .ToList();
    }

    public static void HandleEvent(object? sender, HttpRestEventArgs e)
    {
        _handlers ??= LoadHandlers();

        foreach (var handler in _handlers)
        {
            handler.Handle(e);
            if (e.Responded) return;
        }
    }

    public abstract void Handle(HttpRestEventArgs e);
}