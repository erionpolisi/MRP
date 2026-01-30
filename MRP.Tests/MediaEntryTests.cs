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
}