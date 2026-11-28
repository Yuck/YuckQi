using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Caching;

public class DistributedCachingBehaviorOptionsTests
{
    [Test]
    public void Configure_WithAction_ResolvesConfiguredOptions()
    {
        var services = new ServiceCollection();

        services.Configure<DistributedCachingBehaviorOptions>(t => t.CacheDuration = TimeSpan.FromMinutes(5));

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<DistributedCachingBehaviorOptions>>().Value;

        Assert.That(options.CacheDuration, Is.EqualTo(TimeSpan.FromMinutes(5)));
    }

    [Test]
    public void Configure_WithConfigurationSection_BindsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<String, String?> { ["Caching:CacheDuration"] = "00:10:00" }).Build();
        var services = new ServiceCollection();

        services.Configure<DistributedCachingBehaviorOptions>(configuration.GetSection("Caching"));

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<DistributedCachingBehaviorOptions>>().Value;

        Assert.That(options.CacheDuration, Is.EqualTo(TimeSpan.FromMinutes(10)));
    }

    [Test]
    public void Defaults_HaveNoDurationAndNoSerializerOptions()
    {
        var options = new DistributedCachingBehaviorOptions();

        Assert.That(options.CacheDuration, Is.Null);
        Assert.That(options.SerializerOptions, Is.Null);
    }
}
