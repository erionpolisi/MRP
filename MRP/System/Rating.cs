namespace MRP.System;

public class Rating
{
    public Rating(User user, int stars, string? comment = null)
    {
        User = user;
        SetStars(stars);
        SetComment(comment);
    }

    public Guid Id { get; set; }

    public User User { get; set; }

    public int Stars { get; private set; }

    public string? Comment { get; private set; } = string.Empty;

    public bool IsConfirmed { get; private set; }

    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public HashSet<string> LikedByUsernames { get; set; } = new();

    public void SetStars(int stars)
    {
        if (stars < 1 || stars > 5)
            throw new ArgumentException("Stars must be between 1 and 5.");
        Stars = stars;
        IsConfirmed = false;
    }

    public void SetComment(string? comment)
    {
        Comment = comment;
        IsConfirmed = false;
    }

    public void Confirm()
    {
        IsConfirmed = true;
    }
}