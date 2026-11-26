# YuckQi.Data.Sql.Dapper

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL databases using Dapper and SimpleCRUD. Serves as the base layer for database-specific Dapper implementations.

## Key Types

Provides handler implementations that operate over an `IDbTransaction` scope:

- **`CreationHandler`** &ndash; inserts records via `SimpleCRUD.Insert`
- **`RevisionHandler`** &ndash; updates records via `SimpleCRUD.Update`
- **`PhysicalDeletionHandler`** &ndash; deletes records via `SimpleCRUD.Delete`
- **`RetrievalHandlerBase`** &ndash; abstract base for retrieval with `ISqlGenerator`-driven queries
- **`SearchHandlerBase`** &ndash; abstract base for paginated search with `ISqlGenerator`-driven queries
- **`ISqlGenerator`** &ndash; interface for generating provider-specific SQL (implemented by database-specific packages). The handlers bind filter values with `ToDynamicParameters`, so an implementation must reference them by the names `FilterParameterNames` returns (`@p0`, `@p1_0`), not by field name; any other name fails when the query runs
- **`RecordSchema<TRecord>`** &ndash; resolves a record's table name, schema, selectable properties, key properties and column names for the `SqlGenerator`s, and rejects filter and sort names that are not mapped properties of the record
- **`FilterCriteriaExtensions`** &ndash; builds Dapper `DynamicParameters` from `FilterCriteria`; `Contains` values are wrapped as `%value%` with `%`, `_`, and `\` escaped so they are literal. SQL Server's `SqlGenerator` also treats `[` as literal (a `LIKE` character-class wildcard on that dialect only)
- **`FilterParameterNames`** &ndash; the parameter names shared by `ToDynamicParameters` and the `SqlGenerator`s; use it in a custom `ISqlGenerator` so its SQL matches the bound parameters

Database-specific packages (MySql, Oracle, PostgreSql, SqlServer) provide concrete `ISqlGenerator` implementations and ready-to-use retrieval/search handlers.

## Record Mapping for Reads

The database-specific `SqlGenerator`s build filtered `Get`, `GetList`, `Search` and `Count` queries from `RecordSchema<TRecord>`, which reads the same attributes SimpleCRUD uses for writes. Attributes are matched by type name, so both the Dapper.SimpleCRUD attributes and their `System.ComponentModel.DataAnnotations` equivalents work. When a member carries both, the Dapper.SimpleCRUD attribute wins:

- **Table and schema** &ndash; `[Table("name", Schema = "schema")]`, defaulting to the record type's name with no schema. Tables without a schema are left unqualified, so the database's default schema applies, the same as for SimpleCRUD writes. Custom SimpleCRUD `ITableNameResolver` / `IColumnNameResolver` registrations are not used for reads.
- **Columns** &ndash; `[Column("name")]`, defaulting to the property name. Selected columns are aliased back to the property name.
- **Mapped properties** &ndash; public instance properties with a setter (including `init`), an auto-property backing field, or a name matching a constructor parameter (ignoring case, as Dapper's constructor mapping does), so `class R(Int32 id) { public Int32 Id => id; }` is mapped. `[NotMapped]` properties, other computed properties such as `String Display => ...`, static properties and indexers are not mapped. `[IgnoreSelect]` properties are mapped but not selected. Properties of any type are mapped, including collections such as `IReadOnlyList<Guid>`, since Dapper type handlers and providers such as Npgsql can read them; mark navigation properties `[NotMapped]` or `[IgnoreSelect]`.

### Filter and Sort Names

Only `FilterCriteria.FieldName` and `SortCriteria.Expression` values that name a mapped property of the record, or its column, are accepted. Matches are tried in order: property name, column name, property name ignoring case, column name ignoring case; the first rule that matches exactly one property is used. Any other name throws an `ArgumentException` before SQL is built, so filter and sort names taken from user input cannot inject SQL. Filters and sorts on a `[Column]`-mapped property use the mapped column name. Table, schema and column identifiers are quoted with the dialect's quote character doubled.

### Filter Parameters

Filter values are bound by position, not by field name: the criterion at index `i` is bound as `p{i}`, and element `j` of an `In` criterion as `p{i}_{j}` (`@p0`, `@p1_0`; `:p0` on Oracle). Several criteria on one field, such as `Age >= 18` and `Age <= 65`, each get their own parameter, and fields named after Oracle reserved words such as `Date` or `Level` bind normally. A criterion with a `null` value is written as SQL `null` but still uses its position, so later names don't shift.

`In` takes a collection of values. A `String` or `Byte[]` value throws an `ArgumentException` instead of being split into characters or bytes; wrap it as `new[] { "ABC" }` to match the single value. A `null` member matches `null` values, the same as `Equal` with `null`: it is not bound, and the filter becomes `(column in (...) or column is null)`, or `(column is null)` when every member is `null`. The other members keep their positions, so `new Int32?[] { null, 5 }` binds only `p0_1`. An empty collection matches nothing.

### `Contains`

`Contains` is a case-insensitive substring match on every provider, the same as MemDb. MySQL, Oracle and SQL Server compare `lower(column)` with `lower(value)`, so the result does not depend on the column or connection collation; PostgreSQL uses `ILIKE`. SQL Server casts the column to `nvarchar(max)` first so `text` and `ntext` columns work, and MySQL converts it to `utf8mb4` first so binary string columns are lowered too. Wildcard characters in the value are matched literally.

### Search Ordering

A search with no sort criteria orders by the record's key columns so pages don't repeat or skip rows. The key columns are the mapped properties marked `[Key]` (Dapper.SimpleCRUD or DataAnnotations), or the property named `Id` (ignoring case) when none is marked, the same rule SimpleCRUD uses for writes. A record with neither pages in an undefined order: SQL Server orders by `(select null)`, since it rejects `OFFSET ... FETCH` without `ORDER BY`, and the other providers emit no `ORDER BY`. A search with sort criteria uses only those criteria; include a unique column, such as the key, as the last sort when the others can tie.

Upgrading to 10.5.0:

- The SQL Server and PostgreSQL generators no longer qualify tables with `dbo` / `public` by default; add `Schema = "..."` to `[Table]` if reads relied on that.
- Filter and sort names that are not a mapped property or column of the record now throw `ArgumentException` instead of being sent to the database.
- `[NotMapped]` and computed properties are no longer selected, and DataAnnotations `[Table]` / `[Column]` attributes now apply to reads as well as writes.
- Filter parameters are named by position (`p0`, `p1_0`) instead of by field name (`Name`, `Name0`). Custom `ISqlGenerator`s used with these handlers must use `FilterParameterNames` to match.
- `In` with a `String` or `Byte[]` value now throws `ArgumentException` instead of matching each character or byte.
- `In` with a `null` member now matches `null` values. `ToDynamicParameters` no longer binds `null` members, so a custom `ISqlGenerator` must not reference their parameter names.
- `Contains` now ignores case on MySQL, Oracle and SQL Server, where it used to follow the collation. PostgreSQL is unchanged.
- A search with no sort criteria now orders by the record's key columns. Unsorted searches on SQL Server used to fail with "Invalid usage of the option FETCH"; the other providers returned pages in an undefined order.
- `ISqlGenerator.GenerateSearchQuery` takes `IReadOnlyList<SortCriteria>` instead of `IOrderedEnumerable<SortCriteria>`. Custom generators must change their signature; the criteria are in priority order, as the built-in generators already treated them. Dotted sort paths such as `Address.City` are not a mapped property, so they throw `ArgumentException` like any other unknown name.
- `Create` throws `CreationException` when the insert returns `0` / `Guid.Empty` as the key (for example an `Int16` identity key, which SimpleCRUD does not read back), unless `CreationOptions` is created with `allowDefaultIdentifier: true`. The row has already been inserted in the scope's transaction, so dispose the unit of work without calling `SaveChanges` to roll it back.

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
