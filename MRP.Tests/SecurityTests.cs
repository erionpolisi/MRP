using MRP.System;
using System;
using Xunit;

public class SecurityTests
{
    [Fact]
    public void User_DefaultIsNotAdmin()
    {
        var user = new User();
        Assert.False(user.IsAdmin);
    }

    [Fact]
    public void User_CanBeAdmin()
    {
        var user = new User();
        user.IsAdmin = true;
        Assert.True(user.IsAdmin);
    }

    [Fact]
    public void UserName_IsStoredCorrectly()
    {
        var user = new User();
        user.UserName = "test";
        Assert.Equal("test", user.UserName);
    }
}