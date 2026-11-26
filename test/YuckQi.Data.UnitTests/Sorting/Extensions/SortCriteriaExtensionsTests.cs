using NUnit.Framework;
using YuckQi.Data.Sorting;

namespace YuckQi.Data.UnitTests.Sorting.Extensions;

public class SortCriteriaExtensionsTests
{
    [Test]
    public void OrderBy_WithSingleAscendingCriteria_OrdersByProperty()
    {
        var source = new[] { new SortableEntity(3, "C"), new SortableEntity(1, "A"), new SortableEntity(2, "B") }.AsQueryable();
        var sort = new[] { new SortCriteria("Value", SortOrder.Ascending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void OrderBy_WithSingleDescendingCriteria_OrdersByPropertyDescending()
    {
        var source = new[] { new SortableEntity(1, "A"), new SortableEntity(3, "C"), new SortableEntity(2, "B") }.AsQueryable();
        var sort = new[] { new SortCriteria("Value", SortOrder.Descending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 3, 2, 1 }));
    }

    [Test]
    public void OrderBy_WithMultipleCriteria_UsesFirstCriteriaAsPrimarySort()
    {
        var source = new[] { new SortableEntity(2, "B"), new SortableEntity(1, "B"), new SortableEntity(2, "A"), new SortableEntity(1, "A") }.AsQueryable();
        var sort = new[] { new SortCriteria("Name", SortOrder.Ascending), new SortCriteria("Value", SortOrder.Ascending) };

        var result = source.OrderBy(sort).Select(t => $"{t.Name}{t.Value}").ToList();

        Assert.That(result, Is.EqualTo(new[] { "A1", "A2", "B1", "B2" }));
    }

    [Test]
    public void OrderBy_WithDescendingThenAscendingCriteria_KeepsCallerPriority()
    {
        var source = new[] { new SortableEntity(1, "A"), new SortableEntity(2, "B"), new SortableEntity(1, "B"), new SortableEntity(2, "A") }.AsQueryable();
        var sort = new[] { new SortCriteria("Value", SortOrder.Descending), new SortCriteria("Name", SortOrder.Ascending) };

        var result = source.OrderBy(sort).Select(t => $"{t.Name}{t.Value}").ToList();

        Assert.That(result, Is.EqualTo(new[] { "A2", "B2", "A1", "B1" }));
    }

    [Test]
    public void OrderBy_WithThreeCriteria_AppliesEachInOrder()
    {
        var source = new[]
        {
            new SortableEntity(1, "B", "Y"),
            new SortableEntity(1, "A", "Y"),
            new SortableEntity(2, "A", "X"),
            new SortableEntity(1, "A", "X"),
            new SortableEntity(2, "B", "X")
        }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending), new SortCriteria("Value", SortOrder.Descending), new SortCriteria("Name", SortOrder.Ascending) };

        var result = source.OrderBy(sort).AsEnumerable().Select(t => $"{t.Address?.City}{t.Value}{t.Name}").ToList();

        Assert.That(result, Is.EqualTo(new[] { "X2A", "X2B", "X1A", "Y1A", "Y1B" }));
    }

    [Test]
    public void OrderBy_WithNestedPath_OrdersByNestedProperty()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo"), new SortableEntity(2, "B", "Bergen"), new SortableEntity(3, "C", "Tromso") }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending) };

        var result = source.OrderBy(sort).AsEnumerable().Select(t => t.Address?.City).ToList();

        Assert.That(result, Is.EqualTo(new[] { "Bergen", "Oslo", "Tromso" }));
    }

    [Test]
    public void OrderBy_WithNestedPathDescending_OrdersByNestedPropertyDescending()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo"), new SortableEntity(2, "B", "Bergen"), new SortableEntity(3, "C", "Tromso") }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Descending) };

        var result = source.OrderBy(sort).AsEnumerable().Select(t => t.Address?.City).ToList();

        Assert.That(result, Is.EqualTo(new[] { "Tromso", "Oslo", "Bergen" }));
    }

    [Test]
    public void OrderBy_WithNullIntermediateOnNestedPath_SortsNullsFirst()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo"), new SortableEntity(2, "B", null), new SortableEntity(3, "C", "Bergen") }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Ascending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 2, 3, 1 }));
    }

    [Test]
    public void OrderBy_WithNullIntermediateOnNestedPathDescending_SortsNullsLast()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo"), new SortableEntity(2, "B", null), new SortableEntity(3, "C", "Bergen") }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.City", SortOrder.Descending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 1, 3, 2 }));
    }

    [Test]
    public void OrderBy_WithNullIntermediateBeforeValueTypeMember_SortsNullsFirst()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo"), new SortableEntity(2, "B", null), new SortableEntity(3, "C", "Bergen") }.AsQueryable();
        var sort = new[] { new SortCriteria("Address.Length", SortOrder.Ascending), new SortCriteria("Value", SortOrder.Descending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 2, 1, 3 }));
    }

    [Test]
    public void OrderBy_WithNullNullableValueOnPath_SortsNullsFirst()
    {
        var source = new[] { new SortableEntity(1, "A", "Oslo", 5), new SortableEntity(2, "B", "Oslo"), new SortableEntity(3, "C", "Oslo", 1) }.AsQueryable();
        var sort = new[] { new SortCriteria("Rank.Value", SortOrder.Ascending) };

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 2, 3, 1 }));
    }

    [Test]
    public void OrderBy_WithEmptyCriteria_ReturnsSourceOrder()
    {
        var source = new[] { new SortableEntity(3, "C"), new SortableEntity(1, "A"), new SortableEntity(2, "B") }.AsQueryable();

        var result = source.OrderBy(Array.Empty<SortCriteria>()).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 3, 1, 2 }));
    }

    [Test]
    public void OrderBy_WithUnknownProperty_ThrowsArgumentException()
    {
        var source = Array.Empty<SortableEntity>().AsQueryable();
        var sort = new[] { new SortCriteria("Missing", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(() => source.OrderBy(sort));
    }

    [Test]
    public void OrderBy_WithUnknownNestedProperty_ThrowsArgumentException()
    {
        var source = Array.Empty<SortableEntity>().AsQueryable();
        var sort = new[] { new SortCriteria("Address.Missing", SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(() => source.OrderBy(sort));
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void OrderBy_WithBlankExpression_ThrowsArgumentException(String expression)
    {
        var source = Array.Empty<SortableEntity>().AsQueryable();
        var sort = new[] { new SortCriteria(expression, SortOrder.Ascending) };

        Assert.Throws<ArgumentException>(() => source.OrderBy(sort));
    }

    private sealed class SortableEntity(Int32 value, String name, String? city = "", Int32? rank = null)
    {
        public SortableAddress? Address { get; } = city is null ? null : new SortableAddress(city);

        public String Name { get; } = name;

        public Int32? Rank { get; } = rank;

        public Int32 Value { get; } = value;
    }

    private sealed class SortableAddress(String city)
    {
        public String City { get; } = city;

        public Int32 Length { get; } = city.Length;
    }
}
