using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Domain.ValueObjects;

public record Page(Int32 PageNumber, Int32 PageSize) : IPage;

public record Page<T>(IReadOnlyCollection<T> Items, Int32 TotalCount, Int32 PageNumber, Int32 PageSize) : Page(PageNumber, PageSize), IPage<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Items ?? throw new ArgumentNullException(nameof(Items));

    public virtual Boolean Equals(Page<T>? other)
    {
        return other is not null && base.Equals(other) && TotalCount == other.TotalCount && Items.SequenceEqual(other.Items);
    }

    public override Int32 GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), TotalCount, Items.Count);
    }
}
