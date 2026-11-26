# YuckQi.Data.Sql.Dapper.SqlServer

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL Server databases using Dapper and SimpleCRUD.

## Key Types

- **`SqlGenerator<TRecord>`** &ndash; SQL Server-specific `ISqlGenerator` implementation using bracket-quoted identifiers, `OFFSET ... FETCH` pagination, and `lower(cast(column as nvarchar(max))) LIKE lower(value)` with `ESCAPE` for `FilterOperation.Contains` (a case-insensitive substring match whatever the column collation, including on legacy `text` and `ntext` columns; `[` is treated as a literal via `REPLACE`). A search with no sort orders by the record's key columns, or by `(select null)` when the record has no key, because SQL Server rejects `OFFSET ... FETCH` without `ORDER BY`
- **`RetrievalHandler`** &ndash; concrete retrieval handler pre-configured for SQL Server
- **`SearchHandler`** &ndash; concrete paginated search handler pre-configured for SQL Server

All handlers provide overloads with an explicit `TRecord` type parameter for domain-to-record mapping.

## Dependencies

- [YuckQi.Data.Sql.Dapper](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper.SqlServer
```
