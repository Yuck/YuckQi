# YuckQi.Extensions.Mapping.Abstractions

Mapping abstractions for an object-to-object mapper. Defines the `IMapper` interface consumed by other YuckQi libraries to decouple domain-to-data mapping from any specific mapping framework.

## Key Types

- **`IMapper`** &ndash; interface with generic and non-generic `Map` overloads for non-null sources (non-null results; throws if mapping yields null) and matching `MapOrNull` overloads that accept null sources; generic `MapOrNull` methods are constrained to reference types

`MapOrNull` overloads without a destination return null when the source is null. The overloads that take an existing destination return that destination unchanged when the source is null, so `target = mapper.MapOrNull(source, target)` keeps `target`. They never return null.

Upgrading to 10.5.0:

- The `MapOrNull` overloads that take an existing destination return the destination instead of null when the source is null, and their return types are no longer nullable. Code that checked their result for null to detect a null source must check the source instead.
- Custom `IMapper` implementations must change those two overloads to return `Object` / `TDestination` instead of `Object?` / `TDestination?` (otherwise the compiler reports CS8766) and return the destination when the source is null. Mocks that return null from them need the same change.

## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.Abstractions
```
