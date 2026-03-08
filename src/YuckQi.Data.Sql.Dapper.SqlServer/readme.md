# YuckQi.Data.Sql.Dapper.SqlServer

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL Server databases using Dapper and SimpleCRUD.

## Key Types

- **`SqlGenerator<TRecord>`** &ndash; SQL Server-specific `ISqlGenerator` implementation using bracket-quoted identifiers, `dbo` default schema, and `OFFSET ... FETCH` pagination
- **`RetrievalHandler`** &ndash; concrete retrieval handler pre-configured for SQL Server
- **`SearchHandler`** &ndash; concrete paginated search handler pre-configured for SQL Server

All handlers provide overloads with an explicit `TRecord` type parameter for domain-to-record mapping.

## Dependencies

- [YuckQi.Data.Sql.Dapper](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper.SqlServer
```
