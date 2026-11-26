# YuckQi.Extensions.Mapping.Mapster

An `IMapper` implementation backed by the [Mapster](https://www.nuget.org/packages/Mapster) framework.

## Key Types

- **`DefaultMapper`** &ndash; implements `IMapper` by delegating to a Mapster `IMapper` instance; `Map` requires non-null sources and throws if the mapped result is null, `MapOrNull` short-circuits null sources to null, or to the existing destination for the overloads that take one; accepts an optional `TypeAdapterConfig` (defaults to the global configuration when null)

The non-generic overloads check their type arguments before mapping. A source that is not an instance of `sourceType`, or a destination that is not an instance of `destinationType`, throws an `ArgumentException` naming the parameter and both types. Derived types, interface implementations and boxed values for a `Nullable<T>` type pass the check. `MapOrNull` with a null source maps nothing, so it does not check the type arguments.

Upgrading to 10.5.0:

- `MapOrNull(source, destination)` and `MapOrNull(source, destination, sourceType, destinationType)` return `destination` unchanged when `source` is null. They used to return null.
- The non-generic `Map` and `MapOrNull` overloads throw `ArgumentException` for mismatched type arguments, and `ArgumentNullException` for a null `sourceType` or `destinationType`, whenever the source is not null. Mismatches used to surface as an `InvalidCastException` from inside Mapster.

## Dependencies

- [Mapster](https://www.nuget.org/packages/Mapster)
- [YuckQi.Extensions.Mapping.Abstractions](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Abstractions)

## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.Mapster
```
