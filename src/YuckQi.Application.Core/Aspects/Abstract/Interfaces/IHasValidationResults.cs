using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.Aspects.Abstract.Interfaces;

public interface IHasValidationResults
{
    IReadOnlyCollection<Result> ValidationResults { get; set; }
}
