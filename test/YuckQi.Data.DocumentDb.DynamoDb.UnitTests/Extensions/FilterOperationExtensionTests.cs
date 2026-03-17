using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Filtering.Extensions;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Extensions;

public class FilterOperationExtensionTests
{
    [Test]
    public void ToComparisonOperator_Equal_ReturnsEQ()
    {
        var result = FilterOperation.Equal.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.EQ));
    }

    [Test]
    public void ToComparisonOperator_GreaterThan_ReturnsGT()
    {
        var result = FilterOperation.GreaterThan.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.GT));
    }

    [Test]
    public void ToComparisonOperator_GreaterThanOrEqual_ReturnsGE()
    {
        var result = FilterOperation.GreaterThanOrEqual.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.GE));
    }

    [Test]
    public void ToComparisonOperator_In_ReturnsIN()
    {
        var result = FilterOperation.In.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.IN));
    }

    [Test]
    public void ToComparisonOperator_LessThan_ReturnsLT()
    {
        var result = FilterOperation.LessThan.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.LT));
    }

    [Test]
    public void ToComparisonOperator_LessThanOrEqual_ReturnsLE()
    {
        var result = FilterOperation.LessThanOrEqual.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.LE));
    }

    [Test]
    public void ToComparisonOperator_NotEqual_ReturnsNE()
    {
        var result = FilterOperation.NotEqual.ToComparisonOperator();

        Assert.That(result, Is.EqualTo(ComparisonOperator.NE));
    }

    [Test]
    public void ToComparisonOperator_InvalidOperation_ThrowsArgumentOutOfRangeException()
    {
        var operation = (FilterOperation) 999;

        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => operation.ToComparisonOperator();
    }

    [Test]
    public void ToQueryOperator_Equal_ReturnsEqual()
    {
        var result = FilterOperation.Equal.ToQueryOperator();

        Assert.That(result, Is.EqualTo(QueryOperator.Equal));
    }

    [Test]
    public void ToQueryOperator_GreaterThan_ReturnsGreaterThan()
    {
        var result = FilterOperation.GreaterThan.ToQueryOperator();

        Assert.That(result, Is.EqualTo(QueryOperator.GreaterThan));
    }

    [Test]
    public void ToQueryOperator_GreaterThanOrEqual_ReturnsGreaterThanOrEqual()
    {
        var result = FilterOperation.GreaterThanOrEqual.ToQueryOperator();

        Assert.That(result, Is.EqualTo(QueryOperator.GreaterThanOrEqual));
    }

    [Test]
    public void ToQueryOperator_LessThan_ReturnsLessThan()
    {
        var result = FilterOperation.LessThan.ToQueryOperator();

        Assert.That(result, Is.EqualTo(QueryOperator.LessThan));
    }

    [Test]
    public void ToQueryOperator_LessThanOrEqual_ReturnsLessThanOrEqual()
    {
        var result = FilterOperation.LessThanOrEqual.ToQueryOperator();

        Assert.That(result, Is.EqualTo(QueryOperator.LessThanOrEqual));
    }

    [Test]
    public void ToQueryOperator_In_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => FilterOperation.In.ToQueryOperator();
    }

    [Test]
    public void ToQueryOperator_NotEqual_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => FilterOperation.NotEqual.ToQueryOperator();
    }

    [Test]
    public void ToQueryOperator_InvalidOperation_ThrowsArgumentOutOfRangeException()
    {
        var operation = (FilterOperation) 999;

        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => operation.ToQueryOperator();
    }
}
