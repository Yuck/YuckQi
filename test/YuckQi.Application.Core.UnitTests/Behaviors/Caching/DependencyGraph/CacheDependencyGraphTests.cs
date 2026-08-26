using NUnit.Framework;
using YuckQi.Application.Core.Behaviors.Caching;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching.DependencyGraph;

public class CacheDependencyGraphTests
{
    [Test]
    public void Build_SnapshotsDependencies_SoLaterBuilderChangesDoNotAffectGraph()
    {
        var builder = new CacheDependencyGraphBuilder();

        builder.When("order", t => t.Invalidates("order-detail"));

        var graph = builder.Build();

        builder.When("order", t => t.InvalidatesGlobal("order-list"));

        var expanded = graph.GetExpandedCacheKeys([CacheKeyFactory.Create("order", 42)]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            "order:42",
            "order-detail:42"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_WhenEmptyGraph_ReturnsKeysOnly()
    {
        var expanded = CacheDependencyGraph.Empty.GetExpandedCacheKeys([new CacheKey("order:42"), new CacheKey("order:42")]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[] { "order:42" }));
    }

    [Test]
    public void GetExpandedCacheKeys_ExpandsSameIdentifierAndGlobalDependents()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.Invalidates("order-detail")
                                                                           .InvalidatesGlobal("order-list")));

        var expanded = graph.GetExpandedCacheKeys([CacheKeyFactory.Create("order", 42)]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            "order:42",
            "order-detail:42",
            "order-list"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_ExpandsFromParameter()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.InvalidatesFromParameter("customer-summary", "customer")));

        var key = CacheKeyFactory.Create("order", 42, ("customer", 7));
        var expanded = graph.GetExpandedCacheKeys([key]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            (String) key,
            "customer-summary:7"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_WhenParameterMissing_SkipsParameterDependent()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.InvalidatesFromParameter("customer-summary", "customer")
                                                                           .InvalidatesGlobal("order-list")));

        var expanded = graph.GetExpandedCacheKeys([CacheKeyFactory.Create("order", 42)]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            "order:42",
            "order-list"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_WalksTransitiveDependents()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.Invalidates("customer-summary"))
                                                      .When("customer-summary", u => u.InvalidatesGlobal("dashboard")));

        var expanded = graph.GetExpandedCacheKeys([CacheKeyFactory.Create("order", 7)]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            "order:7",
            "customer-summary:7",
            "dashboard"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_WhenCycleExists_DoesNotLoopForever()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("alpha", u => u.Invalidates("beta"))
                                                      .When("beta", u => u.Invalidates("alpha")));

        var expanded = graph.GetExpandedCacheKeys([CacheKeyFactory.Create("alpha", 1)]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            "alpha:1",
            "beta:1"
        }));
    }

    [Test]
    public void GetExpandedCacheKeys_SupportsCustomFactories()
    {
        var graph = CacheDependencyGraph.Create(t => t.When("order", u => u.Invalidates(v => v.Parameter("customer") is { } customerId ? v.Key("customer", customerId) : (CacheKey?) null)
                                                                           .Invalidates(v => new[] { v.Global("order-list"), v.Global("nav") })));

        var key = CacheKeyFactory.Create("order", 42, ("customer", 7));
        var expanded = graph.GetExpandedCacheKeys([key]);

        Assert.That(expanded.Select(t => (String) t), Is.EquivalentTo(new[]
        {
            (String) key,
            "customer:7",
            "order-list",
            "nav"
        }));
    }
}
