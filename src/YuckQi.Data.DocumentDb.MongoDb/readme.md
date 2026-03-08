# YuckQi.Data.DocumentDb.MongoDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for MongoDB databases.

## Key Types

Provides concrete handler implementations that operate over an `IClientSessionHandle` scope:

- **`CreationHandler`** &ndash; inserts documents into MongoDB collections
- **`RevisionHandler`** &ndash; replaces existing documents
- **`PhysicalDeletionHandler`** &ndash; deletes documents
- **`RetrievalHandler`** &ndash; retrieves documents by identifier or filter criteria
- **`SearchHandler`** &ndash; paginated document search
- **`UnitOfWork`** &ndash; unit of work backed by `IClientSessionHandle` with transaction support
- **`DatabaseAttribute`** / **`CollectionAttribute`** &ndash; attributes for specifying database and collection names on document types
- **`DocumentModelExtensions`** / **`FilterDefinitionExtensions`** &ndash; helpers for collection resolution and filter translation

All handlers provide overloads with an explicit `TDocument` type parameter for domain-to-document mapping.

## Dependencies

- [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.DocumentDb.MongoDb
```
