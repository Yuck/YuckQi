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

- **Filtering** &ndash; `FilterCriteria` and `FilterOperation` for building query predicates (`Contains` is substring match; `In` is set membership). In MemDb and the Dapper providers (MySQL, Oracle, PostgreSQL and SQL Server), `Contains` ignores case and a `null` member of `In` matches `null` values; other providers document their own behavior
- **Sorting** &ndash; `SortCriteria` and `SortOrder` for ordering results. Sort criteria are passed as an `IReadOnlyList<SortCriteria>` in priority order: the first criterion is the primary sort and each later one breaks ties in the ones before it. `SortCriteria.Expression` can be a property name or a dotted path such as `Address.City`; `SortExpression` and `ToSortCriteria` produce the same paths from `t => t.Address.City`
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
- `Create` throws `CreationException` when the provider returns `default(TIdentifier)` (such as `0` or `Guid.Empty`), not only `null`. The provider's result is not assigned to the entity's `Identifier` when this happens; an identifier set before the provider call, including one assigned by `IdentifierFactory`, stays. See the [YuckQi.Domain](https://www.nuget.org/packages/YuckQi.Domain) readme for how unset identifiers are defined. Providers undo their write where they can before the exception is raised; each provider readme says how.
- `CreationOptions` has a new `allowDefaultIdentifier` parameter (default `false`). Set it to `true` when `0`, `Guid.Empty` or another default value is a real identifier, such as an enum-backed lookup whose `Unknown` member is `0`; only `null` then counts as unset. Revision and deletion handlers do not reject default identifiers.
- The batch `Create` and `Revise` overloads in `CreationHandlerBase` and `RevisionHandlerBase` write every entity before returning and return a materialized list. They used to return a lazy sequence, so nothing was written until the result was enumerated, and each enumeration wrote again. Code that relied on discarding the result to skip the write must stop calling the batch overload.
- The async batch `Create` and `Revise` overloads in the base classes await each entity in turn instead of running them all at once with `Task.WhenAll`. Concurrent operations on one `DbContext` or `IDbTransaction` used to fail. A failing entity now stops the batch: later entities are not attempted, and earlier ones stay written unless the scope is rolled back. Providers that override the batch overloads with real batching are unchanged.
- The sync batch `Create` throws `ArgumentNullException` for a `null` scope before writing anything, like the other overloads.
- `Search` checks the page with `PageGuard.ThrowIfInvalid(page)` from [YuckQi.Domain](https://www.nuget.org/packages/YuckQi.Domain) and throws `ArgumentOutOfRangeException` before `DoSearch` or `DoCount` run when the page number or page size is below 1 or the page starts past `Int32.MaxValue`. A `null` page throws `ArgumentNullException`. The built-in providers compute their offset with `IPage.GetOffset()`; custom providers should do the same instead of computing `(PageNumber - 1) * PageSize` themselves.
- `IdentifierFactory` only runs when the entity's `Identifier` is `null` or `default` (`0`, `Guid.Empty`). It used to replace identifiers that callers set themselves. This check ignores `allowDefaultIdentifier`, so a factory still runs for a default value-type identifier; don't combine a factory with entities whose real identifier is the default value. An identifier the factory assigned stays on the entity if creation fails, so a retry reuses it.
- `ActivationHandler` and `LogicalDeletionHandler` put `ActivationMoment` / `DeletionMoment` back to its previous value when `Revise` throws. A failed `Activate`, `Deactivate`, `Delete` or `Restore` used to leave the new value on the entity, so a retry returned early and reported success without saving.
- Expression predicates with the value on the left now flip the operator. `t => 18 <= t.Age` used to become `Age <= 18` and now becomes `Age >= 18`. Code that worked around this by writing the reversed operator must change it back.
- The value side of a comparison or `Contains` can now be any expression that does not use the lambda parameter, and it is evaluated when the predicate is translated. This covers captured variables (`t => t.Email == email`), members of captured objects (`request.Name`), static members (`DateTimeOffset.MaxValue`) and computed values (`cutoff.AddDays(-1)`). Comparisons on nullable members (`t => t.Age == 5` on an `Int32?`) now work too. All of these used to throw `NotSupportedException`.
- Comparing an enum member to an enum value now produces a filter value of the enum type. It used to be the enum's underlying integer.
- The member side must be a chain of properties or fields on the lambda parameter, such as `t.Name` or `t.Address.City`. A cast inside the chain is skipped, so `((Derived) t.Base).Prop` becomes `Base.Prop`; it used to resolve to `Prop`. `.Value` on a nullable member is skipped too, so `t => t.Age.Value > 18` filters on `Age`; it used to filter on a field named `Age.Value`. `.HasValue` throws `NotSupportedException`; compare the member to `null` instead. Method calls (`t.Name.ToLower()`), the parameter itself, and members of captured or static objects now throw `NotSupportedException`. They used to produce an empty or partial field name. The same rule applies to the expressions passed to `IPropertyNameResolver.Resolve`, `WithMapping`, `FilterExpression` and `SortExpression`. A comparison where both sides use the parameter, such as `t => t.Name == t.Nickname`, or where neither side does, throws `NotSupportedException`.
- `ISearchHandler.Search` and `SearchHandlerBase.DoSearch` take `IReadOnlyList<SortCriteria>` instead of `IOrderedEnumerable<SortCriteria>`, and the list order is the sort priority. Pass the criteria as an array or list, such as `new[] { new SortCriteria("LastName", SortOrder.Descending), new SortCriteria("FirstName", SortOrder.Ascending) }`, instead of calling `.OrderBy(...)` on them. Code that sorted the criteria themselves, for example by `Expression`, changes the priority of the sort and must stop doing so.
- `ToSortCriteria` keeps the order of the sort expressions and returns `IReadOnlyList<SortCriteria>`. It used to sort the criteria by `SortOrder`, putting ascending criteria before descending ones, so `[LastName desc, FirstName asc]` became `[FirstName asc, LastName desc]`.
- `SortCriteriaExtensions.OrderBy(IQueryable<T>, IReadOnlyList<SortCriteria>)` applies the first criterion with `OrderBy` / `OrderByDescending` and the rest with `ThenBy` / `ThenByDescending`. Each criterion used to call `OrderBy` again, so Entity Framework sorted by the last criterion only and MemDb made the last criterion the primary sort. It now returns `IQueryable<T>` instead of `IOrderedQueryable<T>`, returns the source unchanged when there are no criteria (it used to add a constant `OrderBy(t => 1)`), and throws `ArgumentNullException` for `null` criteria. `ThenBy` and `ThenByDescending` overloads that take a field name are new.
- Dotted sort paths such as `Address.City` now work in MemDb and Entity Framework. They used to throw `ArgumentException`. For in-memory queries (`EnumerableQuery`, such as `AsQueryable()` over a collection), a `null` object along the path gives a `null` sort value instead of throwing; other providers handle `null` navigations themselves. A blank `SortCriteria.Expression` throws `ArgumentException`, as an unknown property name already did.

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
