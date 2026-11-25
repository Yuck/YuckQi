# YuckQi.Domain

A .NET library for bootstrapping a domain model project. Provides base entity types, value objects, and aspect interfaces for building rich domain models.

## Key Types

### Entities

- **`IDomainEntity<TIdentifier>`** &ndash; interface for entities with a typed `Identifier` that may be unset
- **`DomainEntityBase<TIdentifier>`** &ndash; abstract record implementing `IDomainEntity`
- **`TypeEntityBase<TIdentifier>`** &ndash; abstract record for lookup/type entities with `Name` and optional `ShortName`
- **`IdentifierExtensions.IsSet()`** &ndash; returns whether an identifier has been assigned

#### Unset identifiers

`Identifier` is declared `TIdentifier?`, but that only makes it nullable for reference types. For value types it is never null: an unassigned `Int32` identifier is `0` and an unassigned `Guid` is `Guid.Empty`. `identifier.IsSet()` therefore treats both `null` and `default(TIdentifier)` as unset. A value-object identifier counts as unset only when it equals `default`: a `record struct` wrapping `Guid.Empty` is unset, while a non-null class instance is always set.

#### Equality

Entities are records, so they compare by value: two instances are equal when their runtime type and every property, including `Identifier`, are equal. Two distinct unsaved entities with the same values are equal, and the same entity before and after a change is not. Assigning `Identifier` (which creation handlers do after an insert) or changing any other property also changes the hash code. Don't keep entities in a `HashSet`, a dictionary key or anything else that relies on a stable hash code while they can still change; key by `Identifier` instead.

Upgrading to 10.5.0:

- `IdentifierExtensions.IsSet()` is new. The creation handlers in [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) use it by default, so a created entity whose identifier is `0` or `Guid.Empty` is treated like one whose identifier is `null`. Entities that really use the default value as an identifier need `CreationOptions` with `allowDefaultIdentifier: true`.

### Value Objects

- **`IPage` / `IPage<T>`** &ndash; interfaces for paging metadata (`PageNumber`, `PageSize`, `Items`, `TotalCount`)
- **`Page` / `Page<T>`** &ndash; record implementations of the paging interfaces

### Aspects

Optional audit interfaces that domain entities can implement:

| Interface | Purpose |
|-----------|---------|
| `ICreationMoment` | Creation timestamp |
| `IRevisionMoment` | Last-revised timestamp |
| `IDeletionMoment` | Soft-delete timestamp |
| `IActivationMoment` | Activation timestamp |
| `ICreatedBy<TIdentity>` | Creator identity |
| `IRevisedBy<TIdentity>` | Last-reviser identity |

## Installation

```shell
dotnet add package YuckQi.Domain
```
