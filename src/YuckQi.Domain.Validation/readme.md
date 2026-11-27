# YuckQi.Domain.Validation

A .NET library providing domain validation fundamentals built around a `Result` pattern with FluentValidation integration.

## Key Types

### Result Pattern

- **`Result` / `Result<T>`** &ndash; validation outcome with `Detail` collection and `IsValid` flag; the generic variant carries optional `Content`
  - `new Result<T>(content, detail)` builds a result with content; `Result<T>.Failure(detail)` and `Result<T>.NotFound(identifier)` build an invalid one with `default` content
  - `Result<T>.Failure` throws `ArgumentNullException` for a `null` detail and `ArgumentException` when the detail would make a valid result (empty, or only warnings)
  - The constructors copy `detail`, so later changes to the caller's collection do not change the result; a `null` entry in `detail` throws `ArgumentException`
  - `IsValid` is `true` only when every detail is a `ResultType.Warning`; `Error`, `Unknown`, and undefined types make a result invalid
  - Equality compares `Detail` item by item, in order, so two results with equal details are equal even when they hold different collection instances
- **`IResult` / `IResult<T>`** &ndash; interfaces for the result types
- **`ResultDetail`** &ndash; a single validation issue with message, code, property name, and type
- **`ResultMessage`** &ndash; strongly typed validation message with implicit `String` conversion; includes `NotFound` factory
- **`ResultCode`** &ndash; strongly typed validation code with predefined values (`InvalidRequestDetail`, `NotFound`)
- **`ResultType`** &ndash; enum (`Unknown`, `Warning`, `Error`)

Upgrading to 10.5.0:

- The `Result<T>(ResultDetail)` and `Result<T>(IReadOnlyCollection<ResultDetail>)` constructors are removed. Replace every `new Result<T>(detail)` that passes only detail with `Result<T>.Failure(detail)`. Most such calls stop compiling, but not all: when a detail or detail collection converts to `T` (for example `Result<Object>`, `Result<ResultDetail>`, `Result<IEnumerable<ResultDetail>>`, or `Result<IReadOnlyCollection<ResultDetail>>`), the call still compiles. It now uses the content constructor, so the detail becomes `Content` and the result is valid instead of failed. `new Result<T>(null)` now compiles and creates a result with `null` content.
- `new Result(detail)` and `new Result<T>(content, detail)` copy `detail` instead of keeping the caller's collection, and throw `ArgumentException` when it contains a `null` entry. Before, a `null` entry made `IsValid` throw `NullReferenceException`.
- `IsValid` treats `ResultType.Unknown` (the enum's zero value, and the default for `ApiResultDetail.Type` and for a detail read from JSON with no `Type`) as invalid, along with any undefined `ResultType` value. Before, only `ResultType.Error` made a result invalid.
- `Result` and `Result<T>` equality compares `Detail` by sequence instead of by reference. A `Result` and a `Result<T>` are never equal, even with the same detail.

### FluentValidation Integration

- **`AbstractValidatorExtensions`** &ndash; `GetResult` extension methods (synchronous, and asynchronous with a `CancellationToken`) that execute a FluentValidation `IValidator<T>` / `AbstractValidator<T>` and return a `Result<T>`. Each `ValidationFailure` becomes a `ResultDetail`: `Severity.Error` maps to `ResultType.Error`, `Severity.Warning` and `Severity.Info` map to `ResultType.Warning`, and the failure's `ErrorCode` becomes the `ResultCode` (falling back to `ResultCode.InvalidRequestDetail` when it is empty). `Content` is kept unless a detail is an error.

Upgrading to 10.5.0:

- Warning and info severity failures produce `ResultType.Warning` details instead of `ResultType.Error`. A result with only warnings is valid and keeps `Content`; before, it was invalid and `Content` was `default`.
- `ResultDetail.Code` is the failure's `ErrorCode` (from `.WithErrorCode(...)`, or FluentValidation's default such as `NotEmptyValidator`) instead of always `ResultCode.InvalidRequestDetail`. `InvalidRequestDetail` is used only when a failure has no error code, so checks such as `HasResultCode(ResultCode.InvalidRequestDetail)` on validator output no longer match most failures.

