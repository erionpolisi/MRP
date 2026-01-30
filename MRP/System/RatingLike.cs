using MRP.Repositories;
using MRP.System;

public sealed class RatingLike : Atom, IAtom, __IVerifiable
{
    private static RatingLikeRepository _Repository = new();

    public RatingLike() : base(null) { }

    public RatingLike(Session session, Guid ratingId) : base(session)
    {
        RatingId = ratingId;
        UserName = session.UserName;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid RatingId { get; internal set; }
    public string UserName { get; internal set; } = "";
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

    public static RatingLike? Get(Guid ratingId, string userName)
        => _Repository.Get((ratingId, userName));
}