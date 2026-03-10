using System.Collections.Generic;
using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.Abstract.Aspects.Interfaces;

public interface IHasValidationResults
{
    IReadOnlyCollection<Result> ValidationResults { get; set; }
}
