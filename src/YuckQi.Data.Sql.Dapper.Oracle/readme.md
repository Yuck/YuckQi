# YuckQi.Data.Sql.Dapper.Oracle

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for Oracle databases using Dapper and SimpleCRUD.

## Key Types

- **`SqlGenerator<TRecord>`** &ndash; Oracle-specific `ISqlGenerator` implementation using double-quoted identifiers, `:param` bind variables, and `OFFSET ... FETCH` pagination
- **`RetrievalHandler`** &ndash; concrete retrieval handler pre-configured for Oracle
- **`SearchHandler`** &ndash; concrete paginated search handler pre-configured for Oracle

All handlers provide overloads with an explicit `TRecord` type parameter for domain-to-record mapping.

## Dependencies

- [YuckQi.Data.Sql.Dapper](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper.Oracle
```
