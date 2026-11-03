# YuckQi.Data.Sql.Dapper.MySql

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for MySQL databases using Dapper and SimpleCRUD.

## Key Types

- **`SqlGenerator<TRecord>`** &ndash; MySQL-specific `ISqlGenerator` implementation using backtick-quoted identifiers, `LIMIT`/`OFFSET` pagination, and `LIKE` with `ESCAPE` for `FilterOperation.Contains` (case sensitivity follows the connection collation)
- **`RetrievalHandler`** &ndash; concrete retrieval handler pre-configured for MySQL
- **`SearchHandler`** &ndash; concrete paginated search handler pre-configured for MySQL

All handlers provide overloads with an explicit `TRecord` type parameter for domain-to-record mapping.

## Dependencies

- [YuckQi.Data.Sql.Dapper](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper.MySql
```
