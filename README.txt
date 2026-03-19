# HotelSyncApi - Opera Cloud and HubSpot Integration

This project is an API developed in .NET 8 and C# designed to synchronize reservation data between Oracle Opera Cloud and HubSpot CRM.

## Project Status
Currently, the project has completed Phase 2, achieving data persistence in SQL Server and exposing functional endpoints tested with Postman.

## Features
- **Architecture:** Web API with Dependency Injection.

- **Database:** SQL Server (using `Microsoft.Data.SqlClient`).

- **Endpoints:**

- `POST /api/Sync`: Receives integration data and records it in the `SyncRecords` table.

- **Logging:** Status tracing system (`PENDING`, `SUCCESS`, `ERROR`).

## Requirements
- .NET 8 SDK
- SQL Server Express
- Postman (for endpoint testing)