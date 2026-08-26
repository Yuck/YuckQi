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
- **`DistributedCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IDistributedCache` to cache responses for cacheable requests; configuration via `DistributedCachingBehaviorOptions` record (same file)
- **`MemoryCacheInvalidationBehavior<TRequest, TResponse>`** &ndash; Removes keys from `IMemoryCache` after the handler runs when `TResponse` implements `IHasCacheInvalidationKeys`; expands seeds through required `ICacheDependencyGraph`
- **`MemoryCachingBehavior<TRequest, TResponse>`** &ndash; Uses `IMemoryCache` to cache responses for cacheable requests; configuration via `MemoryCachingBehaviorOptions` record (same file)

**Caching dependency graph** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph`)

- **`CacheKeyParts`** / **`CacheKeyContext`** &ndash; structured cache key parse result and factory callback context
- **`CacheDependencyGraph`** &ndash; default `ICacheDependencyGraph` implementation; use `Create(...)` or `Empty`, then register as `ICacheDependencyGraph`

**Caching dependency graph builders** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Builders`)

- **`CacheDependencyGraphBuilder`** / **`CacheResourceDependencyBuilder`** &ndash; fluent configuration of which resources invalidate which dependents

**Caching dependency graph factories** (`YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories`)

- **`CacheKeyFactory`** &ndash; creates and parses structured cache keys (`resource`, `resource:identifier`, `resource:identifier;name=value`)

**Logging** (`YuckQi.Application.Core.Behaviors.Logging`)

- **`LoggingBehavior<TRequest, TResponse>`** &ndash; Logs message handling start and completion

**Validation** (`YuckQi.Application.Core.Behaviors.Validation`)

- **`ValidationResponse`** / **`ValidationResponse<T>`** &ndash; recommended Mediator `TResponse` for use with `ValidationBehavior`. Implements `IHasValidationResults`. On validation failure the behavior returns `new ValidationResponse` / `new ValidationResponse<T> { ValidationResults = … }` with `Value` left null; on success handlers return the envelope with `Value` set. Keep payload/`T` free of validation concerns (and free of the `new()` constraint), so `required` members on payloads remain valid.
- **`ValidationBehavior<TRequest, TResponse>`** &ndash; Runs FluentValidation validators and short-circuits on error when `TResponse` implements `IHasValidationResults` (prefer `ValidationResponse` / `ValidationResponse<T>` as `TResponse`)

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
services.AddMemoryCache();
services.AddSingleton(Options.Create(new MemoryCachingBehaviorOptions(TimeSpan.FromMinutes(5))));
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
2. **Hit** — returns the cached response; the handler is not invoked.
3. **Miss** — invokes `next`, then stores the response (optional absolute expiration from options).
4. Cache get/set failures are fail-soft (logged; the request still proceeds).

#### Write / invalidating response (`IHasCacheInvalidationKeys`)

1. `MemoryCacheInvalidationBehavior` / `DistributedCacheInvalidationBehavior` always invokes `next` first.
2. Reads `response.CacheKeys` (the seeds).
3. Calls `dependencyGraph.GetExpandedCacheKeys(seeds)`.
4. Removes every expanded key from the cache.
5. Remove failures are fail-soft (logged; other keys still attempted).

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
