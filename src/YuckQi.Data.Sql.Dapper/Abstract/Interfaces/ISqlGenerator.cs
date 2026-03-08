using YuckQi.Data.Filtering;
using YuckQi.Data.Sorting;
using YuckQi.Domain.ValueObjects.Abstract.Interfaces;

namespace YuckQi.Data.Sql.Dapper.Abstract.Interfaces;

public interface ISqlGenerator
{
    String GenerateCountQuery(IReadOnlyCollection<FilterCriteria> parameters);

    String GenerateGetQuery(IReadOnlyCollection<FilterCriteria>? parameters);

    String GenerateSearchQuery(IReadOnlyCollection<FilterCriteria> parameters, IPage page, IOrderedEnumerable<SortCriteria> sort);
}
