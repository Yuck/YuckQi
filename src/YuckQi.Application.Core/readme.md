# YuckQi.Application.Core

A .NET library for bootstrapping a domain application project. Provides Mediator pipeline behaviors for logging, validation (FluentValidation), and caching with dependency-graph invalidation.

## Key Types

### Abstractions

- **`IHasCacheInvalidationKeys`** &ndash; aspect marker for Mediator *response* types that trigger cache invalidation; exposes `CacheKeys` (`IReadOnlySet<CacheKey>`) as the seeds to invalidate after the handler runs
- **`IHasCacheKey`** &ndash; aspect marker for cacheable Mediator messages (`IMessage`) with a `CacheKey`
- **`IHasValidationResults`** &ndash; aspect marker for Mediator *response* types that carry validation results; exposes `ValidationResults` (`IReadOnlyCollection<Result>`) for validation behavior
- **`ICacheDependencyGraph`** &ndash; expands a set of cache keys by walking a resource dependency graph (transitive); lives in `YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces`

### Behaviors

Pipeline behaviors are organized by purpose in subfolders and namespaces:

**Caching** (`YuckQi.Application.Core.Behaviors.Caching`)

- **`CacheKey`** &ndash; value object wrapping a cache key string, with implicit conversions to/from `String` (cast to `String` when calling `IMemoryCache`, which takes `Object` keys)
- **`DistributedCacheInvalidationBehavior<TRequest, TResponse>`** &ndash; Removes keys from `IDistributedCache` after the handler runs when `TResponse` implements `IHasCacheInvalidationKeys`; expands seeds through required `ICacheDependencyGraph`
- **`DistributedCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IDistributedCache` to cache responses for cacheable requests; configured through `DistributedCachingBehaviorOptions`
- **`DistributedCachingBehaviorOptions`** &ndash; `CacheDuration` (absolute expiration; `null` for none) and `SerializerOptions` (`JsonSerializerOptions` used to store and read responses; `null` uses System.Text.Json defaults plus the `YuckQi.Domain.Validation` result converters). Has a parameterless constructor and settable properties, so it works with `services.Configure<T>(...)` and configuration binding
- **`MemoryCacheInvalidationBehavior<TRequest, TResponse>`** &ndash; Removes keys from `IMemoryCache` after the handler runs when `TResponse` implements `IHasCacheInvalidationKeys`; expands seeds through required `ICacheDependencyGraph`
- **`MemoryCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IMemoryCache` to cache responses for cacheable requests; configured through `MemoryCachingBehaviorOptions`
- **`MemoryCachingBehaviorOptions`** &ndash; `CacheDuration` (absolute expiration; `null` for none), `SerializerOptions` (`JsonSerializerOptions` used to snapshot responses; `null` uses System.Text.Json defaults plus the `YuckQi.Domain.Validation` result converters) and `Size` (the entry's `MemoryCacheEntryOptions.Size`, default `1`, must not be negative; counts against `MemoryCacheOptions.SizeLimit`). Has a parameterless constructor and settable properties, so it works with `services.Configure<T>(...)` and configuration binding

**Caching dependency graph** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph`)

- **`CacheKeyParts`** / **`CacheKeyContext`** &ndash; structured cache key parse result and factory callback context
- **`CacheDependencyGraph`** &ndash; default `ICacheDependencyGraph` implementation; use `Create(...)` or `Empty`, then register as `ICacheDependencyGraph`

**Caching dependency graph builders** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders`)

- **`CacheDependencyGraphBuilder`** / **`CacheResourceDependencyBuilder`** &ndash; fluent configuration of which resources invalidate which dependents

