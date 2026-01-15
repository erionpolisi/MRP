using MRP.Repositories;

namespace MRP.System;

public sealed class MediaEntry : Atom, IAtom, __IVerifiable
{
    private static MediaEntryRepository _Repository = new();

    public MediaEntry() : base(null)
    {
    }

    public MediaEntry(Session session) : base(session)
    {
        _InternalID = Guid.NewGuid();
        Creator = session.UserName;
    }

    public Guid Id => (Guid?)_InternalID ?? Guid.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public int AgeRestriction { get; set; }
    public List<string> Genres { get; set; } = new();

    public string Creator { get; internal set; } = string.Empty;

    protected override IRepository _GetRepository() => _Repository;

    public override void Save()
    {
        _EnsureAdminOrOwner(Creator);
        base.Save();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(Creator);
        base.Delete();
    }

    public enum MediaType
    {
        Movie = 1,
        Series = 2,
        Game = 3
    }
}
