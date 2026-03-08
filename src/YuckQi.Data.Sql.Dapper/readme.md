# YuckQi.Data.Sql.Dapper

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL databases using Dapper and SimpleCRUD. Serves as the base layer for database-specific Dapper implementations.

## Key Types

Provides handler implementations that operate over an `IDbTransaction` scope:

- **`CreationHandler`** &ndash; inserts records via `SimpleCRUD.Insert`
- **`RevisionHandler`** &ndash; updates records via `SimpleCRUD.Update`
- **`PhysicalDeletionHandler`** &ndash; deletes records via `SimpleCRUD.Delete`
- **`RetrievalHandlerBase`** &ndash; abstract base for retrieval with `ISqlGenerator`-driven queries
- **`SearchHandlerBase`** &ndash; abstract base for paginated search with `ISqlGenerator`-driven queries
- **`ISqlGenerator`** &ndash; interface for generating provider-specific SQL (implemented by database-specific packages)
- **`DynamicParameterExtensions`** &ndash; builds Dapper `DynamicParameters` from `FilterCriteria`

Database-specific packages (MySql, Oracle, SqlServer) provide concrete `ISqlGenerator` implementations and ready-to-use retrieval/search handlers.

## Dependencies

- [Dapper.SimpleCRUD](https://www.nuget.org/packages/Dapper.SimpleCRUD)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper
```
