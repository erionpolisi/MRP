using MRP.System;
using Xunit;

public class RatingTests
{
    [Fact]
    public void Rating_DefaultStarsIsZero()
    {
        var rating = new Rating();
        Assert.Equal(0, rating.Stars);
    }

    [Fact]
    public void SetRating_SetsStarsCorrectly()
    {
        var rating = new Rating();
        rating.SetRating(5, null);
        Assert.Equal(5, rating.Stars);
    }

    [Fact]
    public void SetRating_SetsComment()
    {
        var rating = new Rating();
        rating.SetRating(4, "good");
        Assert.Equal("good", rating.Comment);
    }

    [Fact]
    public void SetRating_ResetsConfirmation()
    {
        var rating = new Rating();
        rating.SetRating(3, null);
        Assert.False(rating.IsConfirmed);
    }
}