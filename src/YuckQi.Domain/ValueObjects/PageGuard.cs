using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Domain.ValueObjects;

public static class PageGuard
{
    public static void ThrowIfInvalid(IPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentOutOfRangeException.ThrowIfLessThan(page.PageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(page.PageSize, 1);

        if (page.PageNumber - 1 > Int32.MaxValue / page.PageSize)
            throw new ArgumentOutOfRangeException(nameof(page), page, $"Page {page.PageNumber} with a page size of {page.PageSize} starts past the largest supported offset ({Int32.MaxValue}).");
    }
}
