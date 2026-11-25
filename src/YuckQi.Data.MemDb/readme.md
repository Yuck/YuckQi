# YuckQi.Data.MemDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for an in-memory "database" backed by `ConcurrentDictionary`, ideal for rapid development and testing without external dependencies.

## Key Types

- **`CreationHandler`** &ndash; adds entities to the in-memory store
- **`RevisionHandler`** &ndash; updates entities in-place
- **`PhysicalDeletionHandler`** &ndash; removes entities from the store
- **`RetrievalHandler`** &ndash; retrieves entities by identifier or filter criteria using compiled expression predicates
- **`SearchHandler`** &ndash; paginated search with filtering and sorting; `FilterOperation.Contains` is a case-insensitive substring match (`StringComparison.OrdinalIgnoreCase`)
- **`UnitOfWork` / `UnitOfWork<TScope>`** &ndash; manages typed `ConcurrentDictionary` instances per entity type; `GetEntities<TDomainEntity, TIdentifier>()` provides direct access to the underlying store; changes are applied immediately and `SaveChanges()` / `SaveChanges(CancellationToken)` are no-ops (no transactional semantics)

Entities are stored by `Identifier`. `Create` throws `InvalidOperationException` when it is unset: `null`, or the default value for value-type identifiers (`0`, `Guid.Empty`) unless `CreationOptions` is created with `allowDefaultIdentifier: true`. Set the identifier yourself or pass an `IdentifierFactory` in `CreationOptions`. `Revise` and `Delete` reject only `null` identifiers, so an entity stored under `0` can still be revised and deleted.

Upgrading to 10.5.0:

- `Create` with a value-type identifier left at its default now throws `InvalidOperationException`, as it does for `null`. The entity used to be stored under key `0` / `Guid.Empty`, and the next unset entity then failed with `CreationException`. Pass `allowDefaultIdentifier: true` to keep storing default identifiers.

## Dependencies

- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.MemDb
```
