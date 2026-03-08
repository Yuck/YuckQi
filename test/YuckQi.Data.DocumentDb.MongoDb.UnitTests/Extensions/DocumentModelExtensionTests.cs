using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.MongoDb.Attributes;
using YuckQi.Data.DocumentDb.MongoDb.Extensions;

namespace YuckQi.Data.DocumentDb.MongoDb.UnitTests.Extensions;

public class DocumentModelExtensionTests
{
    private static readonly SurLaTableDocument? NullSurLaTableRecord = null;
    private static readonly Type? NullType = null;

    [Test]
    public void GetCollectionName_WithNullType_IsNull()
    {
        var name = NullType.GetCollectionName();

        Assert.That(name, Is.Null);
    }

    [Test]
    public async Task GetCollectionName_WithMultipleRequests_IsValid()
    {
        var type = typeof(SurLaTableDocument);
        var tasks = new[] { 1, 2, 3, 4, 5 }.Select(_ => Task.Run(() => type.GetCollectionName())).ToList();

        await Task.WhenAll(tasks);

        var first = tasks.First().Result;

        Assert.That(tasks.All(t => Equals(t.Result, first)), Is.True);
    }

    [Test]
    public void GetCollectionName_WithCollectionAttribute_ReturnsAttributeName()
    {
        var name = typeof(DecoratedDocument).GetCollectionName();

        Assert.That(name, Is.EqualTo("CustomCollection"));
    }

    [Test]
    public void GetCollectionName_WithoutCollectionAttribute_ReturnsTypeName()
    {
        var name = typeof(SurLaTableDocument).GetCollectionName();

        Assert.That(name, Is.EqualTo(nameof(SurLaTableDocument)));
    }

    [Test]
    public void GetDatabaseName_WithNullType_IsNull()
    {
        var name = NullType.GetDatabaseName();

        Assert.That(name, Is.Null);
    }

    [Test]
    public async Task GetDatabaseName_WithMultipleRequests_IsValid()
    {
        var type = typeof(SurLaTableDocument);
        var tasks = new[] { 1, 2, 3, 4, 5 }.Select(_ => Task.Run(() => type.GetDatabaseName())).ToList();

        await Task.WhenAll(tasks);

        var first = tasks.First().Result;

        Assert.That(tasks.All(t => Equals(t.Result, first)), Is.True);
    }

    [Test]
    public void GetDatabaseName_WithDatabaseAttribute_ReturnsAttributeName()
    {
        var name = typeof(SurLaTableDocument).GetDatabaseName();

        Assert.That(name, Is.EqualTo("Tableau"));
    }

    [Test]
    public void GetDatabaseName_WithoutDatabaseAttribute_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(UndecoratedDocument).GetDatabaseName());
    }

    [Test]
    public void GetIdentifierFieldDefinition_WithNullType_IsNull()
    {
        var name = NullType.GetIdentifierFieldDefinition<SurLaTableDocument, Int32>();

        Assert.That(name, Is.Null);
    }

    [Test]
    public async Task GetIdentifierFieldDefinition_WithMultipleRequests_IsValid()
    {
        var type = typeof(SurLaTableDocument);
        var tasks = new[] { 1, 2, 3, 4, 5 }.Select(_ => Task.Run(() => type.GetIdentifierFieldDefinition<SurLaTableDocument, Int32>())).ToList();

        await Task.WhenAll(tasks);

        var registry = BsonSerializer.SerializerRegistry;
        var serializer = registry.GetSerializer<SurLaTableDocument>();
        var first = tasks.First().Result;
        var arguments = new RenderArgs<SurLaTableDocument>(serializer, registry);
        var field = first?.Render(arguments).FieldName;

        Assert.That(tasks.All(t => Equals(t.Result?.Render(arguments).FieldName, field)), Is.True);
    }

    [Test]
    public void GetIdentifierFieldDefinition_WithMismatchedType_ThrowsArgumentException()
    {
        var type = typeof(SurLaTableDocument);

        Assert.Throws<ArgumentException>(() => type.GetIdentifierFieldDefinition<DecoratedDocument, Int32>());
    }

    [Test]
    public void GetIdentifier_WithNullRecord_IsNull()
    {
        var name = NullSurLaTableRecord?.GetIdentifier<SurLaTableDocument, Int32>();

        Assert.That(name, Is.Null);
    }

    [Test]
    public async Task GetIdentifier_WithMultipleRequests_IsValid()
    {
        var document = new SurLaTableDocument { Id = 1, Name = "test" };
        var tasks = new[] { 1, 2, 3, 4, 5 }.Select(_ => Task.Run(() => document.GetIdentifier<SurLaTableDocument, Int32>())).ToList();

        await Task.WhenAll(tasks);

        var first = tasks.First().Result;

        Assert.That(tasks.All(t => Equals(t.Result, first)), Is.True);
    }

    [Test]
    public void GetIdentifier_WithValidDocument_ReturnsIdentifier()
    {
        var document = new SurLaTableDocument { Id = 42, Name = "test" };

        var result = document.GetIdentifier<SurLaTableDocument, Int32>();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void GetIdentifier_WithMismatchedIdentifierType_ThrowsInvalidOperationException()
    {
        var document = new SurLaTableDocument { Id = 1, Name = "test" };

        Assert.Throws<InvalidOperationException>(() => document.GetIdentifier<SurLaTableDocument, String>());
    }

    [Test]
    public void GetIdentifier_WithNoIdentifierProperty_ThrowsInvalidOperationException()
    {
        var document = new UndecoratedDocument { Name = "test" };

        Assert.Throws<InvalidOperationException>(() => document.GetIdentifier<UndecoratedDocument, Int32>());
    }

    [Database("Tableau")]
    public class SurLaTableDocument
    {
        [BsonId] public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    [Database("CustomDb")]
    [Collection("CustomCollection")]
    public class DecoratedDocument
    {
        [BsonId] public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    public class UndecoratedDocument
    {
        public String Name { get; set; } = String.Empty;
    }
}
