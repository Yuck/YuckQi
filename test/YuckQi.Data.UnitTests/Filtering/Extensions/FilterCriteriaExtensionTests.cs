using System.Numerics;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Filtering.Extensions;

namespace YuckQi.Data.UnitTests.Filtering.Extensions;

public class FilterCriteriaExtensionTests
{
    private static readonly Object? NullObject = null;

    [Test]
    public void Object_WithSingleProperty_IsValid()
    {
        var parameters = new { thing = "a test" };
        var filters = parameters.ToFilterCollection();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Value, Is.EqualTo("a test"));
        });
    }

    [Test]
    public void Object_WithSingleNullProperty_IsValid()
    {
        var parameters = new { thing = (Int32?) null };
        var filters = parameters.ToFilterCollection();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Value, Is.Null);
        });
    }

    [Test]
    public void Object_WithNoProperties_Throws()
    {
        var parameters = new { };

        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void Object_WithMultipleProperties_IsValid()
    {
        var parameters = new { thing = "a test", other = 1234.56M };
        var filters = parameters.ToFilterCollection();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(2));
            Assert.That(filters.First().Value, Is.EqualTo("a test"));
            Assert.That(filters.Last().Value, Is.EqualTo(1234.56M));
        });
    }

    [Test]
    public void Null_Throws()
    {
        var parameters = NullObject;

        Assert.Throws<ArgumentNullException>(() => parameters.ToFilterCollection());
    }

    [Test]
    [TestCaseSource(nameof(ScalarValues))]
    public void Scalar_Throws(Object parameters)
    {
        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void Enumerable_OfNonFilterCriteria_Throws()
    {
        var parameters = new List<Int32> { 1, 2, 3 };

        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void Object_WithIndexer_SkipsIndexer()
    {
        var parameters = new Indexed { Name = "ABC" };
        var filters = parameters.ToFilterCollection();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().FieldName, Is.EqualTo(nameof(Indexed.Name)));
            Assert.That(filters.First().Value, Is.EqualTo("ABC"));
        }
    }

    [Test]
    public void Dictionary_OfObjectValues_UsesKeysAndValues()
    {
        var parameters = new Dictionary<String, Object?> { ["Name"] = "ABC", ["Optional"] = null };
        var filters = parameters.ToFilterCollection();

        Assert.That(filters, Is.EqualTo(new[] { new FilterCriteria("Name", "ABC"), new FilterCriteria("Optional", (Object?) null) }));
    }

    [Test]
    public void Dictionary_OfTypedValues_UsesKeysAndValues()
    {
        var parameters = new Dictionary<String, Int32> { ["Identifier"] = 5, ["TenantId"] = 7 };
        var filters = parameters.ToFilterCollection();

        Assert.That(filters, Is.EqualTo(new[] { new FilterCriteria("Identifier", 5), new FilterCriteria("TenantId", 7) }));
    }

    [Test]
    public void Dictionary_WhenEmpty_Throws()
    {
        var parameters = new Dictionary<String, Object?>();

        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void FilterCriteria_WhenEmpty_Throws()
    {
        var parameters = new List<FilterCriteria>();

        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void Dictionary_WithNonStringKeys_Throws()
    {
        var parameters = new Dictionary<Int32, String> { [1] = "ABC" };

        Assert.Throws<ArgumentException>(() => parameters.ToFilterCollection());
    }

    [Test]
    public void FilterCriteria_Single_IsValid()
    {
        var parameters = new FilterCriteria("SomeField", 1234);
        var filters = parameters.ToFilterCollection();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Value, Is.EqualTo(1234));
        });
    }

    [Test]
    public void FilterCriteria_Multiple_IsValid()
    {
        var parameters = new List<FilterCriteria> { new("SomeField", 1234), new("AnotherOne", "hi") };
        var filters = parameters.ToFilterCollection();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(2));
            Assert.That(filters.First().Value, Is.EqualTo(1234));
            Assert.That(filters.Last().Value, Is.EqualTo("hi"));
        });
    }

    private static IEnumerable<Object> ScalarValues()
    {
        yield return 5;
        yield return 5L;
        yield return "ABC";
        yield return 12.5M;
        yield return Guid.NewGuid();
        yield return DateTime.UtcNow;
        yield return DateTimeOffset.UtcNow;
        yield return new DateOnly(2026, 11, 25);
        yield return TimeSpan.FromMinutes(1);
        yield return FilterOperation.Equal;
        yield return new BigInteger(5);
        yield return new Uri("https://example.com/");
        yield return new Version(1, 2, 3);
        yield return new KeyValuePair<String, Object?>("Name", "ABC");
        yield return Tuple.Create(1, "ABC");
    }

    private class Indexed
    {
        public String Name { get; init; } = String.Empty;

        public String this[Int32 index] => Name;
    }
}
