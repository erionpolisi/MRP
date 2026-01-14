using MRP.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace MRP.System;

public sealed class User : Atom, IAtom, __IVerifiable, __IAuthentificable
{
    private static UserRepository _Repository = new();


    public User(Session? session)
    {
        _EditingSession = session;
    }

    public User() : this(null)
    { }


    public static User? Get(string userName, Session? session = null)
    {
        return _Repository.Get(userName, session);
    }


    protected override IRepository _GetRepository()
    {
        return _Repository;
    }


    public bool IsAdmin { get; set; } = false;

    public string UserName
    {
        get { return ((string?)((__IVerifiable)this).__InternalID) ?? string.Empty; }
        set
        {
            if (_InternalID is not null)
            {
                throw new InvalidOperationException("User name cannot be changed.");
            }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("User name must not be empty."); }

            ((__IAuthentificable)this).__Username = value;
            ((__IAuthentificable)this).__PasswordHash = null;
        }
    }

    internal static string? _HashPassword(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(password)) { return null; }

        StringBuilder rval = new();
        foreach (byte i in SHA256.HashData(Encoding.UTF8.GetBytes(userName + password)))
        {
            rval.Append(i.ToString("x2"));
        }
        return rval.ToString();
    }

    public string FullName
    {
        get; set;
    } = string.Empty;


    public string EMail
    {
        get; set;
    } = string.Empty;


    public void SetPassword(string password)
    {
        ((__IAuthentificable)this).__PasswordHash = _HashPassword(UserName, password);
    }

    public override void Save()
    {
        if (IsAdmin) { _EnsureAdmin(); }
        base.Save();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(UserName);
        base.Delete();
    }


    string? __IAuthentificable.__Username
    {
        get; set;
    }

    string? __IAuthentificable.__PasswordHash
    {
        get; set;
    }
}
