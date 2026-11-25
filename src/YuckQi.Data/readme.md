# YuckQi.Data

A .NET library of lightweight data access handlers which can be used to compose repositories and domain services. Defines the core abstractions and base classes that provider-specific implementations extend.

## Key Concepts

### Handler Architecture

All data operations follow a **Template Method** pattern where public methods call abstract `DoXxx` methods that providers implement.

**Read Handlers**

- **`IRetrievalHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single-item and list retrieval by identifier, expression predicate, filter criteria, or anonymous object
- **`ISearchHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; paginated search with expression predicate, filter criteria, or anonymous object, plus sort criteria

**Write Handlers**

- **`ICreationHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single and batch entity creation
- **`IRevisionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; single and batch entity revision
- **`IPhysicalDeletionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; hard delete
- **`ILogicalDeletionHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; soft delete and restore (composed over `IRevisionHandler`)
- **`IActivationHandler<TDomainEntity, TIdentifier, TScope>`** &ndash; activate/deactivate (composed over `IRevisionHandler`)

### Supporting Types

- **Filtering** &ndash; `FilterCriteria` and `FilterOperation` for building query predicates (`Contains` is substring match; `In` is set membership)
- **Sorting** &ndash; `SortCriteria` and `SortOrder` for ordering results
- **Options** &ndash; `CreationOptions`, `RevisionOptions`, and `PropertyHandling` for controlling automatic property assignment
- **Unit of Work** &ndash; `IUnitOfWork<TScope>` abstraction; `UnitOfWork<TScope, TDbConnection>` SQL implementation backed by `IDbTransaction` for transactional SQL workflows; provider packages document their own unit of work and transaction behavior
- **Exceptions** &ndash; `CreationException`, `RevisionException`, `PhysicalDeletionException`

### Filter Parameters

The `Object parameters` overloads of `Get`, `GetList` and `Search` accept a `FilterCriteria`, a collection of `FilterCriteria`, a dictionary of field names to values, or an object whose public readable properties name the fields to filter on (indexers are skipped). Every filter in a collection must specify a `FilterOperation`; `FilterOperation.Unknown` (the default value) is rejected with an `ArgumentException`.

Anything else throws instead of becoming an empty filter: `null` throws `ArgumentNullException`; primitives, enums, types from `System` namespaces (such as `String`, `Guid`, `DateTime`, `BigInteger`, `Uri` and `KeyValuePair<,>`), other collections, objects with no readable properties, and inputs that produce no filters (such as an empty dictionary) throw `ArgumentException`. A `null` filter or one whose operation is `FilterOperation.Unknown` also throws `ArgumentException`. A value of the handler's identifier type passed to `Get(Object, ...)`, such as a boxed `Int32` or a non-null `Int32?` for an `Int32` key, is looked up by identifier.

`Get` and `GetList` with an empty filter collection throw `ArgumentException`; call `GetList(scope)` to get every entity. `Search` still accepts an empty `FilterCriteria` collection and pages through every entity.

Upgrading to 10.5.0:

- `FilterOperation.Unknown = 0` is new, and `NotEqual` is now `8` (it was `0`). A `FilterCriteria` deserialized or bound without an operation used to mean `NotEqual`; it is now rejected. Operations stored or sent as numbers must use the new value for `NotEqual`; other values are unchanged.
- `ToFilterCollection` and the `Object parameters` overloads throw for `null`, scalar values, non-filter collections and objects with no readable properties, where they used to produce no filters and run an unfiltered query.
- `Get` with an empty filter collection throws instead of returning the first entity, and `GetList` with one throws instead of returning every entity; use `GetList(scope)` for that.
- Empty dictionaries and other `Object` parameters that produce no filters throw in `Get`, `GetList` and `Search`.
- Dictionaries are now read as field name/value pairs instead of filtering on `Comparer`, `Count`, `Keys` and `Values`.
- `Create` throws `CreationException` when the provider returns `default(TIdentifier)` (such as `0` or `Guid.Empty`), not only `null`. The entity's `Identifier` is left unchanged when this happens. See the [YuckQi.Domain](https://www.nuget.org/packages/YuckQi.Domain) readme for how unset identifiers are defined. Providers undo their write where they can before the exception is raised; each provider readme says how.
- `CreationOptions` has a new `allowDefaultIdentifier` parameter (default `false`). Set it to `true` when `0`, `Guid.Empty` or another default value is a real identifier, such as an enum-backed lookup whose `Unknown` member is `0`; only `null` then counts as unset. Revision and deletion handlers do not reject default identifiers.

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
