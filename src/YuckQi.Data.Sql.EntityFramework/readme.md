# YuckQi.Data.Sql.EntityFramework

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for SQL databases using Entity Framework Core. Handlers operate over a `DbContext` scope.

## Key Types

- **`UnitOfWork<TContext>`** &ndash; implements `IUnitOfWork<TContext>` for a `DbContext`; `SaveChanges()` / `SaveChanges(CancellationToken)` persist all tracked changes in the underlying `DbContext` (transaction semantics are those of the configured EF Core provider)
- **`CreationHandler`** &ndash; inserts entities via `DbSet.Add`; like Dapper, does not call `SaveChanges` (caller commits via `UnitOfWork.SaveChanges()`). For database-generated keys (e.g. identity), use `CreationOptions.IdentifierFactory` so an identifier is returned; otherwise the key is only set after `SaveChanges`.
- **`RevisionHandler`** &ndash; updates entities via `DbSet.Update`
- **`PhysicalDeletionHandler`** &ndash; deletes entities via `DbSet.Remove`
- **`RetrievalHandler`** &ndash; retrieval by identifier or filter criteria using `DbSet.Find` and LINQ
- **`SearchHandler`** &ndash; paginated search with filtering and sorting via LINQ
- **`FilterCriteriaExtensions`** &ndash; builds `Expression<Func<T, Boolean>>` predicates from `FilterCriteria` for use with `IQueryable`

## Dependencies

- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.Sql.EntityFramework
```
