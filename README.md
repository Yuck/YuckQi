# YuckQi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET solution of lightweight, composable libraries for domain modeling, data access, and mapping. Use handler-based data abstractions to build repositories and services against SQL, document stores, or in-memory backends.

## Overview

YuckQi provides:

- **Domain** — Bootstrapping and validation for a domain model (YuckQi.Domain, YuckQi.Domain.Validation).
- **Data** — Handler-based data access (create, read, update, delete, search) with a scope/unit-of-work pattern and sync/async parity (YuckQi.Data).
- **Data implementations** — Concrete handlers for multiple backends so you can compose repositories without tying code to a single store.
- **Mapping** — Abstractions and adapters for object-to-object mapping (AutoMapper, Mapster).

Handlers follow a template-method pattern: you implement provider-specific logic (e.g., Dapper, Entity Framework, MongoDB), and the core library handles options, filtering, sorting, and optional mapping.

## Repository Structure

| Folder | Contents |
|--------|----------|
| **src/** | NuGet packages (net8.0). Each package may be published independently. |
| **test/** | Unit test projects aligned to each source project. |

### Source Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| **YuckQi.Domain** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Domain.svg)](https://www.nuget.org/packages/YuckQi.Domain) | Domain model bootstrapping. |
| **YuckQi.Domain.Validation** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Domain.Validation.svg)](https://www.nuget.org/packages/YuckQi.Domain.Validation) | Domain validation (FluentValidation). |
| **YuckQi.Extensions.Mapping.Abstractions** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Extensions.Mapping.Abstractions.svg)](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Abstractions) | Mapping abstractions (`IMapper`). |
| **YuckQi.Extensions.Mapping.AutoMapper** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Extensions.Mapping.AutoMapper.svg)](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.AutoMapper) | AutoMapper implementation. |
| **YuckQi.Extensions.Mapping.Mapster** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Extensions.Mapping.Mapster.svg)](https://www.nuget.org/packages/YuckQi.Extensions.Mapping.Mapster) | Mapster implementation. |
| **YuckQi.Data** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.svg)](https://www.nuget.org/packages/YuckQi.Data) | Core data handlers, `IUnitOfWork`, filtering/sorting, exceptions. |
| **YuckQi.Data.MemDb** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.MemDb.svg)](https://www.nuget.org/packages/YuckQi.Data.MemDb) | In-memory implementation (ConcurrentDictionary). |
| **YuckQi.Data.Sql.Dapper** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.Sql.Dapper.svg)](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper) | SQL via Dapper + SimpleCRUD (base). |
| **YuckQi.Data.Sql.Dapper.SqlServer** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.Sql.Dapper.SqlServer.svg)](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper.SqlServer) | SQL Server (Dapper). |
| **YuckQi.Data.Sql.Dapper.MySql** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.Sql.Dapper.MySql.svg)](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper.MySql) | MySQL (Dapper). |
| **YuckQi.Data.Sql.Dapper.Oracle** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.Sql.Dapper.Oracle.svg)](https://www.nuget.org/packages/YuckQi.Data.Sql.Dapper.Oracle) | Oracle (Dapper). |
| **YuckQi.Data.Sql.EntityFramework** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.Sql.EntityFramework.svg)](https://www.nuget.org/packages/YuckQi.Data.Sql.EntityFramework) | SQL via Entity Framework Core. |
| **YuckQi.Data.DocumentDb.MongoDb** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.DocumentDb.MongoDb.svg)](https://www.nuget.org/packages/YuckQi.Data.DocumentDb.MongoDb) | MongoDB. |
| **YuckQi.Data.DocumentDb.RavenDb** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.DocumentDb.RavenDb.svg)](https://www.nuget.org/packages/YuckQi.Data.DocumentDb.RavenDb) | RavenDB. |
| **YuckQi.Data.DocumentDb.DynamoDb** | [![NuGet](https://img.shields.io/nuget/v/YuckQi.Data.DocumentDb.DynamoDb.svg)](https://www.nuget.org/packages/YuckQi.Data.DocumentDb.DynamoDb) | Amazon DynamoDB. |

## Getting Started

Reference only what you need. For example, core data + SQL Server with Dapper:

```bash
dotnet add package YuckQi.Data
dotnet add package YuckQi.Data.Sql.Dapper.SqlServer
```

For domain and validation:

```bash
dotnet add package YuckQi.Domain
dotnet add package YuckQi.Domain.Validation
```

Each package has its own readme (in the project folder and on NuGet) with types and dependencies.

## Building and Testing

```bash
dotnet build
dotnet test
```

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

## Author

Kevin J Lambert
