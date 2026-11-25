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
- **`FilterCriteriaExtensions`** &ndash; builds Dapper `DynamicParameters` from `FilterCriteria`; `Contains` values are wrapped as `%value%` with `%`, `_`, and `\` escaped so they are literal. SQL Server's `SqlGenerator` also treats `[` as literal (a `LIKE` character-class wildcard on that dialect only)

Database-specific packages (MySql, Oracle, PostgreSql, SqlServer) provide concrete `ISqlGenerator` implementations and ready-to-use retrieval/search handlers.

## `DateOnly` and `TimeOnly` Properties

SimpleCRUD only generates insert/update columns for the types on its built-in "simple type" list, which leaves out `DateOnly` and `TimeOnly`. `CreationHandler` and `RevisionHandler` add those columns (including the nullable forms), so you don't need `[Editable(true)]` on them. The added columns follow SimpleCRUD's usual attribute rules: `[Column]`, `[Key]`, `[Required]`, `[IgnoreInsert]`, `[IgnoreUpdate]`, `[NotMapped]`, `[ReadOnly]` and `[Editable]` work the same as they do for a `DateTime` property. If another property already maps to the same column name, the column is written once, from that other property.

The handlers do this by updating the column lists SimpleCRUD caches for each record type. They check those cached lists, and any cached UPDATE statement, on every write and restore the added columns if SimpleCRUD has rebuilt them, so you can still call SimpleCRUD's `Insert`/`Update` directly on the same record types elsewhere. A direct SimpleCRUD call writes only the columns SimpleCRUD itself knows about. A direct call made at the same moment a handler is restoring the cache can also leave that one handler statement without the added columns.

Upgrading: you can remove `[Editable(true)]` attributes that were only there for `DateOnly`/`TimeOnly` columns. These values now go to Dapper as parameters, so Dapper must be able to bind them. If your Dapper version or database provider can't, register an `SqlMapper.TypeHandler<DateOnly>` and `SqlMapper.TypeHandler<TimeOnly>`. Without one, the write fails with an error instead of writing `NULL`.

This works by reading private members of Dapper.SimpleCRUD (tested against 2.3.0), and only for record types that have `DateOnly`/`TimeOnly` properties. If a future SimpleCRUD version removes those members or changes `GetColumnName`'s signature, the write throws an `InvalidOperationException` that names the member.

## Dependencies

- [Dapper.SimpleCRUD](https://www.nuget.org/packages/Dapper.SimpleCRUD)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.Dapper
```
