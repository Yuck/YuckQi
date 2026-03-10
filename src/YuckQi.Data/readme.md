# YuckQi.Data

A .NET library of lightweight data access handlers which can be used to compose repositories and domain services. Defines the core abstractions and base classes that provider-specific implementations extend.

## Key Concepts

### Handler Architecture

All data operations follow a **Template Method** pattern where public methods call abstract `DoXxx` methods that providers implement.

**Read Handlers**

- **`IRetrievalHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single-item and list retrieval by identifier, expression predicate, filter criteria, or anonymous object
- **`ISearchHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; paginated search with filter and sort criteria

**Write Handlers**

- **`ICreationHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single and batch entity creation
- **`IRevisionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single and batch entity revision
- **`IPhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; hard delete
- **`ILogicalDeletionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; soft delete and restore (composed over `IRevisionHandler`)
- **`IActivationHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; activate/deactivate (composed over `IRevisionHandler`)

### Supporting Types

- **Filtering** &ndash; `FilterCriteria` and `FilterOperation` for building query predicates
- **Sorting** &ndash; `SortCriteria` and `SortOrder` for ordering results
- **Options** &ndash; `CreationOptions`, `RevisionOptions`, and `PropertyHandling` for controlling automatic property assignment
- **Unit of Work** &ndash; `IUnitOfWork<TScope>` abstraction; `UnitOfWork<TScope, TDbConnection>` SQL implementation backed by `IDbTransaction` for transactional SQL workflows; provider packages document their own unit of work and transaction behavior
- **Exceptions** &ndash; `CreationException`, `RevisionException`, `PhysicalDeletionException`

### Design Patterns

- **Scope pattern** &ndash; all operations accept a `TScope?` representing the data store context (e.g., `IDbTransaction`)
- **Sync + async parity** &ndash; every operation has both synchronous and `Task`-returning async overloads
- **Optional mapper** &ndash; `IMapper?` enables mapping between domain entities and data records; null implies identity mapping

## Dependencies

- [YuckQi.Domain](https://www.nuget.org/packages/YuckQi.Domain)
- [YuckQi.Extensions.Mapping.Abstractions](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Abstractions)

## Installation

```shell
dotnet add package YuckQi.Data
```
