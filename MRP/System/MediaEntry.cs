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
        CreatorId = session.UserId;
        CreatorUserName = session.UserName;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id => (Guid?)_InternalID ?? Guid.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public int AgeRestriction { get; set; }
    public List<string> Genres { get; set; } = new();
    public double AverageScore { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    public Guid CreatorId { get; internal set; }
    public string CreatorUserName { get; internal set; } = string.Empty;

    public enum MediaType
    {
        Unknown = 0,
        Movie = 1,
        Series = 2,
        Game = 3
    }

    protected override IRepository _GetRepository() => _Repository;

    public static IEnumerable<MediaEntry> All(Session? session = null)
    {
        return _Repository.GetAll(session);
    }

    public override void Save()
    {
        _EnsureAdminOrOwner(CreatorUserName);
        base.Save();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(CreatorUserName);
        base.Delete();
    }


    public static MediaEntry? Get(Guid id, Session? session = null)
        => _Repository.Get(id, session);
}
