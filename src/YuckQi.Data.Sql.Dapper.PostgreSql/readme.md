# YuckQi.Data.Sql.Dapper.PostgreSql

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for PostgreSQL databases using Dapper and SimpleCRUD.

## Key Types

- **`SqlGenerator<TRecord>`** &ndash; PostgreSQL-specific `ISqlGenerator` implementation using double-quoted identifiers, `public` default schema, and `LIMIT`/`OFFSET` pagination
- **`RetrievalHandler`** &ndash; concrete retrieval handler pre-configured for PostgreSQL
- **`SearchHandler`** &ndash; concrete paginated search handler pre-configured for PostgreSQL

All handlers provide overloads with an explicit `TRecord` type parameter for domain-to-record mapping.

## Dependencies

- [YuckQi.Data.Sql.Dapper](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper.PostgreSql
```
