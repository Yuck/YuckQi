using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.Behaviors.Validation;

public sealed record ValidationResponse : IHasValidationResults
{
    public IReadOnlyCollection<Result> ValidationResults { get; set; } = [];
}

public sealed record ValidationResponse<T> : IHasValidationResults
{
    public IReadOnlyCollection<Result> ValidationResults { get; set; } = [];
    public T? Value { get; set; }
}
