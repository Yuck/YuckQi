# YuckQi.Data.DocumentDb.RavenDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for RavenDB document databases.

## Key Types

Provides concrete handler implementations that operate over an `IAsyncDocumentSession` scope:

- **`CreationHandler`** &ndash; stores documents in RavenDB
- **`RevisionHandler`** &ndash; updates existing documents
- **`PhysicalDeletionHandler`** &ndash; deletes documents
- **`RetrievalHandler`** &ndash; retrieves documents by identifier or filter criteria
- **`SearchHandler`** &ndash; paginated document search
- **`UnitOfWork`** &ndash; unit of work backed by `IAsyncDocumentSession`; `SaveChanges()` / `SaveChanges(CancellationToken)` flush all pending session changes to the server using RavenDB's session-level unit of work (not a database transaction in the relational sense)
- **`DatabaseAttribute`** / **`CollectionAttribute`** &ndash; attributes for specifying database and collection names on document types (optional; RavenDB infers collection from type by default)
- **`DocumentModelExtensions`** &ndash; collection and database name resolution, identifier handling, and document ID conversion
- **`DocumentQueryExtensions`** &ndash; applies `FilterCriteria` to RavenDB document queries (`IDocumentQuery` / `IAsyncDocumentQuery`)

All handlers provide overloads with an explicit `TDocument` type parameter for domain-to-document mapping.

Document types should expose a string `Id` property for RavenDB document identity. Use `[DatabaseAttribute]` and `[CollectionAttribute]` when targeting multiple databases or custom collection names.

## Dependencies

- [RavenDB.Client](https://www.nuget.org/packages/RavenDB.Client)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.DocumentDb.RavenDb
```
