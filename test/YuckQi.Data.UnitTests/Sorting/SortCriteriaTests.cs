using NUnit.Framework;
using YuckQi.Data.Sorting;

namespace YuckQi.Data.UnitTests.Sorting;

public class SortCriteriaTests
{
    [Test]
    public void Constructor_WithExpressionAndOrder_SetsProperties()
    {
        var criteria = new SortCriteria("Name", SortOrder.Descending);

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Expression, Is.EqualTo("Name"));
            Assert.That(criteria.Order, Is.EqualTo(SortOrder.Descending));
        });
    }

    [Test]
    public void Constructor_WithAscendingOrder_IsValid()
    {
        var criteria = new SortCriteria("Identifier", SortOrder.Ascending);

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Expression, Is.EqualTo("Identifier"));
            Assert.That(criteria.Order, Is.EqualTo(SortOrder.Ascending));
        });
    }

    [Test]
    [TestCase(SortOrder.Ascending)]
    [TestCase(SortOrder.Descending)]
    public void Constructor_WithEachSortOrder_IsValid(SortOrder order)
    {
        var criteria = new SortCriteria("Field", order);

        Assert.That(criteria.Order, Is.EqualTo(order));
    }
}
