# YuckQi.Extensions.Mapping.AutoMapper

An `IMapper` implementation backed by the [AutoMapper](https://www.nuget.org/packages/AutoMapper) framework.

## Key Types

- **`DefaultMapper`** &ndash; implements `YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces.IMapper` by delegating to an AutoMapper `IMapper` instance created from a provided `IConfigurationProvider`

## Dependencies

- [AutoMapper](https://www.nuget.org/packages/AutoMapper)
- [YuckQi.Extensions.Mapping.Abstractions](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Abstractions)

## Installation

```shell
dotnet add package YuckQi.Extensions.Mapping.AutoMapper
```
