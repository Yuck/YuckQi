using System.Linq.Expressions;
using NUnit.Framework;
using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Filtering.Extensions;

namespace YuckQi.Data.UnitTests.Extensions;

public class PredicateFilterExtensionTests
{
    [Test]
    public void Predicate_WithSingleEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test";

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filters.First().Value, Is.EqualTo("test"));
        });
    }

    [Test]
    public void Predicate_WithAndAlso_IsConvertedToMultipleFilterExpressions()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test" && t.Identifier > 10;

        var filters = predicate.ToFilterExpressions().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(2));
            Assert.That(filters[0].Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filters[0].Value, Is.EqualTo("test"));
            Assert.That(filters[1].Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(filters[1].Value, Is.EqualTo(10));
        });
    }

    [Test]
    public void Predicate_WithNotEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name != "exclude";

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.NotEqual));
            Assert.That(filters.First().Value, Is.EqualTo("exclude"));
        });
    }

    [Test]
    public void Predicate_WithLessThan_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier < 100;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.LessThan));
            Assert.That(filters.First().Value, Is.EqualTo(100));
        });
    }

    [Test]
    public void Predicate_WithLessThanOrEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier <= 50;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.LessThanOrEqual));
            Assert.That(filters.First().Value, Is.EqualTo(50));
        });
    }

    [Test]
    public void Predicate_WithGreaterThanOrEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier >= 1;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.GreaterThanOrEqual));
            Assert.That(filters.First().Value, Is.EqualTo(1));
        });
    }

    [Test]
    public void Predicate_WithUnsupportedBinaryExpression_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "a" || t.Name == "b";

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_ToFilterExpressionsThenToFilterCriteria_ProducesMatchingFilterCriteria()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test" && t.Identifier > 10;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(2));
            Assert.That(criteria[0].FieldName, Is.EqualTo("Name"));
            Assert.That(criteria[0].Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria[0].Value, Is.EqualTo("test"));
            Assert.That(criteria[1].FieldName, Is.EqualTo("Identifier"));
            Assert.That(criteria[1].Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria[1].Value, Is.EqualTo(10));
        });
    }

    private sealed class TestDomainEntity
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