**Caching dependency graph factories** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories`)

- **`CacheKeyFactory`** &ndash; creates and parses structured cache keys (`resource`, `resource:identifier`, `resource:identifier;name=value`)

**Logging** (`YuckQi.Application.Core.Behaviors.Logging`)

- **`LoggingBehavior<TRequest, TResponse>`** &ndash; Logs when message handling starts and when it completes, fails or is canceled, with the elapsed time. A failure is logged as an error with the exception and then rethrown; an `OperationCanceledException` thrown after the request's token is canceled is logged at information level and rethrown.

**Validation** (`YuckQi.Application.Core.Behaviors.Validation`)

- **`ValidationResponse`** / **`ValidationResponse<T>`** &ndash; recommended Mediator `TResponse` for use with `ValidationBehavior`. Implements `IHasValidationResults`. On validation failure the behavior returns `new ValidationResponse` / `new ValidationResponse<T> { ValidationResults = … }` with `Value` left null; on success handlers return the envelope with `Value` set. Keep payload/`T` free of validation concerns (and free of the `new()` constraint), so `required` members on payloads remain valid.
- **`ValidationBehavior<TRequest, TResponse>`** &ndash; Runs FluentValidation validators and short-circuits on error when `TResponse` implements `IHasValidationResults` (prefer `ValidationResponse` / `ValidationResponse<T>` as `TResponse`). Validators are resolved as `IEnumerable<IValidator<TRequest>>`, the service type that `AddValidatorsFromAssembly` / `AddValidatorsFromAssemblyContaining` register, and run one at a time in registration order, so validators can share a scoped `DbContext` or connection. Only results that have details are reported, as plain `Result` instances that carry the details but not the request. When none of them has an error, the handler runs and the behavior returns a shallow copy of its response with those results (warnings) appended to `ValidationResults`. The handler's own instance is not changed, so a response held in a cache or shared between requests does not pick up warnings. A `null` response is returned as is.

Upgrading to 10.5.0:

- `ValidationBehavior` depends on `IEnumerable<IValidator<TRequest>>` instead of `IEnumerable<AbstractValidator<TRequest>>`. Validators registered through FluentValidation's DI extensions now run; before, they were never resolved and requests reached handlers unvalidated. Validators registered only as `AbstractValidator<TRequest>` must be registered as `IValidator<TRequest>`.
- Validators run sequentially instead of concurrently.
- A failed validation's `ValidationResults` no longer include empty results from validators that passed.
- On success, warning results are returned on a shallow copy of the handler's response instead of being dropped. When there are warnings, the returned instance is not the one the handler returned.
- Validator rules with `Severity.Warning` or `Severity.Info` no longer short-circuit; the handler runs and those failures are returned as warnings. Validation results carry the failure's FluentValidation `ErrorCode` instead of always `InvalidRequestDetail` (see `YuckQi.Domain.Validation`).
- `LoggingBehavior` logs failures (error level, with the exception) and cancellations, with the elapsed time.
- `MemoryCachingBehaviorOptions` and `DistributedCachingBehaviorOptions` are no longer positional records. They have a parameterless constructor and settable `CacheDuration` / `SerializerOptions` properties (plus `Size` on the memory options), so `services.Configure<T>(...)` and configuration binding work; before, both threw `MissingMethodException`. `new MemoryCachingBehaviorOptions(duration)` still compiles, but deconstruction and positional `with` patterns do not. Each type moved to its own file.
- Cached entries carry the response type. A response cached under a key is only returned to a request with the same `TResponse`; any other type, and entries written by earlier versions, are treated as a miss and overwritten. Before, `DistributedCachingBehavior` deserialized another type's JSON into a mostly default `TResponse` and returned it as a hit.
- `MemoryCachingBehavior` stores a JSON snapshot of the response and returns a fresh copy on every hit, as `DistributedCachingBehavior` does. Mutating a returned response no longer changes what later callers get. Responses cached in memory must now round-trip through `System.Text.Json`.
- Both caching behaviors check every response before storing it: it is serialized as `TResponse`, compared with its serialization as its runtime type, read back, and serialized again. A response that loses data on the way (non-public setters, get-only properties backed by private fields, a derived type returned for a base `TResponse`) or cannot be read back is returned uncached and a warning is logged, instead of being cached and later returned with missing data.
- When `SerializerOptions` is `null`, the behaviors use System.Text.Json defaults plus `AddResultJsonConverters()`, so `ValidationResponse<T>` with warnings is cached without extra setup. Custom `SerializerOptions` must call `AddResultJsonConverters()` themselves if responses carry `Result` values.
- Responses that are `null`, or that implement `IHasValidationResults` and contain a result that is not valid, are no longer cached.
- `MemoryCachingBehavior` sets `MemoryCacheEntryOptions.Size` (`MemoryCachingBehaviorOptions.Size`, default `1`). Before, a memory cache with `SizeLimit` rejected every entry and caching was silently disabled.
- `DistributedCachingBehavior` and `DistributedCacheInvalidationBehavior` rethrow an `OperationCanceledException` raised after the request's token is canceled. Before, a canceled read fell through to the handler with the canceled token, and invalidation logged one warning per key and returned normally.

## Caching guide

Caching is opt-in at the Mediator message/response layer. Reads declare a cache key; writes declare *seed* keys for what changed. A dependency graph expands those seeds so related cached queries are invalidated without listing every dependent key in every handler.

### Mental model

| Role | Responsibility |
|------|----------------|
| **`IHasCacheKey`** (on the *request*) | “This query’s response may be cached under this key.” |
| **`IHasCacheInvalidationKeys`** (on the *response*) | “This command changed these resources” — the **seeds**. |
| **`ICacheDependencyGraph`** | “Given these seeds, also invalidate these related keys.” |
| **Caching behaviors** | Get/set on cache hit/miss (reads). |
| **Invalidation behaviors** | After the handler, expand seeds and remove keys (writes). |

The graph does **not** discover what changed. Handlers (via the response) still supply seeds. The graph only expands them.

### Cache key format

Use `CacheKeyFactory` so read keys and invalidation seeds share one vocabulary.

| Form | Example | Meaning |
|------|---------|---------|
| Resource only | `order-list` | Global / aggregate key (no entity id) |
| Resource + identifier | `order:42` | Entity-scoped key |
| Resource + identifier + parameters | `order:42;customer=7` | Entity key plus context used when expanding dependents |

```csharp
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

