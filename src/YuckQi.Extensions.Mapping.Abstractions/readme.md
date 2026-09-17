# YuckQi.Extensions.Mapping.Abstractions

Mapping abstractions for an object-to-object mapper. Defines the `IMapper` interface consumed by other YuckQi libraries to decouple domain-to-data mapping from any specific mapping framework.

## Key Types

- **`IMapper`** &ndash; interface with generic and non-generic `Map` overloads for non-null sources (non-null results; throws if mapping yields null) and matching `MapOrNull` overloads that accept null sources and return null; generic `MapOrNull` methods are constrained to reference types


## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.Abstractions
```
