# YuckQi.Data.MemDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for an in-memory "database" backed by `ConcurrentDictionary`, ideal for rapid development and testing without external dependencies.

## Key Types

- **`CreationHandler`** &ndash; adds entities to the in-memory store
- **`RevisionHandler`** &ndash; updates entities in-place
- **`PhysicalDeletionHandler`** &ndash; removes entities from the store
- **`RetrievalHandler`** &ndash; retrieves entities by identifier or filter criteria using compiled expression predicates
- **`SearchHandler`** &ndash; paginated search with filtering and sorting
- **`UnitOfWork` / `UnitOfWork<TScope>`** &ndash; manages typed `ConcurrentDictionary` instances per entity type; `GetEntities<TDomainEntity, TIdentifier>()` provides direct access to the underlying store; changes are applied immediately and `SaveChanges()` / `SaveChanges(CancellationToken)` are no-ops (no transactional semantics)

## Dependencies

- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.MemDb
```
