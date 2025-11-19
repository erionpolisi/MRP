using MRP.Server;

namespace MRP.Handlers;

public interface IHandler
{ 
    public void Handle(HttpRestEventArgs e);
}