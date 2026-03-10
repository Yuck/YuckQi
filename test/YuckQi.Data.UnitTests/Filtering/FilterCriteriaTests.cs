using NUnit.Framework;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.UnitTests.Filtering;

public class FilterCriteriaTests
{
    [Test]
    public void Constructor_WithFieldNameAndValue_SetsEqualOperation()
    {
        var criteria = new FilterCriteria("Name", "ABC");

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Name"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria.Value, Is.EqualTo("ABC"));
        });
    }

    [Test]
    public void Constructor_WithFieldNameValueAndOperation_SetsAllProperties()
    {
        var criteria = new FilterCriteria("Age", FilterOperation.GreaterThan, 18);

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Age"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria.Value, Is.EqualTo(18));
        });
    }

    [Test]
    public void Constructor_WithNullValue_IsValid()
    {
        var criteria = new FilterCriteria("Optional", (Object?) null);

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Optional"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria.Value, Is.Null);
        });
    }

    [Test]
    [TestCase(FilterOperation.Equal)]
    [TestCase(FilterOperation.NotEqual)]
    [TestCase(FilterOperation.GreaterThan)]
    [TestCase(FilterOperation.GreaterThanOrEqual)]
    [TestCase(FilterOperation.LessThan)]
    [TestCase(FilterOperation.LessThanOrEqual)]
    [TestCase(FilterOperation.In)]
    public void Constructor_WithEachFilterOperation_IsValid(FilterOperation operation)
    {
        var criteria = new FilterCriteria("Field", operation, 42);

        Assert.That(criteria.Operation, Is.EqualTo(operation));
    }
}
