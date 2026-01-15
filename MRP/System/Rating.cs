using MRP.Repositories;

namespace MRP.System;

public sealed class Rating : Atom, IAtom, __IVerifiable
{
    private static RatingRepository _Repository = new();

    public Rating() : base(null)
    {
    }

    public Rating(Session session, MediaEntry media) : base(session)
    {
        _InternalID = Guid.NewGuid();
        MediaId = media.Id;
        UserName = session.UserName;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id => (Guid?)_InternalID ?? Guid.Empty;

    public Guid MediaId { get; internal set; }
    public string UserName { get; internal set; } = string.Empty;
    public int Stars { get; internal set; }
    public string? Comment { get; internal set; }
    public bool IsConfirmed { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    public void SetRating(int stars, string? comment)
    {
        if (stars < 1 || stars > 5)
            throw new ArgumentException("Stars must be between 1 and 5.");

        Stars = stars;
        Comment = comment;
        IsConfirmed = false;
    }

    public void Confirm()
    {
        _EnsureAdminOrOwner(UserName);
        IsConfirmed = true;
    }

    protected override IRepository _GetRepository() => _Repository;

    public override void Save()
    {
        _EnsureAdminOrOwner(UserName);
        base.Save();
    }

    public override void Delete()
    {
        _EnsureAdminOrOwner(UserName);
        base.Delete();
    }

    public static IEnumerable<Rating> For(MediaEntry media)
        => _Repository.For(media);
}
