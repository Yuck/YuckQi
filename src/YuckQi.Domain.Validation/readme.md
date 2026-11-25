# YuckQi.Domain.Validation

A .NET library providing domain validation fundamentals built around a `Result` pattern with FluentValidation integration.

## Key Types

### Result Pattern

- **`Result` / `Result<T>`** &ndash; validation outcome with `Detail` collection and `IsValid` flag; the generic variant carries optional `Content`
- **`IResult` / `IResult<T>`** &ndash; interfaces for the result types
- **`ResultDetail`** &ndash; a single validation issue with message, code, property name, and type
- **`ResultMessage`** &ndash; strongly typed validation message with implicit `String` conversion; includes `NotFound` factory
- **`ResultCode`** &ndash; strongly typed validation code with predefined values (`InvalidRequestDetail`, `NotFound`)
- **`ResultType`** &ndash; enum (`Unknown`, `Warning`, `Error`)

### FluentValidation Integration

- **`AbstractValidatorExtensions`** &ndash; `GetResult` extension methods (synchronous, and asynchronous with a `CancellationToken`) that execute a FluentValidation `IValidator<T>` / `AbstractValidator<T>` and return a `Result<T>`. Each `ValidationFailure` becomes a `ResultDetail`: `Severity.Error` maps to `ResultType.Error`, `Severity.Warning` and `Severity.Info` map to `ResultType.Warning`, and the failure's `ErrorCode` becomes the `ResultCode` (falling back to `ResultCode.InvalidRequestDetail` when it is empty). `Content` is kept unless a detail is an error.

Upgrading to 10.5.0:

- Warning and info severity failures produce `ResultType.Warning` details instead of `ResultType.Error`. A result with only warnings is valid and keeps `Content`; before, it was invalid and `Content` was `default`.
- `ResultDetail.Code` is the failure's `ErrorCode` (from `.WithErrorCode(...)`, or FluentValidation's default such as `NotEmptyValidator`) instead of always `ResultCode.InvalidRequestDetail`. `InvalidRequestDetail` is used only when a failure has no error code, so checks such as `HasResultCode(ResultCode.InvalidRequestDetail)` on validator output no longer match most failures.

### API Response Models

- **`ApiResult` / `ApiResult<T>`** &ndash; API response wrappers with `Detail` collection
- **`ApiResultDetail`** &ndash; serializable detail model with `Code`, `Message`, `Property`, and `Type`

### JSON Support

- **`ResultCodeJsonConverter`** / **`ResultMessageJsonConverter`** &ndash; `System.Text.Json` converters for `ResultCode` and `ResultMessage`

## Dependencies

- [FluentValidation](https://www.nuget.org/packages/FluentValidation)

## Installation

```shell
dotnet add package YuckQi.Domain.Validation
```
