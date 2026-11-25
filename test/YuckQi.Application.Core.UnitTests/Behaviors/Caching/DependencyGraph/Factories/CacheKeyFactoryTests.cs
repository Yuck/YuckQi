using NUnit.Framework;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching.DependencyGraph.Factories;

public class CacheKeyFactoryTests
{
    [Test]
    public void Create_WithResourceOnly_ReturnsCacheKey()
    {
        var key = CacheKeyFactory.Create("order-list");

        Assert.That((String) key, Is.EqualTo("order-list"));
    }

    [Test]
    public void Create_WithIdentifier_ReturnsCacheKey()
    {
        Assert.That((String) CacheKeyFactory.Create("order", 42), Is.EqualTo("order:42"));
    }

    [Test]
    public void Create_WithParameters_ReturnsCacheKey()
    {
        var key = CacheKeyFactory.Create("order", 42, ("customer", 7), ("region", "east"));

        Assert.That((String) key, Is.EqualTo("order:42;customer=7;region=east"));
    }

    [Test]
    public void TryParse_WithResourceOnly_Succeeds()
    {
        var parts = ParseSuccessfully("order-list");

        Assert.That(parts.Resource, Is.EqualTo("order-list"));
        Assert.That(parts.Identifier, Is.Null);
        Assert.That(parts.Parameters, Is.Empty);
        Assert.That((String) parts.ToKey(), Is.EqualTo("order-list"));
    }

    [Test]
    public void TryParse_WithIdentifier_Succeeds()
    {
        var parts = ParseSuccessfully("order:42");

        Assert.That(parts.Resource, Is.EqualTo("order"));
        Assert.That(parts.Identifier, Is.EqualTo("42"));
        Assert.That(parts.Parameters, Is.Empty);
        Assert.That((String) parts.ToKey(), Is.EqualTo("order:42"));
    }

    [Test]
    public void TryParse_WithParameters_Succeeds()
    {
        var parts = ParseSuccessfully("order:42;customer=7;region=east");

        Assert.That(parts.Resource, Is.EqualTo("order"));
        Assert.That(parts.Identifier, Is.EqualTo("42"));
        Assert.That(parts.Parameters["customer"], Is.EqualTo("7"));
        Assert.That(parts.Parameters["region"], Is.EqualTo("east"));
        Assert.That((String) parts.ToKey(), Is.EqualTo("order:42;customer=7;region=east"));
    }

    [Test]
    public void TryParse_WhenNullOrWhiteSpace_ReturnsFalse()
    {
        Assert.That(CacheKeyFactory.TryParse((String?) null, out var parts), Is.False);
        Assert.That(parts, Is.Null);
        Assert.That(CacheKeyFactory.TryParse(" ", out parts), Is.False);
        Assert.That(parts, Is.Null);
    }

    [Test]
    public void TryParse_WhenMalformed_ReturnsFalse()
    {
        Assert.That(CacheKeyFactory.TryParse(":42", out var parts), Is.False);
        Assert.That(parts, Is.Null);
        Assert.That(CacheKeyFactory.TryParse("order:", out parts), Is.False);
        Assert.That(parts, Is.Null);
        Assert.That(CacheKeyFactory.TryParse("order:42;customer", out parts), Is.False);
        Assert.That(parts, Is.Null);
        Assert.That(CacheKeyFactory.TryParse("order:42;=7", out parts), Is.False);
        Assert.That(parts, Is.Null);
    }

    [Test]
    public void CacheKey_WhenNullOrWhiteSpace_Throws()
    {
        Assert.That(() => new CacheKey(" "), Throws.TypeOf<ArgumentException>());
        Assert.That(() => (CacheKey) "", Throws.TypeOf<ArgumentException>());
    }

    private static CacheKeyParts ParseSuccessfully(String key)
    {
        if (CacheKeyFactory.TryParse(key, out var parts))
            return parts;

        throw new AssertionException($"Expected '{key}' to parse as a cache key.");
    }
}
