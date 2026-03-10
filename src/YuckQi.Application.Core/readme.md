# YuckQi.Application.Core

A .NET library for bootstrapping a domain application project. Provides MediatR pipeline behaviors for logging and validation with FluentValidation integration.

## Key Types

### Abstractions

- **`IHasCacheInvalidationKeys`** &ndash; aspect marker for MediatR *response* types that trigger cache invalidation; exposes `CacheKeys` (`IReadOnlySet<String>`) to remove after the handler runs
- **`IHasCacheKey`** &ndash; aspect marker for cacheable MediatR requests with cache key and expiration
- **`IValidated`** &ndash; response marker with `ValidationResults` for validation behavior

### Behaviors

Pipeline behaviors are organized by purpose in subfolders and namespaces:

**Caching** (`YuckQi.Application.Core.Behaviors.Caching`)

- **`DistributedCacheInvalidationBehavior<TRequest, TResponse>`** &ndash; Removes keys from `IDistributedCache` after the handler runs when `TResponse` implements `IHasCacheInvalidationKeys`
- **`DistributedCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IDistributedCache` to cache responses for cacheable requests; configuration via `DistributedCachingBehaviorOptions` record (same file)
- **`MemoryCacheInvalidationBehavior<TRequest, TResponse>`** &ndash; Removes keys from `IMemoryCache` after the handler runs when `TResponse` implements `IHasCacheInvalidationKeys`
- **`MemoryCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IMemoryCache` to cache responses for cacheable requests; configuration via `MemoryCachingBehaviorOptions` record (same file)

**Logging** (`YuckQi.Application.Core.Behaviors.Logging`)

- **`LoggingBehavior<TRequest, TResponse>`** &ndash; Logs request handling start and completion

**Validation** (`YuckQi.Application.Core.Behaviors.Validation`)

- **`ValidationBehavior<TRequest, TResponse>`** &ndash; Runs FluentValidation validators and short-circuits on error

## Dependencies

- [FluentValidation](https://www.nuget.org/packages/FluentValidation)
- [MediatR](https://www.nuget.org/packages/MediatR)
- [Microsoft.Extensions.Caching.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Abstractions)
- [Microsoft.Extensions.Caching.Memory](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Memory)
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)
- [YuckQi.Domain.Validation](https://www.nuget.org/packages/YuckQi.Domain.Validation) (project reference)

## Installation

```shell
dotnet add package YuckQi.Application.Core
```
