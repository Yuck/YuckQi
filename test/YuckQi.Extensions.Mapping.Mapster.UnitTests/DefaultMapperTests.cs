using Mapster;
using NUnit.Framework;
using YuckQi.Extensions.Mapping.Mapster;

namespace YuckQi.Extensions.Mapping.Mapster.UnitTests;

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

    private static DefaultMapper CreateNullResultMapper()
    {
        var configuration = new TypeAdapterConfig();
        configuration.NewConfig<SourceType, DestinationType>().MapWith(_ => null!);

        return new DefaultMapper(configuration);
    }

    [Test]
    public void Constructor_WhenConfigurationIsNull_UsesDefaultConfiguration()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };

        var result = mapper.Map<SourceType, DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("A"));
    }

    [Test]
    public void Constructor_WhenConfigurationIsProvided_UsesProvidedConfiguration()
    {
        var configuration = new TypeAdapterConfig();
        configuration.NewConfig<SourceType, DestinationType>();
        var mapper = new DefaultMapper(configuration);
        var source = new SourceType { Id = 1, Name = "A" };

        var result = mapper.Map<SourceType, DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("A"));
    }

    [Test]
    public void Map_WithSourceDestinationAndTypes_MapsToDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        var result = mapper.Map(source, destination, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.SameAs(destination));
        Assert.That(((DestinationType) result).Id, Is.EqualTo(1));
        Assert.That(((DestinationType) result).Name, Is.EqualTo("A"));
    }

    [Test]
    public void Map_WithSourceDestinationAndTypes_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var destination = new DestinationType();

        Assert.That(() => mapper.Map(null!, destination, typeof(SourceType), typeof(DestinationType)), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithSourceDestinationAndTypes_WhenDestinationIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.Map(source, null!, typeof(SourceType), typeof(DestinationType)), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithSourceDestinationAndTypes_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        Assert.That(() => mapper.Map(source, destination, typeof(SourceType), typeof(DestinationType)), Throws.InvalidOperationException);
    }

    [Test]
    public void Map_WithSourceAndTypes_ReturnsNewInstance()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 2, Name = "B" };

        var result = mapper.Map(source, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.InstanceOf<DestinationType>());
        var dest = (DestinationType) result;
        Assert.That(dest.Id, Is.EqualTo(2));
        Assert.That(dest.Name, Is.EqualTo("B"));
    }

    [Test]
    public void Map_WithSourceAndTypes_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);

        Assert.That(() => mapper.Map(null!, typeof(SourceType), typeof(DestinationType)), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithSourceAndTypes_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.Map(source, typeof(SourceType), typeof(DestinationType)), Throws.InvalidOperationException);
    }

    [Test]
    public void Map_WithGenericDestination_ReturnsTypedDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 3, Name = "C" };

        var result = mapper.Map<DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("C"));
    }

    [Test]
    public void Map_WithGenericDestination_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);

        Assert.That(() => mapper.Map<DestinationType>(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithGenericDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.Map<DestinationType>(source), Throws.InvalidOperationException);
    }

    [Test]
    public void Map_WithGenericSourceDestinationAndExistingDestination_MapsIntoDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 5, Name = "E" };
        var destination = new DestinationType { Id = 0, Name = String.Empty };

        var result = mapper.Map(source, destination);

        Assert.That(result, Is.SameAs(destination));
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.Name, Is.EqualTo("E"));
    }

    [Test]
    public void Map_WithGenericSourceDestinationAndExistingDestination_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var destination = new DestinationType();

        Assert.That(() => mapper.Map((SourceType) null!, destination), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithGenericSourceDestinationAndExistingDestination_WhenDestinationIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.Map(source, (DestinationType) null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithGenericSourceDestinationAndExistingDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        Assert.That(() => mapper.Map(source, destination), Throws.InvalidOperationException);
    }

    [Test]
    public void Map_WithGenericSourceAndDestination_ReturnsTypedDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 4, Name = "D" };

        var result = mapper.Map<SourceType, DestinationType>(source);

        Assert.That(result.Id, Is.EqualTo(4));
        Assert.That(result.Name, Is.EqualTo("D"));
    }

    [Test]
    public void Map_WithGenericSourceAndDestination_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);

        Assert.That(() => mapper.Map<SourceType, DestinationType>(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Map_WithGenericSourceAndDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.Map<SourceType, DestinationType>(source), Throws.InvalidOperationException);
    }

    [Test]
    public void MapOrNull_WithSourceDestinationAndTypes_MapsToDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        var result = mapper.MapOrNull(source, destination, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.SameAs(destination));
        Assert.That(((DestinationType) result!).Id, Is.EqualTo(1));
        Assert.That(((DestinationType) result).Name, Is.EqualTo("A"));
    }

    [Test]
    public void MapOrNull_WithSourceDestinationAndTypes_WhenSourceIsNull_ReturnsNull()
    {
        var mapper = new DefaultMapper(null);
        var destination = new DestinationType();

        var result = mapper.MapOrNull(null, destination, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOrNull_WithSourceDestinationAndTypes_WhenDestinationIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.MapOrNull(source, null!, typeof(SourceType), typeof(DestinationType)), Throws.ArgumentNullException);
    }

    [Test]
    public void MapOrNull_WithSourceDestinationAndTypes_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        Assert.That(() => mapper.MapOrNull(source, destination, typeof(SourceType), typeof(DestinationType)), Throws.InvalidOperationException);
    }

    [Test]
    public void MapOrNull_WithSourceAndTypes_ReturnsNewInstance()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 2, Name = "B" };

        var result = mapper.MapOrNull(source, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.InstanceOf<DestinationType>());
        var dest = (DestinationType) result!;
        Assert.That(dest.Id, Is.EqualTo(2));
        Assert.That(dest.Name, Is.EqualTo("B"));
    }

    [Test]
    public void MapOrNull_WithSourceAndTypes_WhenSourceIsNull_ReturnsNull()
    {
        var mapper = new DefaultMapper(null);

        var result = mapper.MapOrNull(null, typeof(SourceType), typeof(DestinationType));

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOrNull_WithSourceAndTypes_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.MapOrNull(source, typeof(SourceType), typeof(DestinationType)), Throws.InvalidOperationException);
    }

    [Test]
    public void MapOrNull_WithGenericDestination_ReturnsTypedDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 3, Name = "C" };

        var result = mapper.MapOrNull<DestinationType>(source);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("C"));
    }

    [Test]
    public void MapOrNull_WithGenericDestination_WhenSourceIsNull_ReturnsNull()
    {
        var mapper = new DefaultMapper(null);

        var result = mapper.MapOrNull<DestinationType>(null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOrNull_WithGenericDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.MapOrNull<DestinationType>(source), Throws.InvalidOperationException);
    }

    [Test]
    public void MapOrNull_WithGenericSourceDestinationAndExistingDestination_MapsIntoDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 5, Name = "E" };
        var destination = new DestinationType { Id = 0, Name = String.Empty };

        var result = mapper.MapOrNull(source, destination);

        Assert.That(result, Is.SameAs(destination));
        Assert.That(result!.Id, Is.EqualTo(5));
        Assert.That(result.Name, Is.EqualTo("E"));
    }

    [Test]
    public void MapOrNull_WithGenericSourceDestinationAndExistingDestination_WhenSourceIsNull_ReturnsNull()
    {
        var mapper = new DefaultMapper(null);
        SourceType? source = null;
        var destination = new DestinationType { Id = 9, Name = "Unchanged" };

        var result = mapper.MapOrNull(source, destination);

        Assert.That(result, Is.Null);
        Assert.That(destination.Id, Is.EqualTo(9));
        Assert.That(destination.Name, Is.EqualTo("Unchanged"));
    }

    [Test]
    public void MapOrNull_WithGenericSourceDestinationAndExistingDestination_WhenDestinationIsNull_ThrowsArgumentNullException()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.MapOrNull(source, (DestinationType) null!), Throws.ArgumentNullException);
    }

    [Test]
    public void MapOrNull_WithGenericSourceDestinationAndExistingDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };
        var destination = new DestinationType();

        Assert.That(() => mapper.MapOrNull(source, destination), Throws.InvalidOperationException);
    }

    [Test]
    public void MapOrNull_WithGenericSourceAndDestination_ReturnsTypedDestination()
    {
        var mapper = new DefaultMapper(null);
        var source = new SourceType { Id = 4, Name = "D" };

        var result = mapper.MapOrNull<SourceType, DestinationType>(source);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(4));
        Assert.That(result.Name, Is.EqualTo("D"));
    }

    [Test]
    public void MapOrNull_WithGenericSourceAndDestination_WhenSourceIsNull_ReturnsNull()
    {
        var mapper = new DefaultMapper(null);
        SourceType? source = null;

        var result = mapper.MapOrNull<SourceType, DestinationType>(source);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOrNull_WithGenericSourceAndDestination_WhenMappingProducesNull_ThrowsInvalidOperationException()
    {
        var mapper = CreateNullResultMapper();
        var source = new SourceType { Id = 1, Name = "A" };

        Assert.That(() => mapper.MapOrNull<SourceType, DestinationType>(source), Throws.InvalidOperationException);
    }
}
