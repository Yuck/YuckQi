using System.Linq.Expressions;
using NUnit.Framework;
using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;

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

    private sealed class TestDomainEntity
    {
        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
