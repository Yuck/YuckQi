# YuckQi.Extensions.Mapping.Abstractions

Mapping abstractions for an object-to-object mapper. Defines the `IMapper` interface consumed by other YuckQi libraries to decouple domain-to-data mapping from any specific mapping framework.

## Key Types

- **`IMapper`** &ndash; interface with generic and non-generic overloads for mapping between object types; null sources short-circuit to null, while destination-bearing overloads require a non-null destination

## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.Abstractions
```
