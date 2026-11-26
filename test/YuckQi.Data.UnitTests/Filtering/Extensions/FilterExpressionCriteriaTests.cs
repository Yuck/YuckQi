using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Filtering.Extensions;
using YuckQi.Data.Resolvers;
using YuckQi.Data.Sorting;
using YuckQi.Data.Sorting.Extensions;

namespace YuckQi.Data.UnitTests.Filtering.Extensions;

public class FilterExpressionCriteriaTests
{
    [Test]
    public void FilterExpressions_ToFilterCriteria_Single_IsValid()
    {
        var filters = new[] { new FilterExpression<TestDomainEntity>(t => t.Name, "ABC") };

        var criteria = filters.ToFilterCriteria<TestDomainEntity, TestRecord>();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(1));
            Assert.That(criteria.First().FieldName, Is.EqualTo("Name"));
            Assert.That(criteria.First().Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria.First().Value, Is.EqualTo("ABC"));
        });
    }

    [Test]
    public void FilterExpressions_ToFilterCriteria_WithOperation_IsValid()
    {
        var filters = new[] { new FilterExpression<TestDomainEntity>(t => t.Identifier, FilterOperation.GreaterThan, 10) };

        var resolver = new DefaultPropertyNameResolver().WithMapping<TestDomainEntity, TestRecord>(t => t.Identifier, t => t.Id);

        var criteria = filters.ToFilterCriteria<TestDomainEntity, TestRecord>(resolver);

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(1));
            Assert.That(criteria.First().FieldName, Is.EqualTo("Id"));
            Assert.That(criteria.First().Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria.First().Value, Is.EqualTo(10));
        });
    }

    [Test]
    public void FilterExpressions_ToFilterCriteria_NestedProperty_IsValid()
    {
        var filters = new[] { new FilterExpression<TestDomainEntity>(t => t.Nested.Value, "XYZ") };

        var criteria = filters.ToFilterCriteria<TestDomainEntity, TestRecord>();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(1));
            Assert.That(criteria.First().FieldName, Is.EqualTo("Nested.Value"));
            Assert.That(criteria.First().Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria.First().Value, Is.EqualTo("XYZ"));
        });
    }

    [Test]
    public void SortExpressions_ToSortCriteria_SingleAscending_IsValid()
    {
        var sorts = new[] { new SortExpression<TestDomainEntity>(t => t.Identifier) };

        var resolver = new DefaultPropertyNameResolver().WithMapping<TestDomainEntity, TestRecord>(t => t.Identifier, t => t.Id);

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>(resolver).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(1));
            Assert.That(criteria.First().Expression, Is.EqualTo("Id"));
            Assert.That(criteria.First().Order, Is.EqualTo(SortOrder.Ascending));
        });
    }

    [Test]
    public void SortExpressions_ToSortCriteria_SingleDescending_IsValid()
    {
        var sorts = new[] { new SortExpression<TestDomainEntity>(t => t.Name, SortOrder.Descending) };

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(1));
            Assert.That(criteria.First().Expression, Is.EqualTo("Name"));
            Assert.That(criteria.First().Order, Is.EqualTo(SortOrder.Descending));
        });
    }

    [Test]
    public void SortExpressions_ToSortCriteria_Multiple_IsValid()
    {
        var sorts = new[]
        {
            new SortExpression<TestDomainEntity>(t => t.Name, SortOrder.Descending),
            new SortExpression<TestDomainEntity>(t => t.Identifier, SortOrder.Ascending)
        };

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>(null).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(2));
            Assert.That(criteria[0].Expression, Is.EqualTo("Name"));
            Assert.That(criteria[0].Order, Is.EqualTo(SortOrder.Descending));
            Assert.That(criteria[1].Expression, Is.EqualTo("Identifier"));
            Assert.That(criteria[1].Order, Is.EqualTo(SortOrder.Ascending));
        });
    }

    [Test]
    public void SortExpressions_ToSortCriteria_PreservesInputOrderRegardlessOfDirection()
    {
        var sorts = new[]
        {
            new SortExpression<TestDomainEntity>(t => t.Identifier, SortOrder.Descending),
            new SortExpression<TestDomainEntity>(t => t.Name, SortOrder.Ascending),
            new SortExpression<TestDomainEntity>(t => t.Nested.Value, SortOrder.Descending)
        };

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>();

        Assert.That(criteria, Is.EqualTo(new[] { new SortCriteria("Identifier", SortOrder.Descending), new SortCriteria("Name", SortOrder.Ascending), new SortCriteria("Nested.Value", SortOrder.Descending) }));
    }

    [Test]
    public void SortExpressions_ToSortCriteria_NestedPath_UsesDottedPath()
    {
        var sorts = new[] { new SortExpression<TestDomainEntity>(t => t.Nested.Value) };

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>();

        Assert.That(criteria.Single(), Is.EqualTo(new SortCriteria("Nested.Value", SortOrder.Ascending)));
    }

    [Test]
    public void SortExpressions_ToSortCriteria_IsNotDeferred()
    {
        var sorts = new List<SortExpression<TestDomainEntity>> { new(t => t.Name) };

        var criteria = sorts.ToSortCriteria<TestDomainEntity, TestRecord>();
        sorts.Add(new SortExpression<TestDomainEntity>(t => t.Identifier));

        Assert.That(criteria, Has.Count.EqualTo(1));
    }

    [Test]
    public void FilterExpressions_ToFilterCriteria_WithNullResolver_UsesDefaultPropertyNames()
    {
        var filters = new[] { new FilterExpression<TestDomainEntity>(t => t.Name, "X") };

        var criteria = filters.ToFilterCriteria<TestDomainEntity, TestRecord>(null);

        Assert.That(criteria.First().FieldName, Is.EqualTo("Name"));
    }

    private sealed class TestDomainEntity
    {
        public Int32 Identifier { get; set; }

        public NestedDomainEntity Nested { get; set; } = new();

        public String Name { get; set; } = String.Empty;
    }

    private sealed class NestedDomainEntity
    {
        public String Value { get; set; } = String.Empty;
    }

    private sealed class TestRecord
    {
        public Int32 Id { get; set; }

        public NestedRecord Nested { get; set; } = new();

        public String Name { get; set; } = String.Empty;
    }

    private sealed class NestedRecord
    {
        public String Value { get; set; } = String.Empty;
    }
}
