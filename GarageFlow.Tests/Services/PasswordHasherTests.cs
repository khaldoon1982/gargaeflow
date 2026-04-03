using GarageFlow.Infrastructure.Services;
using Xunit;

namespace GarageFlow.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ShouldReturnDifferentHashForSamePassword()
    {
        var hash1 = _hasher.Hash("test123");
        var hash2 = _hasher.Hash("test123");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ShouldReturnTrueForCorrectPassword()
    {
        var hash = _hasher.Hash("admin123");
        Assert.True(_hasher.Verify("admin123", hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalseForWrongPassword()
    {
        var hash = _hasher.Hash("admin123");
        Assert.False(_hasher.Verify("wrong", hash));
    }
}
