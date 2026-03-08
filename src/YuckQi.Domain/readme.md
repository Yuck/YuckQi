# YuckQi.Domain

A .NET library for bootstrapping a domain model project. Provides base entity types, value objects, and aspect interfaces for building rich domain models.

## Key Types

### Entities

- **`IDomainEntity<TIdentifier>`** &ndash; interface for entities with a typed identifier
- **`DomainEntityBase<TIdentifier>`** &ndash; abstract record implementing `IDomainEntity` with a required `Identifier` property
- **`TypeEntityBase<TIdentifier>`** &ndash; abstract record for lookup/type entities with `Name` and optional `ShortName`

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
