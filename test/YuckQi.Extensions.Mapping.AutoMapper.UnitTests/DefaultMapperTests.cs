using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using YuckQi.Extensions.Mapping.AutoMapper;

namespace YuckQi.Extensions.Mapping.AutoMapper.UnitTests;

public class DefaultMapperTests
{
    private sealed class DestinationType
    {
        public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    private sealed class SourceType
    {
        public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    private static IConfigurationProvider CreateConfiguration()
    {
        return new MapperConfiguration(t =>
        {
            t.CreateMap<SourceType, DestinationType>();
        }, new NullLoggerFactory());
    }

    [Test]
    public void Constructor_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultMapper(null!));
    }

    [Test]
    public void Map_WithSourceAndDestinationAndTypes_MapsToDestination()
    {
        var configuration = CreateConfiguration();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        var result = mapper.Map(source, destination, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.SameAs(destination));
        Assert.That(((DestinationType) result).Id, Is.EqualTo(1));
        Assert.That(((DestinationType) result).Name, Is.EqualTo("A"));
    }

    [Test]
    public void Map_WithSourceAndTypes_ReturnsNewInstance()
    {
        var configuration = CreateConfiguration();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 2, Name = "B" };

        var result = mapper.Map(source, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.InstanceOf<DestinationType>());
        var dest = (DestinationType) result;
        Assert.That(dest.Id, Is.EqualTo(2));
        Assert.That(dest.Name, Is.EqualTo("B"));
    }

    [Test]
    public void Map_WithGenericDestination_ReturnsTypedDestination()
    {
        var configuration = CreateConfiguration();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 3, Name = "C" };

        var result = mapper.Map<DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("C"));
    }

    [Test]
    public void Map_WithGenericSourceAndDestination_ReturnsTypedDestination()
    {
        var configuration = CreateConfiguration();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 4, Name = "D" };

        var result = mapper.Map<SourceType, DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(4));
        Assert.That(result.Name, Is.EqualTo("D"));
    }

    [Test]
    public void Map_WithGenericSourceDestinationAndExistingDestination_MapsIntoDestination()
    {
        var configuration = CreateConfiguration();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 5, Name = "E" };
        var destination = new DestinationType { Id = 0, Name = String.Empty };

        var result = mapper.Map(source, destination);

        Assert.That(result, Is.SameAs(destination));
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.Name, Is.EqualTo("E"));
    }
}
