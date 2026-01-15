using MRP.Repositories;

namespace MRP.System;

public abstract class Atom : IAtom, __IVerifiable
{
    protected Session? _EditingSession = null;

    protected object? _InternalID;

    public Atom(Session? session)
    {
        _EditingSession = session;
    }

    protected abstract IRepository _GetRepository();

    protected void _VerifySession(Session? session = null)
    {
        if (session is not null)
            _EditingSession = session;

        if (_EditingSession is null)
            throw new UnauthorizedAccessException("Invalid session.");
    }


    protected void _EndEdit()
    {
        _EditingSession = null;
    }

    protected void _EnsureAdmin()
    {
        if (!(_EditingSession?.IsAdmin ?? false))
        {
            throw new UnauthorizedAccessException("Admin privileges required.");
        }
    }
    
    protected void _EnsureAdminOrOwner(string? owner)
    {
        ((__IVerifiable)this).__VerifySession();
        if (!(_EditingSession!.IsAdmin || (_EditingSession.UserName == owner)))
        {
            throw new UnauthorizedAccessException("Admin or owner privileges required.");
        }
    }
    
    object? __IVerifiable.__InternalID
    {
        get { return _InternalID; }
        set { _InternalID = value; }
    }

    void __IVerifiable.__VerifySession(Session? session)
    {
        _VerifySession(session);
    }
    
    void __IVerifiable.__EndEdit()
    {
        _EndEdit();
    }

    void __IVerifiable.__EnsureAdmin()
    {
        _EnsureAdmin();
    }

    void __IVerifiable.__EnsureAdminOrOwner(string? owner)
    {
        _EnsureAdminOrOwner(owner);
    }

    public virtual void BeginEdit(Session session)
    {
        _VerifySession(session);
    }

    public virtual void Save()
    {
        _GetRepository().Save(this);
        _EndEdit();
    }

    public virtual void Delete()
    {
        _GetRepository().Delete(this);
        _EndEdit();
    }

    public virtual void Refresh()
    {
        _GetRepository().Refresh(this);
        _EndEdit();
    }
}
