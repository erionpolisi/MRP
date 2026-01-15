using MRP.Repositories;
using MRP.System;

public sealed class MediaFavorite : Atom, IAtom, __IVerifiable
{
    private static MediaFavoriteRepository _Repository = new();

    public MediaFavorite() : base(null) { }

    public MediaFavorite(Session session, MediaEntry media) : base(session)
    {
        UserName = session.UserName;
        MediaId = media.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public string UserName { get; internal set; } = "";
    public Guid MediaId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }


    protected override IRepository _GetRepository() => _Repository;

    public override void Save()
    {
        _VerifySession();
        base.Save();
    }

    public override void Delete()
    {
        _VerifySession();
        base.Delete();
    }

    public static MediaFavorite? Get(string userName, Guid mediaId)
    {
        return _Repository.Get((userName, mediaId));
    }

}