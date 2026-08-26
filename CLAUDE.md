# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build MySolution.slnx          # build entire solution
dotnet run --project MyProject.API    # API, http://localhost:5108
dotnet run --project View             # Blazor Server UI, http://localhost:5144
```

There are no test projects in the solution. To see the UI working end to end, both `MyProject.API` and `View` must be running at the same time — `View` talks to the API over HTTP, not via a project reference.

## Stack

- **.NET 10**, C#, nullable + implicit usings enabled on all projects
- **Blazor Server** (`View`) — Interactive Server render mode, `net10.0`, `Microsoft.NET.Sdk.Web`
- **ASP.NET Core Web API** (`MyProject.API`) — controller-based, OpenAPI enabled in Development, CORS wide open (`AllowAll`)
- **MySQL** via `MySql.Data` (Oracle's connector, not EF Core) — raw ADO.NET (`MySqlConnection`/`MySqlCommand`/`MySqlDataReader`) in `DAL`
- Database name: `online_store`

## Architecture

4-project layered solution, referenced solution-wide via `MySolution.slnx`:

```
Model   → plain POCOs, no dependencies (BaseEntity, Person, Product, Purchase)
DAL     → data access, depends on Model + MySql.Data (BaseDB, PersonDB, PurchaseDB)
MyProject.API → ASP.NET Core Web API, depends on DAL + Model (Controllers/)
View    → Blazor Server UI, depends on Model ONLY — talks to MyProject.API over HttpClient, not a project reference
```

`View` has no reference to `MyProject.API` or `DAL`; it only shares the `Model` types and calls the API's HTTP endpoints via an injected `HttpClient` whose `BaseAddress` is hardcoded in `View/Program.cs` to `http://localhost:5108/` (must match the API's `launchSettings.json` port).

### DAL / connection string handling

- `BaseDB.connectionString` is a `protected readonly` field set via constructor — no hardcoded value in code. `PersonDB` and `PurchaseDB` both take `connectionString` in their constructor and pass it to `base()`.
- The real connection strings live **only** in .NET User Secrets for `MyProject.API` (`UserSecretsId` in its `.csproj`, store at `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`), never in a file git tracks:
  - `ConnectionStrings:Local` — local MySQL (`localhost`, database `online_store`).
  - `ConnectionStrings:Remote` — the active one; Aiven Cloud MySQL (SSL required).
- `MyProject.API/Program.cs` reads `builder.Configuration.GetConnectionString("Remote")` and passes it into the `PersonDB`/`PurchaseDB` DI registrations — both controllers now get the DAL classes via constructor injection (no more `new PersonDB()`/`new PurchaseDB()` inside controllers).
- To point a fresh clone at a database, run `dotnet user-secrets set "ConnectionStrings:Remote" "..."` (or `:Local`) inside `MyProject.API/` — `appsettings.json` intentionally has no `ConnectionStrings` section.

### Naming mismatch

- `MyProject.API/Controllers/CustomersController.cs` actually defines a class named `PersonController` (route `api/Person`, not `api/Customers`) — grep by class name, not filename, when looking for controllers.

### Comments

Existing code comments (DAL, Program.cs) are written in Hebrew — match that convention when adding comments to those files.
