using Dapper;
using NUnit.Framework;
using YuckQi.Data.Sql.Dapper.Schema;
using Annotations = System.ComponentModel.DataAnnotations.Schema;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Schema;

public class RecordSchemaTests
{
    [Test]
    public void GetColumnName_WithPropertyName_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("Name"), Is.EqualTo("Name"));
    }

    [Test]
    public void GetColumnName_WithColumnMappedPropertyName_ReturnsColumnName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("DisplayName"), Is.EqualTo("display_name"));
    }

    [Test]
    public void GetColumnName_WithDataAnnotationsColumnMappedPropertyName_ReturnsColumnName()
    {
        Assert.That(RecordSchema<AnnotatedRecord>.GetColumnName("DisplayName"), Is.EqualTo("display_name"));
    }

    [Test]
    public void GetColumnName_WithBlankColumnName_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("Blank"), Is.EqualTo("Blank"));
    }

    [Test]
    public void GetColumnName_WithDifferentlyCasedPropertyName_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("name"), Is.EqualTo("Name"));
    }

    [Test]
    public void GetColumnName_WithIgnoreSelectPropertyName_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("Notes"), Is.EqualTo("Notes"));
    }

    [Test]
    public void GetColumnName_WithGetOnlyAutoPropertyName_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("Code"), Is.EqualTo("Code"));
    }

    [Test]
    public void GetColumnName_WithColumnName_ReturnsColumnName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("display_name"), Is.EqualTo("display_name"));
    }

    [Test]
    public void GetColumnName_WithDifferentlyCasedColumnName_ReturnsColumnName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetColumnName("DISPLAY_NAME"), Is.EqualTo("display_name"));
    }

    [Test]
    public void GetProperty_WithColumnName_ReturnsMappedProperty()
    {
        Assert.That(RecordSchema<ResolvableRecord>.GetProperty("display_name").Name, Is.EqualTo(nameof(ResolvableRecord.DisplayName)));
    }

    [Test]
    public void GetColumnName_WithNameOfPropertyAndOfAnotherPropertysColumn_PrefersProperty()
    {
        Assert.That(RecordSchema<OverlappingNamesRecord>.GetColumnName("Name"), Is.EqualTo("full_name"));
    }

    [Test]
    public void GetColumnName_WithPrimaryConstructorProperty_ReturnsPropertyName()
    {
        Assert.That(RecordSchema<PrimaryConstructorRecord>.GetColumnName("Id"), Is.EqualTo("Id"));
    }

    [Test]
    public void SelectableProperties_IncludesConstructorBoundGetOnlyProperties()
    {
        var names = RecordSchema<PrimaryConstructorRecord>.SelectableProperties.Select(t => t.Name);

        Assert.That(names, Is.EqualTo(new[] { "Id", "Name" }));
    }

    [Test]
    public void SelectableProperties_IncludesFieldBackedConstructorBoundProperties()
    {
        var names = RecordSchema<FieldBackedRecord>.SelectableProperties.Select(t => t.Name);

        Assert.That(names, Is.EqualTo(new[] { "Id" }));
    }

    [Test]
    public void TableName_WithDapperAndDataAnnotationsTableAttributes_PrefersDapperAttribute()
    {
        Assert.That(RecordSchema<DoublyAnnotatedRecord>.TableName, Is.EqualTo("dapper_table"));
    }

    [Test]
    public void SchemaName_WithDapperAndDataAnnotationsTableAttributes_PrefersDapperAttribute()
    {
        Assert.That(RecordSchema<DoublyAnnotatedRecord>.SchemaName, Is.Null);
    }

    [Test]
    public void GetColumnName_WithDapperAndDataAnnotationsColumnAttributes_PrefersDapperAttribute()
    {
        Assert.That(RecordSchema<DoublyAnnotatedRecord>.GetColumnName("Name"), Is.EqualTo("dapper_column"));
    }

    [Test]
    public void GetColumnName_WithUnknownName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName("Name]; drop table [Resolvable]; --"); });
    }

    [Test]
    public void GetColumnName_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName(String.Empty); });
    }

    [Test]
    public void GetColumnName_WithStaticPropertyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName(nameof(ResolvableRecord.Shared)); });
    }

    [Test]
    public void GetColumnName_WithIndexerName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName("Item"); });
    }

    [Test]
    public void GetColumnName_WithComputedPropertyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName(nameof(ResolvableRecord.Display)); });
    }

    [Test]
    public void GetColumnName_WithNotMappedPropertyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName(nameof(ResolvableRecord.Ignored)); });
    }

    [Test]
    public void GetColumnName_WithDataAnnotationsNotMappedPropertyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<ResolvableRecord>.GetColumnName(nameof(ResolvableRecord.AlsoIgnored)); });
    }

    [Test]
    public void GetColumnName_WithExactCaseOfCaseVariantProperties_ReturnsExactProperty()
    {
        Assert.That(RecordSchema<CaseVariantRecord>.GetColumnName("URL"), Is.EqualTo("URL"));
    }

    [Test]
    public void GetColumnName_WithAmbiguousCaseOfCaseVariantProperties_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => { _ = RecordSchema<CaseVariantRecord>.GetColumnName("url"); });
    }

    [Test]
    public void SelectableProperties_ExcludesUnmappedAndIgnoreSelectProperties()
    {
        var names = RecordSchema<ResolvableRecord>.SelectableProperties.Select(t => t.Name);

        Assert.That(names, Is.EqualTo(new[] { "Id", "Name", "DisplayName", "Blank", "Code" }));
    }

    [Test]
    public void SelectableProperties_IncludesCollectionProperties()
    {
        var names = RecordSchema<CollectionRecord>.SelectableProperties.Select(t => t.Name);

        Assert.That(names, Is.EqualTo(new[] { "Id", "AccessRoleIds" }));
    }

    [Test]
    public void TableName_WithDapperTableAttribute_ReturnsAttributeName()
    {
        Assert.That(RecordSchema<ResolvableRecord>.TableName, Is.EqualTo("Resolvable"));
    }

    [Test]
    public void TableName_WithDataAnnotationsTableAttribute_ReturnsAttributeName()
    {
        Assert.That(RecordSchema<AnnotatedRecord>.TableName, Is.EqualTo("customers"));
    }

    [Test]
    public void TableName_WithoutTableAttribute_ReturnsTypeName()
    {
        Assert.That(RecordSchema<CaseVariantRecord>.TableName, Is.EqualTo(nameof(CaseVariantRecord)));
    }

    [Test]
    public void SchemaName_WithDapperTableAttributeSchema_ReturnsSchema()
    {
        Assert.That(RecordSchema<DapperSchemaRecord>.SchemaName, Is.EqualTo("audit"));
    }

    [Test]
    public void SchemaName_WithDataAnnotationsTableAttributeSchema_ReturnsSchema()
    {
        Assert.That(RecordSchema<AnnotatedRecord>.SchemaName, Is.EqualTo("sales"));
    }

    [Test]
    public void SchemaName_WithoutSchema_IsNull()
    {
        Assert.That(RecordSchema<ResolvableRecord>.SchemaName, Is.Null);
    }

    [Table("Resolvable")]
    public class ResolvableRecord(String code)
    {
        public static String Shared { get; set; } = String.Empty;

        public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;

        [Column("display_name")]
        public String DisplayName { get; set; } = String.Empty;

        [Column(" ")]
        public String Blank { get; set; } = String.Empty;

        [IgnoreSelect]
        public String Notes { get; set; } = String.Empty;

        public String Code { get; } = code;

        public String Display => $"{Code}: {Name}";

        [NotMapped]
        public String Ignored { get; set; } = String.Empty;

        [Annotations.NotMapped]
        public String AlsoIgnored { get; set; } = String.Empty;

        public String this[Int32 index] => Name;
    }

    [Annotations.Table("customers", Schema = "sales")]
    public class AnnotatedRecord
    {
        public Int32 Id { get; set; }

        [Annotations.Column("display_name")]
        public String DisplayName { get; set; } = String.Empty;
    }

    [Table("Audited", Schema = "audit")]
    public class DapperSchemaRecord
    {
        public Int32 Id { get; set; }
    }

    public class CollectionRecord
    {
        public Int32 Id { get; set; }

        public IReadOnlyList<Guid> AccessRoleIds { get; set; } = [];
    }

    public class OverlappingNamesRecord
    {
        [Column("full_name")]
        public String Name { get; set; } = String.Empty;

        [Column("Name")]
        public String Nickname { get; set; } = String.Empty;
    }

    public class PrimaryConstructorRecord(Int32 id, String name)
    {
        public Int32 Id => id;

        public String Name => name;

        public String Display => $"{id}: {name}";
    }

    public class FieldBackedRecord
    {
        private readonly Int32 _identifier;

        public FieldBackedRecord(Int32 id)
        {
            _identifier = id;
        }

        public Int32 Id => _identifier;

        public Int32 Doubled => _identifier * 2;
    }

    [Table("dapper_table")]
    [Annotations.Table("annotations_table", Schema = "annotations")]
    public class DoublyAnnotatedRecord
    {
        [Column("dapper_column")]
        [Annotations.Column("annotations_column")]
        public String Name { get; set; } = String.Empty;
    }

    public class CaseVariantRecord
    {
        public String Url { get; set; } = String.Empty;

        public String URL { get; set; } = String.Empty;
    }
}
