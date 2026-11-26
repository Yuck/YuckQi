using NUnit.Framework;
using YuckQi.Domain.ValueObjects;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Domain.UnitTests.ValueObjects;

public class PageGuardTests
{
    [TestCase(1, 10)]
    [TestCase(214748365, 10)]
    [TestCase(2, Int32.MaxValue)]
    [TestCase(Int32.MaxValue, 1)]
    public void ThrowIfInvalid_WithValidPage_DoesNotThrow(Int32 pageNumber, Int32 pageSize)
    {
        var page = new Page(pageNumber, pageSize);

        Assert.DoesNotThrow(() => PageGuard.ThrowIfInvalid(page));
    }

    [TestCase(0, 10, "page.PageNumber")]
    [TestCase(-1, 10, "page.PageNumber")]
    [TestCase(Int32.MinValue, 10, "page.PageNumber")]
    [TestCase(1, 0, "page.PageSize")]
    [TestCase(1, -1, "page.PageSize")]
    [TestCase(1, Int32.MinValue, "page.PageSize")]
    [TestCase(Int32.MaxValue, 10, "page")]
    [TestCase(214748366, 10, "page")]
    [TestCase(3, Int32.MaxValue, "page")]
    [TestCase(Int32.MaxValue, Int32.MaxValue, "page")]
    public void ThrowIfInvalid_WithInvalidPage_ThrowsArgumentOutOfRangeException(Int32 pageNumber, Int32 pageSize, String paramName)
    {
        var page = new Page(pageNumber, pageSize);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => PageGuard.ThrowIfInvalid(page));

        Assert.That(exception.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void ThrowIfInvalid_WithPageChangedByWith_ChecksResult()
    {
        var page = new Page(5000, 10) with { PageSize = 1_000_000, PageNumber = 1 };

        Assert.DoesNotThrow(() => PageGuard.ThrowIfInvalid(page));
    }

    [Test]
    public void ThrowIfInvalid_WithNullPage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => PageGuard.ThrowIfInvalid(null!));
    }

    [Test]
    public void ThrowIfInvalid_WithCustomPage_ChecksSameRules()
    {
        var page = new CustomPage(0, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => PageGuard.ThrowIfInvalid(page));
    }

    private record CustomPage(Int32 PageNumber, Int32 PageSize) : IPage;
}
