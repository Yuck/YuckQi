# YuckQi.Data.Sql.EntityFramework

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL databases using Entity Framework Core. Handlers operate over a `DbContext` scope.

## Key Types

- **`UnitOfWork<TContext>`** &ndash; implements `IUnitOfWork<TContext>` for a `DbContext`; `SaveChanges()` / `SaveChanges(CancellationToken)` persist all tracked changes in the underlying `DbContext` (transaction semantics are those of the configured EF Core provider)
- **`CreationHandler`** &ndash; inserts entities via `DbSet.Add`; like Dapper, does not call `SaveChanges` (caller commits via `UnitOfWork.SaveChanges()`). For database-generated keys (e.g. identity), use `CreationOptions.IdentifierFactory` so an identifier is returned; otherwise the key is only set after `SaveChanges`.
- **`RevisionHandler`** &ndash; updates entities via `DbSet.Update`
- **`PhysicalDeletionHandler`** &ndash; deletes entities via `DbSet.Remove`
- **`RetrievalHandler`** &ndash; retrieval by identifier or filter criteria using `DbSet.Find` and LINQ
- **`SearchHandler`** &ndash; paginated search with filtering and sorting via LINQ
- **`FilterCriteriaExtensions`** &ndash; builds `Expression<Func<T, Boolean>>` predicates from `FilterCriteria` for use with `IQueryable`; `FilterOperation.Contains` maps to `String.Contains` (case sensitivity follows the EF provider/collation)

When the key read back after `Add` is unset (`null`, or `0` / `Guid.Empty` for value types unless `CreationOptions` is created with `allowDefaultIdentifier: true`), `CreationHandler` detaches the record before throwing `CreationException`, so a later `SaveChanges` does not insert it.

Upgrading to 10.5.0:

- `Create` with a key that EF does not generate and that is left at `0` / `Guid.Empty` now throws `CreationException` and detaches the record. It used to be tracked and inserted with the default key on `SaveChanges`.

## Dependencies

- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.EntityFramework
```
