using System;
using MRP.System;
using Xunit;

public class UserTests
{
    [Fact]
    public void PasswordHash_IsNotPlainText()
    {
        var hash = User._HashPassword("test", "secret");
        Assert.NotEqual("secret", hash);
    }

    [Fact]
    public void SameUserAndPassword_ProducesSameHash()
    {
        var h1 = User._HashPassword("test", "pw");
        var h2 = User._HashPassword("test", "pw");
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void DifferentUsers_ProduceDifferentHashes()
    {
        var h1 = User._HashPassword("a", "pw");
        var h2 = User._HashPassword("b", "pw");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void EmptyPassword_ReturnsNull()
    {
        var hash = User._HashPassword("test", "");
        Assert.Null(hash);
    }

    [Fact]
    public void Username_CannotBeEmpty()
    {
        var user = new User();
        Assert.Throws<ArgumentException>(() => user.UserName = "");
    }

    [Fact]
    public void SetPassword_SetsHash()
    {
        var user = new User();
        user.UserName = "test";
        user.SetPassword("pw");
        Assert.NotNull(((__IAuthentificable)user).__PasswordHash);
    }

    [Fact]
    public void ChangingPassword_ChangesPasswordHash()
    {
        var user = new User();
        user.UserName = "test";

        user.SetPassword("pw1");
        var hash1 = ((__IAuthentificable)user).__PasswordHash;

        user.SetPassword("pw2");
        var hash2 = ((__IAuthentificable)user).__PasswordHash;

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void PasswordHash_DependsOnUsernameAndPassword()
    {
        var hash1 = User._HashPassword("user1", "pw");
        var hash2 = User._HashPassword("user2", "pw");

        Assert.NotEqual(hash1, hash2);
    }

}