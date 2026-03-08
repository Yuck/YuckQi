# YuckQi.Data.DocumentDb.DynamoDb

An implementation of [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data) for Amazon DynamoDB databases.

## Key Types

Provides concrete handler implementations that operate over an `IDynamoDBContext` scope:

- **`CreationHandler`** &ndash; creates documents in DynamoDB
- **`RevisionHandler`** &ndash; updates existing documents
- **`PhysicalDeletionHandler`** &ndash; deletes documents
- **`RetrievalHandler`** &ndash; retrieves documents by identifier or filter criteria
- **`SearchHandler`** &ndash; paginated document search
- **`UnitOfWork`** &ndash; unit of work backed by `IDynamoDBContext`
- **`FilterCriteriaExtensions`** / **`FilterOperationExtensions`** &ndash; translate `FilterCriteria` into DynamoDB scan conditions

All handlers provide overloads with an explicit `TDocument` type parameter for domain-to-document mapping.

## Dependencies

- [AWSSDK.DynamoDBv2](https://www.nuget.org/packages/AWSSDK.DynamoDBv2)
- [YuckQi.Data](https://www.nuget.org/packages/YuckQi.Data)

## Installation

```shell
dotnet add package YuckQi.Data.DocumentDb.DynamoDb
```
