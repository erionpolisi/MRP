using MRP.System;
using Xunit;

public class MediaEntryTests
{
    [Fact]
    public void MediaEntry_HasEmptyTitleByDefault()
    {
        var media = new MediaEntry();
        Assert.Equal(string.Empty, media.Title);
    }

    [Fact]
    public void MediaEntry_DefaultAverageScoreIsZero()
    {
        var media = new MediaEntry();
        Assert.Equal(0, media.AverageScore);
    }

    [Fact]
    public void MediaEntry_GenresCanBeAdded()
    {
        var media = new MediaEntry();
        media.Genres.Add("Action");
        Assert.Single(media.Genres);
    }

    [Fact]
    public void MediaEntry_TypeCanBeSet()
    {
        var media = new MediaEntry();
        media.Type = MediaEntry.MediaType.Movie;
        Assert.Equal(MediaEntry.MediaType.Movie, media.Type);
    }

    [Fact]
    public void MediaEntry_GenresMaintainInsertionOrder()
    {
        var media = new MediaEntry();

        media.Genres.Add("Action");
        media.Genres.Add("Drama");
        media.Genres.Add("Sci-Fi");

        Assert.Equal(3, media.Genres.Count);
        Assert.Equal("Action", media.Genres[0]);
        Assert.Equal("Drama", media.Genres[1]);
        Assert.Equal("Sci-Fi", media.Genres[2]);
    }
}