CacheKeyFactory.Create("order-list");
CacheKeyFactory.Create("order", orderId);
CacheKeyFactory.Create("order", orderId, ("customer", customerId));
```

`CacheKey` is a value object over that string. It converts implicitly to/from `String`. When calling `IMemoryCache` (keys are `Object`), cast explicitly so the store receives a `String` rather than a boxed `CacheKey`:

```csharp
cache.Remove((String) key);
// or
var cacheKey = (String) key;
```

`IDistributedCache` takes `String`, so the implicit conversion is enough at the call site.

### Configuring the dependency graph

Register one `ICacheDependencyGraph` for the host. Edges are authored by **resource name** (the segment before `:`), not by full key strings.

```csharp
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph;
using YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Abstract.Interfaces;

services.AddSingleton<ICacheDependencyGraph>(CacheDependencyGraph.Create(graph => graph
    .When("order", order => order
        .Invalidates("order-detail")                          // same identifier → order-detail:42
        .InvalidatesGlobal("order-list")                      // always → order-list
        .InvalidatesFromParameter("customer-summary", "customer")) // from ;customer=… → customer-summary:7
    .When("customer-summary", summary => summary
        .InvalidatesGlobal("dashboard"))));
```

If you are not using graph edges yet, still register a graph — invalidation behaviors require it:

```csharp
services.AddSingleton(CacheDependencyGraph.Empty);
```

#### Builder methods on a resource

For a seed whose resource is `order` (e.g. `order:42;customer=7`):

| Method | Resulting key(s) |
|--------|------------------|
| **`Invalidates("order-detail")`** | Same identifier as the seed → `order-detail:42`. If the seed has no identifier, produces a resource-only key. |
| **`InvalidatesGlobal("order-list")`** | Always `order-list` (no identifier). |
| **`InvalidatesFromParameter("customer-summary", "customer")`** | Uses parameter `customer` from the seed → `customer-summary:7`. Skipped if the parameter is missing. |
| **`Invalidates(ctx => …)`** | Custom single key (`CacheKey?`; return `null` to skip) or many keys (`IEnumerable<CacheKey>`). `CacheKeyContext` exposes `Resource`, `Identifier`, `Parameter(name)`, `Key(...)`, and `Global(...)`. |

Expansion is **transitive** and cycle-safe: if `order` invalidates `customer-summary`, and `customer-summary` invalidates `dashboard`, a seed of `order:7` yields `order:7`, `customer-summary:7`, and `dashboard`.

Seeds themselves are always included in the expanded set.

### Wiring pipeline behaviors

The library does not register Mediator behaviors for you. In the host:

1. Register `IMemoryCache` and/or `IDistributedCache`.
2. Register `ICacheDependencyGraph` (configured or `Empty`).
3. Register caching options if you use the caching behaviors.
4. Register the open-generic pipeline behaviors you want (Mediator DI).

Example (memory cache + invalidation):

```csharp
services.AddMemoryCache(t => t.SizeLimit = 1_000);
services.Configure<MemoryCachingBehaviorOptions>(t => t.CacheDuration = TimeSpan.FromMinutes(5));
// or bind from configuration:
// services.Configure<MemoryCachingBehaviorOptions>(configuration.GetSection("Caching:Memory"));
services.AddSingleton<ICacheDependencyGraph>(CacheDependencyGraph.Create(/* … */));

