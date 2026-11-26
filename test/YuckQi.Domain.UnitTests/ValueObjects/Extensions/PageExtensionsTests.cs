using NUnit.Framework;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Extensions;

namespace YuckQi.Domain.UnitTests.ValueObjects.Extensions;

public class PageExtensionsTests
{
    [TestCase(1, 10, 0)]
    [TestCase(2, 10, 10)]
    [TestCase(3, 25, 50)]
    [TestCase(214748365, 10, 2147483640)]
    [TestCase(2, Int32.MaxValue, Int32.MaxValue)]
    [TestCase(Int32.MaxValue, 1, Int32.MaxValue - 1)]
    public void GetOffset_ReturnsItemsBeforePage(Int32 pageNumber, Int32 pageSize, Int32 offset)
    {
        var page = new Page(pageNumber, pageSize);

        Assert.That(page.GetOffset(), Is.EqualTo(offset));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(Int32.MaxValue, 10)]
    public void GetOffset_WithInvalidPage_ThrowsArgumentOutOfRangeException(Int32 pageNumber, Int32 pageSize)
    {
        var page = new Page(pageNumber, pageSize);

        Assert.Throws<ArgumentOutOfRangeException>(() => page.GetOffset());
    }
}
