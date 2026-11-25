using Dapper;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Domain.ValueObjects;
using Annotations = System.ComponentModel.DataAnnotations.Schema;

namespace YuckQi.Data.Sql.Dapper.MySql.UnitTests;

public class SqlGeneratorTests
{
    [Test]
    public void GenerateCountQuery_WithSingleParameter_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Id", 1) };
        var sql = generator.GenerateCountQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select count(*) from `SurLaTable` where (`Id` = @Id);"));
    }

    [Test]
    public void GenerateGetQuery_WithSingleParameter_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Id", 1) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Id` = @Id);"));
    }

    [Test]
    public void GenerateSearchQuery_WithSingleParameter_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", "Some Guy") };
        var page = new Page(2, 50);
        var sort = new List<SortCriteria> { new("Name", SortOrder.Descending) }.OrderBy(t => t);
        var sql = generator.GenerateSearchQuery(parameters, page, sort).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` = @Name) order by `Name` desc limit 50 offset 50;"));
    }

    [Test]
    public void GenerateGetQuery_WithEqualOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` = @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithEqualOperation_AndNullValue_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", null) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` is null);"));
    }

    [Test]
    public void GenerateGetQuery_WithNotEqualOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.NotEqual, "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` != @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithNotEqualOperation_AndNullValue_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.NotEqual, null) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` is not null);"));
    }

    [Test]
    public void GenerateGetQuery_WithGreaterThanOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.GreaterThan, 1234) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` > @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithGreaterThanOrEqualOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.GreaterThanOrEqual, 1234) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` >= @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithLessThanOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.LessThan, 1234) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` < @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithLessThanOrEqualOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.LessThanOrEqual, 1234) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` <= @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithContainsOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, "Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` like @Name escape '\\\\');"));
    }

    [Test]
    public void GenerateGetQuery_WithContainsOperation_AndNullValue_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.Contains, null) };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateGetQuery(parameters); });
    }

    [Test]
    public void GenerateGetQuery_WithInOperation_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.In, new[] { "Bill", "Billy", "Mac", "Buddy" }) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` in (@Name0,@Name1,@Name2,@Name3));"));
    }

    [Test]
    public void GenerateGetQuery_WithInOperation_AndEmptySet_IsValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.In, new String[] { }) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` in (null));"));
    }

    [Test]
    public void GenerateGetQuery_WithInOperation_AndInvalidParameter_IsNotValid()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name", FilterOperation.In, 1234) };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " "); });
    }

    [Test]
    public void GenerateCountQuery_WithUnknownFieldName_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name`; drop table `SurLaTable`; --", 1) };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateCountQuery(parameters); });
    }

    [Test]
    public void GenerateGetQuery_WithUnknownFieldName_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("Name`; drop table `SurLaTable`; --", 1) };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateGetQuery(parameters); });
    }

    [Test]
    public void GenerateSearchQuery_WithUnknownSortExpression_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 10);
        var sort = new List<SortCriteria> { new("Name`; drop table `SurLaTable`; --", SortOrder.Ascending) }.OrderBy(t => t);

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateSearchQuery(parameters, page, sort); });
    }

    [Test]
    public void GenerateGetQuery_WithDifferentlyCasedFieldName_FiltersOnPropertyColumn()
    {
        var generator = new SqlGenerator<SurLaTableRecord>();
        var parameters = new[] { new FilterCriteria("name", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name` from `SurLaTable` where (`Name` = @name);"));
    }

    [Test]
    public void GenerateGetQuery_WithColumnMappedField_FiltersOnColumnName()
    {
        var generator = new SqlGenerator<ColumnMappedRecord>();
        var parameters = new[] { new FilterCriteria("DisplayName", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `display_name` `DisplayName` from `ColumnMapped` where (`display_name` = @DisplayName);"));
    }

    [Test]
    public void GenerateGetQuery_WithColumnNameAsFieldName_FiltersOnColumnName()
    {
        var generator = new SqlGenerator<ColumnMappedRecord>();
        var parameters = new[] { new FilterCriteria("display_name", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `display_name` `DisplayName` from `ColumnMapped` where (`display_name` = @display_name);"));
    }

    [Test]
    public void GenerateSearchQuery_WithColumnMappedSort_OrdersByColumnName()
    {
        var generator = new SqlGenerator<ColumnMappedRecord>();
        var parameters = Array.Empty<FilterCriteria>();
        var page = new Page(1, 10);
        var sort = new List<SortCriteria> { new("DisplayName", SortOrder.Ascending) }.OrderBy(t => t);
        var sql = generator.GenerateSearchQuery(parameters, page, sort).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `display_name` `DisplayName` from `ColumnMapped` order by `display_name` limit 10 offset 0;"));
    }

    [Test]
    public void GenerateGetQuery_WithQuoteCharacterInColumnName_EscapesIdentifier()
    {
        var generator = new SqlGenerator<QuotedColumnRecord>();
        var parameters = new[] { new FilterCriteria("Name", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `odd``name` `Name` from `QuotedColumn` where (`odd``name` = @Name);"));
    }

    [Test]
    public void GenerateGetQuery_WithUnmappedProperties_SelectsOnlyMappedColumns()
    {
        var generator = new SqlGenerator<UnmappedRecord>();
        var parameters = new[] { new FilterCriteria("Id", 1) };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `Name`, `Code` from `Unmapped` where (`Id` = @Id);"));
    }

    [Test]
    public void GenerateGetQuery_WithNotMappedFieldName_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<UnmappedRecord>();
        var parameters = new[] { new FilterCriteria("Ignored", 1) };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateGetQuery(parameters); });
    }

    [Test]
    public void GenerateGetQuery_WithComputedFieldName_ThrowsArgumentException()
    {
        var generator = new SqlGenerator<UnmappedRecord>();
        var parameters = new[] { new FilterCriteria("Display", "Some Guy") };

        Assert.Throws<ArgumentException>(() => { _ = generator.GenerateGetQuery(parameters); });
    }

    [Test]
    public void GenerateGetQuery_WithDataAnnotationsAttributes_UsesTableSchemaAndColumn()
    {
        var generator = new SqlGenerator<AnnotatedRecord>();
        var parameters = new[] { new FilterCriteria("DisplayName", "Some Guy") };
        var sql = generator.GenerateGetQuery(parameters).Replace(Environment.NewLine, " ");

        Assert.That(sql, Is.EqualTo("select `Id`, `display_name` `DisplayName` from `sales`.`customers` where (`display_name` = @DisplayName);"));
    }

    [Table("SurLaTable")]
    public class SurLaTableRecord
    {
        public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    [Table("ColumnMapped")]
    public class ColumnMappedRecord
    {
        public Int32 Id { get; set; }

        [Column("display_name")]
        public String DisplayName { get; set; } = String.Empty;
    }

    [Table("QuotedColumn")]
    public class QuotedColumnRecord
    {
        [Column("odd`name")]
        public String Name { get; set; } = String.Empty;
    }

    [Table("Unmapped")]
    public class UnmappedRecord(String code)
    {
        public Int32 Id { get; set; }

        public String Name { get; set; } = String.Empty;

        public String Code { get; } = code;

        public String Display => $"{Code}: {Name}";

        [NotMapped]
        public String Ignored { get; set; } = String.Empty;

        [Annotations.NotMapped]
        public String AlsoIgnored { get; set; } = String.Empty;
    }

    [Annotations.Table("customers", Schema = "sales")]
    public class AnnotatedRecord
    {
        public Int32 Id { get; set; }

        [Annotations.Column("display_name")]
        public String DisplayName { get; set; } = String.Empty;
    }
}
