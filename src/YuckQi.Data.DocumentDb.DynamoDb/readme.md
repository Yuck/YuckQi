# YuckQi.Data.DocumentDb.DynamoDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for Amazon DynamoDB databases.

## Key Types

Provides concrete handler implementations that operate over an `IDynamoDBContext` scope:

- **`CreationHandler`** &ndash; creates documents in DynamoDB
- **`RevisionHandler`** &ndash; updates existing documents
- **`PhysicalDeletionHandler`** &ndash; deletes documents
- **`RetrievalHandler`** &ndash; retrieves documents by identifier or filter criteria
- **`SearchHandler`** &ndash; paginated document search
- **`UnitOfWork`** &ndash; unit of work backed by `IDynamoDBContext`; DynamoDB operations are persisted immediately by `IDynamoDBContext`, and `SaveChanges()` / `SaveChanges(CancellationToken)` are no-ops (no transaction aggregation)
- **`FilterCriteriaExtensions`** / **`FilterOperationExtensions`** &ndash; translate `FilterCriteria` into DynamoDB scan conditions

All handlers provide overloads with an explicit `TDocument` type parameter for domain-to-document mapping.

DynamoDB does not generate keys, so a single-entity `Create` throws `InvalidOperationException` before writing when `Identifier` is unset: `null`, or the default value for value-type identifiers (`0`, `Guid.Empty`) unless `CreationOptions` is created with `allowDefaultIdentifier: true`. Set the identifier yourself or pass an `IdentifierFactory` in `CreationOptions`.

Upgrading to 10.5.0:

- A single-entity `Create` with an unset identifier now throws instead of writing an item keyed `0` / `Guid.Empty`.

## Dependencies

- [AWSSDK.DynamoDBv2](https://www.nuget.org/packages/AWSSDK.DynamoDBv2)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.DocumentDb.DynamoDb
```
