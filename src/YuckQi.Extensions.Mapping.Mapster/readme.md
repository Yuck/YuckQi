# YuckQi.Extensions.Mapping.Mapster

An `IMapper` implementation backed by the [Mapster](https://www.nuget.org/packages/Mapster) framework.

## Key Types

- **`DefaultMapper`** &ndash; implements `IMapper` by delegating to a Mapster `IMapper` instance; `Map` requires non-null sources and throws if the mapped result is null, `MapOrNull` short-circuits null sources to null; accepts an optional `TypeAdapterConfig` (defaults to the global configuration when null)


## Dependencies

- [Mapster](https://www.nuget.org/packages/Mapster)
- [YuckQi.Extensions.Mapping.Abstractions](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Abstractions)

## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.Mapster
```
