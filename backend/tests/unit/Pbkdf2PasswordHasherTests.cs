using Ehsms.Modules.Identity.Infrastructure.Authentication;
using Xunit;

namespace Ehsms.UnitTests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesSelfDescriptingFormat()
    {
        var hash = _hasher.Hash("Correct-Horse-Battery-Staple");

        Assert.StartsWith("PBKDF2$", hash);
        Assert.Equal(4, hash.Split('$').Length);
    }

    [Fact]
    public void Verify_MatchesCorrectPassword()
    {
        var hash = _hasher.Hash("S3cret!");

        Assert.True(_hasher.Verify("S3cret!", hash));
    }

    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = _hasher.Hash("S3cret!");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_IsDifferentForEachCall()
    {
        var a = _hasher.Hash("same");
        var b = _hasher.Hash("same");

        Assert.NotEqual(a, b); // random salt
    }

    [Theory]
    [InlineData("")]
    [InlineData("PBKDF2$100000$AAAA")]
    public void Verify_RejectsMalformedHash(string malformed)
    {
        Assert.False(_hasher.Verify("anything", malformed));
    }
}