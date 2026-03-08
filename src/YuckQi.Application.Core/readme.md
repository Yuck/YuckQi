# YuckQi.Application.Core

A .NET library for bootstrapping a domain application project. Provides MediatR pipeline behaviors for logging and validation with FluentValidation integration.

## Key Types

### Abstractions

- **`IValidated`** &ndash; response marker with `ValidationResults` for validation behavior

### Behaviors

- **`LoggingBehavior<TRequest, TResponse>`** &ndash; MediatR pipeline behavior that logs request handling start and completion
- **`ValidationBehavior<TRequest, TResponse>`** &ndash; MediatR pipeline behavior that runs FluentValidation validators and short-circuits on error

## Dependencies

- [FluentValidation](https://www.nuget.org/packages/FluentValidation)
- [MediatR](https://www.nuget.org/packages/MediatR)
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)
- [YuckQi.Domain.Validation](https://www.nuget.org/packages/YuckQi.Domain.Validation) (project reference)

## Installation

```shell
dotnet add package YuckQi.Application.Core
```
