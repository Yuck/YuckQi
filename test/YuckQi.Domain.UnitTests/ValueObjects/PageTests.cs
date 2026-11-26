using NUnit.Framework;
using YuckQi.Domain.ValueObjects;

namespace YuckQi.Domain.UnitTests.ValueObjects;

public class PageTests
{
    [Test]
    public void Page_Properties_HaveExpectedValues()
    {
        var page = new Page(1, 10);

        Assert.That(page.PageNumber, Is.EqualTo(1));
        Assert.That(page.PageSize, Is.EqualTo(10));
    }

    [Test]
    public void PageOfString_Properties_HaveExpectedValues()
    {
        var items = new HashSet<String> { "abc", "def", "ghi" };
        var page = new Page<String>(items, items.Count, 1, 10);

        Assert.That(page.Items, Contains.Item("abc"));
        Assert.That(page.Items, Contains.Item("def"));
        Assert.That(page.Items, Contains.Item("ghi"));
        Assert.That(page.PageNumber, Is.EqualTo(1));
        Assert.That(page.PageSize, Is.EqualTo(10));
        Assert.That(page.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public void PageOfString_WithNullItems_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => _ = new Page<String>(null!, 0, 1, 10));

        Assert.That(exception.ParamName, Is.EqualTo(nameof(Page<String>.Items)));
    }

    [Test]
    public void PageOfString_WithSameItemsInDifferentCollections_IsEqual()
    {
        var first = new Page<String>(new List<String> { "abc", "def" }, 2, 1, 10);
        var second = new Page<String>(new[] { "abc", "def" }, 2, 1, 10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }
    }

    [Test]
    public void PageOfString_WithItemsInDifferentOrder_IsNotEqual()
    {
        var first = new Page<String>(["abc", "def"], 2, 1, 10);
        var second = new Page<String>(["def", "abc"], 2, 1, 10);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void PageOfString_WithDifferentItems_IsNotEqual()
    {
        var first = new Page<String>(["abc", "def"], 2, 1, 10);
        var second = new Page<String>(["abc", "ghi"], 2, 1, 10);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void PageOfString_WithDifferentTotalCount_IsNotEqual()
    {
        var first = new Page<String>(["abc"], 1, 1, 10);
        var second = new Page<String>(["abc"], 2, 1, 10);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void PageOfString_WithDifferentPageNumber_IsNotEqual()
    {
        var first = new Page<String>(["abc"], 1, 1, 10);
        var second = new Page<String>(["abc"], 1, 2, 10);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void PageOfString_ComparedWithPage_IsNotEqual()
    {
        Page first = new Page<String>([], 0, 1, 10);
        var second = new Page(1, 10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(second, Is.Not.EqualTo(first));
        }
    }

    [Test]
    public void PageOfString_WithChangedItems_IsEqualToPageWithThoseItems()
    {
        var page = new Page<String>(["abc"], 1, 1, 10);

        var changed = page with { Items = ["def"] };

        Assert.That(changed, Is.EqualTo(new Page<String>(["def"], 1, 1, 10)));
    }
}
