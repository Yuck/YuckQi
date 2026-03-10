using NUnit.Framework;
using YuckQi.Data.DocumentDb.DynamoDb.Filtering.Extensions;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.DocumentDb.DynamoDb.UnitTests.Filtering.Extensions;

public class FilterCriteriaExtensionTests
{
    [Test]
    public void ToQueryFilter_WithStringValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", "ABC") };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithInt32Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Id", 42) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithBooleanValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("IsActive", true) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithDecimalValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Price", 9.99m) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithDoubleValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Score", 3.14d) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithInt16Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Code", (Int16) 7) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithInt64Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("BigId", 100L) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithByteValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Flag", (Byte) 1) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithSByteValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("SignedFlag", (SByte)(-1)) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithSingleValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Rate", 1.5f) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithCharValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Grade", 'A') };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithUInt16Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Port", (UInt16) 8080) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithUInt32Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Count", (UInt32) 100) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithUInt64Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("BigCount", (UInt64) 999) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithMultipleParameters_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", "ABC"), new("Id", 42) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithEmptyCollection_ReturnsFilter()
    {
        var parameters = Array.Empty<FilterCriteria>();

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithUnsupportedValueType_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Data", new Object()) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithStringValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", "ABC") };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithMultipleParameters_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", "ABC"), new("Status", "Active") };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithEmptyCollection_ReturnsFilter()
    {
        var parameters = Array.Empty<FilterCriteria>();

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithGreaterThanOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.GreaterThan, 18) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithNotEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Status", FilterOperation.NotEqual, "Deleted") };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithInt32Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Id", 42) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithBooleanValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("IsActive", true) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithDecimalValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Price", 9.99m) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithDoubleValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Score", 3.14d) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithInt16Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Code", (Int16) 7) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithInt64Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("BigId", 100L) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithByteValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Flag", (Byte) 1) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithSByteValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("SignedFlag", (SByte)(-1)) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithSingleValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Rate", 1.5f) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithCharValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Grade", 'A') };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithUInt16Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Port", (UInt16) 8080) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithUInt32Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Count", (UInt32) 100) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithUInt64Value_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("BigCount", (UInt64) 999) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithNullValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", null) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithNullValue_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", null) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithLessThanOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.LessThan, 65) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithLessThanOrEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.LessThanOrEqual, 64) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithGreaterThanOrEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.GreaterThanOrEqual, 18) };

        var result = parameters.ToQueryFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToQueryFilter_WithInOperation_ThrowsArgumentOutOfRangeException()
    {
        var parameters = new FilterCriteria[] { new("Status", FilterOperation.In, "Active") };

        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => parameters.ToQueryFilter();
    }

    [Test]
    public void ToQueryFilter_WithNotEqualOperation_ThrowsArgumentOutOfRangeException()
    {
        var parameters = new FilterCriteria[] { new("Status", FilterOperation.NotEqual, "Deleted") };

        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        void Act() => parameters.ToQueryFilter();
    }

    [Test]
    public void ToScanFilter_WithGreaterThanOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.GreaterThan, 18) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithLessThanOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.LessThan, 65) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithLessThanOrEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.LessThanOrEqual, 64) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithGreaterThanOrEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Age", FilterOperation.GreaterThanOrEqual, 18) };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithInOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Status", FilterOperation.In, "Active") };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ToScanFilter_WithEqualOperation_ReturnsFilter()
    {
        var parameters = new FilterCriteria[] { new("Name", FilterOperation.Equal, "ABC") };

        var result = parameters.ToScanFilter();

        Assert.That(result, Is.Not.Null);
    }
}
