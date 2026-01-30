using MRP.Repositories;
using MRP.System;

public sealed class MediaFavorite : Atom, IAtom, __IVerifiable
{
    private static MediaFavoriteRepository _Repository = new();

    public MediaFavorite() : base(null) { }

    public MediaFavorite(Session session, MediaEntry media) : base(session)
    {
        CreatorUserName = session.UserName;
        MediaId = media.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public string CreatorUserName { get; internal set; } = "";
    public Guid MediaId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public MediaEntry Media { get; internal set; }


    protected override IRepository _GetRepository() => _Repository;

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

    public static MediaFavorite? Get(string userName, Guid mediaId)
    {
        return _Repository.Get((userName, mediaId));
    }

    public MediaEntry GetMedia()
    {
        return Media ??= MediaEntry.Get(MediaId);
    }

    public static IEnumerable<MediaFavorite> ForUser(string userName)
        => _Repository.ForUser(userName);


}