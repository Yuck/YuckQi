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

- **`AbstractValidatorExtensions`** &ndash; `GetResult` and `GetResultAsync` extension methods that execute a `FluentValidation.AbstractValidator<T>` and return a `Result<T>`

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