### API Response Models

- **`ApiResult` / `ApiResult<T>`** &ndash; API response wrappers with `Detail` collection
- **`ApiResultDetail`** &ndash; serializable detail model with `Code`, `Message`, `Property`, and `Type`

Upgrading to 10.5.0:

- `ApiResult` and `ApiResult<T>` equality compares `Detail` item by item, in order, instead of by reference. An `ApiResult` and an `ApiResult<T>` are never equal, even with the same detail.

### JSON Support

The result types carry no serialization attributes. To round-trip them through `System.Text.Json`, register the converters with `AddResultJsonConverters`:

```csharp
var options = new JsonSerializerOptions().AddResultJsonConverters();
var json = JsonSerializer.Serialize(result, options);
var copy = JsonSerializer.Deserialize<Result<Order>>(json, options);
```

In ASP.NET Core:

```csharp
builder.Services.ConfigureHttpJsonOptions(t => t.SerializerOptions.AddResultJsonConverters());
```

- **`JsonSerializerOptionsExtensions`** &ndash; `AddResultJsonConverters` adds all of the converters below to a `JsonSerializerOptions` and returns it
- **`ResultCodeJsonConverter`** / **`ResultMessageJsonConverter`** &ndash; write `ResultCode` and `ResultMessage` as plain strings and read them back
- **`ResultDetailJsonConverter`** &ndash; writes a `ResultDetail` in the `ApiResultDetail` shape (`Code`, `Message`, `Property`, `Type`) and reads it back; a missing `Message` becomes an empty `ResultMessage`
- **`ResultJsonConverter`** / **`ResultJsonConverter<T>`** / **`ResultJsonConverterFactory`** (in `JsonConverters.Factories`) &ndash; write a `Result` as `Detail` and `IsValid`, and a `Result<T>` as `Content`, `Detail`, and `IsValid`, and read them back through their constructors; `IsValid` is recomputed from `Detail` on read, and a `null` entry in `Detail` throws `JsonException`
- The converters honor the serializer's naming policy, case-insensitive matching, ignore conditions, unmapped member handling, and converters registered for `String`, `ResultType` (such as `JsonStringEnumConverter`), and `T`
- The converters work with source-generated `JsonSerializerContext` metadata and need no reflection; include the result types in the context, for example `[JsonSerializable(typeof(Result<Order>))]`

Upgrading to 10.5.0:

- `ResultDetailJsonConverter`, `ResultJsonConverter`, `ResultJsonConverter<T>`, `ResultJsonConverterFactory`, and `AddResultJsonConverters` are new. Without them, `ResultCode` and `ResultMessage` still serialize as `{}`, and `ResultDetail` and `Result<T>` cannot be deserialized.
- The implicit conversions between `String` and `ResultCode` / `ResultMessage` pass `null` through: a null `ResultCode` or `ResultMessage` converts to a null `String`, and a null `String` to a null `ResultCode` or `ResultMessage`, instead of throwing `NullReferenceException` or creating an instance that wraps `null`. Both conversions are now declared nullable (`String?`, `ResultCode?`, `ResultMessage?`).
- `new ResultCode(null)` and `new ResultMessage(null)` throw `ArgumentNullException`.
- `ResultDetail` constructors accept a null message (`String?` or `ResultMessage?`) and store an empty `ResultMessage`, so `Message` is never null. `GetResult` therefore maps a FluentValidation failure with a null `ErrorMessage` to an empty message.

## Dependencies

- [FluentValidation](https://www.nuget.org/packages/FluentValidation)

## Installation

```shell
dotnet add package YuckQi.Domain.Validation
```
