using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Domain.ValueObjects.Extensions;

public static class PageExtensions
{
    public static Int32 GetOffset(this IPage page)
    {
        PageGuard.ThrowIfInvalid(page);

        return (page.PageNumber - 1) * page.PageSize;
    }
}
