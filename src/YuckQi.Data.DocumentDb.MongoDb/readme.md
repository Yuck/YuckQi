# YuckQi.Data.DocumentDb.MongoDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for MongoDB databases.

## Key Types

Provides concrete handler implementations that operate over an `IClientSessionHandle` scope:

- **`CreationHandler`** &ndash; inserts documents into MongoDB collections
- **`RevisionHandler`** &ndash; replaces existing documents
- **`PhysicalDeletionHandler`** &ndash; deletes documents
- **`RetrievalHandler`** &ndash; retrieves documents by identifier or filter criteria
- **`SearchHandler`** &ndash; paginated document search
- **`UnitOfWork`** &ndash; unit of work backed by `IClientSessionHandle`; when a MongoDB transaction is active, `SaveChanges()` / `SaveChanges(CancellationToken)` commit the transaction and rotate the session; operations performed outside an explicit transaction are persisted immediately by MongoDB and `SaveChanges` simply rotates the session
- **`DatabaseAttribute`** / **`CollectionAttribute`** &ndash; attributes for specifying database and collection names on document types
- **`DocumentModelExtensions`** / **`FilterDefinitionExtensions`** &ndash; helpers for collection resolution and filter translation

All handlers provide overloads with an explicit `TDocument` type parameter for domain-to-document mapping.

A single-entity `Create` whose inserted document has an unset `_id` (`null`, or `0` / `Guid.Empty` for value types unless `CreationOptions` is created with `allowDefaultIdentifier: true`) deletes that document in the same session before throwing `CreationException`.

Upgrading to 10.5.0:

- Creating a document whose `_id` ends up `0` or `Guid.Empty` now deletes it again and throws `CreationException`. It used to be inserted and reported as created.
- `Search` throws `InvalidOperationException` when more than `Int32.MaxValue` documents match, because `TotalCount` cannot hold the count. It used to report a negative total.
- `Search` no longer returns the whole collection for a page size of `0`; invalid pages are rejected before the query runs.

## Dependencies

- [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.DocumentDb.MongoDb
```
