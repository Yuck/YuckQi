using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class MemoryCachingBehaviorOptionsTests
{
    [Test]
    public void Configure_WithAction_ResolvesConfiguredOptions()
    {
        var services = new ServiceCollection();

        services.Configure<MemoryCachingBehaviorOptions>(t =>
        {
            t.CacheDuration = TimeSpan.FromMinutes(5);
            t.Size = 3;
        });

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<MemoryCachingBehaviorOptions>>().Value;

        Assert.That(options.CacheDuration, Is.EqualTo(TimeSpan.FromMinutes(5)));
        Assert.That(options.Size, Is.EqualTo(3));
    }

    [Test]
    public void Configure_WithConfigurationSection_BindsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<String, String?> { ["Caching:CacheDuration"] = "00:10:00", ["Caching:Size"] = "4" }).Build();
        var services = new ServiceCollection();

        services.Configure<MemoryCachingBehaviorOptions>(configuration.GetSection("Caching"));

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<MemoryCachingBehaviorOptions>>().Value;

        Assert.That(options.CacheDuration, Is.EqualTo(TimeSpan.FromMinutes(10)));
        Assert.That(options.Size, Is.EqualTo(4));
    }

    [Test]
    public void Defaults_HaveNoDurationAndSizeOfOne()
    {
        var options = new MemoryCachingBehaviorOptions();

        Assert.That(options.CacheDuration, Is.Null);
        Assert.That(options.SerializerOptions, Is.Null);
        Assert.That(options.Size, Is.EqualTo(1));
    }

    [Test]
    public void Size_WhenNegative_Throws()
    {
        var options = new MemoryCachingBehaviorOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Size = -1);
    }
}