// Register with your Mediator pipeline registration of choice, e.g.:
// services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MemoryCachingBehavior<,>));
// services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MemoryCacheInvalidationBehavior<,>));
```

**Pipeline order** is host-defined. Typical expectations:

- **Caching** should run so that on a hit it can short-circuit before the handler (and before invalidation for that same request, which would not apply to a pure read anyway).
- **Invalidation** must run *after* the handler so seeds come from the completed response.
- Pair memory *or* distributed behaviors with the matching store; do not assume one store’s keys are visible to the other.

### What happens at runtime

#### Cached read (`IHasCacheKey`)

1. `MemoryCachingBehavior` / `DistributedCachingBehavior` reads `request.CacheKey`.
2. **Hit** — returns the cached response; the handler is not invoked. Both behaviors store a JSON snapshot tagged with the response type and return a fresh copy on every hit, so callers can mutate what they get.
3. **Miss** — invokes `next`, then stores the response (optional absolute expiration from options; the memory behavior also sets the entry `Size`). A response is stored only if it round-trips through `System.Text.Json` unchanged with the configured `SerializerOptions`; otherwise it is returned uncached and a warning is logged. Responses must therefore be serializable data: public settable properties or constructor parameters, and `TResponse` matching the returned type's shape (collection interfaces such as `IReadOnlyList<T>` are fine).
4. An entry stored for a different `TResponse` under the same key is a miss and is overwritten. Two queries with different response types that share a key evict each other, so give them distinct keys (for example `customer-summary:42` and `customer-detail:42`) and let the dependency graph invalidate both.
5. `null` responses and responses implementing `IHasValidationResults` with a result that is not valid are returned without being cached. Warnings alone do not prevent caching.
6. Cache get/set failures are fail-soft (logged; the request still proceeds). Cancellation of the request's token is not a failure: the `OperationCanceledException` propagates.

#### Write / invalidating response (`IHasCacheInvalidationKeys`)

1. `MemoryCacheInvalidationBehavior` / `DistributedCacheInvalidationBehavior` always invokes `next` first.
2. Reads `response.CacheKeys` (the seeds).
3. Calls `dependencyGraph.GetExpandedCacheKeys(seeds)`.
4. Removes every expanded key from the cache.
5. Remove failures are fail-soft (logged; other keys still attempted). Cancellation of the request's token stops the loop and propagates.

Handlers never call the cache APIs directly; behaviors own get/set/remove.

### End-to-end example

**Graph** (startup):

```csharp
services.AddSingleton<ICacheDependencyGraph>(CacheDependencyGraph.Create(graph => graph
    .When("order", order => order
        .InvalidatesGlobal("order-list")
        .InvalidatesFromParameter("customer-summary", "customer"))));
```

**Cached query** — same key vocabulary as invalidation:

```csharp
public sealed class GetOrderListQuery : IRequest<IReadOnlyList<OrderDto>>, IHasCacheKey
{
    public CacheKey CacheKey => CacheKeyFactory.Create("order-list");
}

public sealed class GetCustomerSummaryQuery : IRequest<CustomerSummaryDto>, IHasCacheKey
{
    public required Int32 CustomerId { get; init; }

    public CacheKey CacheKey => CacheKeyFactory.Create("customer-summary", CustomerId);
}
```

**Command response** — seed what changed (include parameters the graph needs):

```csharp
public sealed record ReviseOrderResponse(OrderDto? Value, IReadOnlySet<CacheKey> CacheKeys) : IHasCacheInvalidationKeys;

// In the handler, after a successful revise of order 42 for customer 7:
return new ReviseOrderResponse(
    orderDto,
    new HashSet<CacheKey>
    {
        CacheKeyFactory.Create("order", 42, ("customer", 7))
    });
```

**Resulting invalidation** for that seed:

1. Seed: `order:42;customer=7`
2. Graph expands to: `order:42;customer=7`, `order-list`, `customer-summary:7`
3. Invalidation behavior removes those three keys from the configured cache

Subsequent `GetOrderListQuery` and `GetCustomerSummaryQuery` for customer `7` miss cache and re-run their handlers.

### Practical tips

- Prefer `CacheKeyFactory` everywhere keys are authored so separators and parameter encoding stay consistent.
- Put on the seed every parameter a dependent edge might need (`InvalidatesFromParameter` / custom factories); missing parameters simply skip that edge.
- Over-invalidation (clearing a list when one row changes) is intentional for correctness; tune edges if that is too aggressive.
- `CacheDependencyGraph.Empty` still invalidates exact seeds — useful before you add edges.
- Data-layer handlers stay cache-unaware; keep caching at the application/Mediator boundary.

## Dependencies

- [FluentValidation](https://www.nuget.org/packages/FluentValidation)
- [Mediator.Abstractions](https://www.nuget.org/packages/Mediator.Abstractions)
- [Microsoft.Extensions.Caching.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Abstractions)
- [Microsoft.Extensions.Caching.Memory](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Memory)
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)
- [YuckQi.Domain.Validation](https://www.nuget.org/packages/YuckQi.Domain.Validation) (project reference)

## Installation

```shell
dotnet add package YuckQi.Application.Core
```
