using NUnit.Framework;
using YuckQi.Data.Sorting;

namespace YuckQi.Data.UnitTests.Sorting.Extensions;

public class SortCriteriaExtensionsTests
{
    [Test]
    public void OrderBy_WithSingleAscendingCriteria_OrdersByProperty()
    {
        var source = new[] { new SortableEntity(3, "C"), new SortableEntity(1, "A"), new SortableEntity(2, "B") }.AsQueryable();
        var sort = new[] { new SortCriteria("Value", SortOrder.Ascending) }.OrderBy(t => t.Order);

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void OrderBy_WithSingleDescendingCriteria_OrdersByPropertyDescending()
    {
        var source = new[] { new SortableEntity(1, "A"), new SortableEntity(3, "C"), new SortableEntity(2, "B") }.AsQueryable();
        var sort = new[] { new SortCriteria("Value", SortOrder.Descending) }.OrderBy(t => t.Order);

        var result = source.OrderBy(sort).Select(t => t.Value).ToList();

        Assert.That(result, Is.EqualTo(new[] { 3, 2, 1 }));
    }

    [Test]
    public void OrderBy_WithMultipleCriteria_AppliesOrderByThenBy()
    {
        var source = new[]
        {
            new SortableEntity(2, "B"),
            new SortableEntity(1, "B"),
            new SortableEntity(2, "A"),
            new SortableEntity(1, "A")
        }.AsQueryable();
        var sort = new[]
        {
            new SortCriteria("Name", SortOrder.Ascending),
            new SortCriteria("Value", SortOrder.Ascending)
        }.OrderBy(t => t.Order);

        var result = source.OrderBy(sort).ToList().Select(t => (t.Name, t.Value)).ToList();

        Assert.That(result, Has.Count.EqualTo(4));
        Assert.That(result.Select(t => t.Name).Distinct().OrderBy(t => t).ToList(), Is.EqualTo(new[] { "A", "B" }));
        Assert.That(result.Select(t => t.Value).Distinct().OrderBy(t => t).ToList(), Is.EqualTo(new[] { 1, 2 }));
    }

    private sealed class SortableEntity
    {
        public SortableEntity(Int32 value, String name)
        {
            Value = value;
            Name = name;
        }

        public Int32 Value { get; }

        public String Name { get; }
    }
}
