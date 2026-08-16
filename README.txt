Tenant Task Manager

A small multi-tenant task management system with an ASP.NET Core API, an Angular web client, and a WPF desktop client.

How to run the project

Prerequisites

- .NET 10 SDK
- Visual Studio with ASP.NET and WPF
- SQL Server LocalDB
- Node.js 24 and npm

From the repository folder, restore the local EF Core tool:

  dotnet tool restore

The API keeps passwords and the JWT signing key in .NET user secrets. Run the
following PowerShell commands once:

  $jwtSecret = [Convert]::ToBase64String(
      [Security.Cryptography.RandomNumberGenerator]::GetBytes(48))

  dotnet user-secrets set "Jwt:Secret" $jwtSecret `
      --project src/TenantTaskManager.Api
  dotnet user-secrets set "DevelopmentSeed:AdminPassword" "Admin123!" `
      --project src/TenantTaskManager.Api
  dotnet user-secrets set "DevelopmentSeed:UserPassword" "User123!" `
      --project src/TenantTaskManager.Api
  dotnet user-secrets set "DevelopmentSeed:SecondUserPassword" "Other123!" `
      --project src/TenantTaskManager.Api

Start the API with its HTTP profile:

  dotnet run --project src/TenantTaskManager.Api --launch-profile http

The API runs at http://localhost:5010. In Development it applies pending EF
Core migrations and creates the sample tenants and users if they do not exist.

To run the Angular client, open a second PowerShell window:

  cd src/TenantTaskManager.Web
  npm ci
  npm start

Open http://localhost:4200. The Angular development server proxies /api calls
to the API on port 5010.

To run the WPF client, keep the API running and either start
TenantTaskManager.Desktop from Visual Studio or run:

  dotnet run --project src/TenantTaskManager.Desktop

Development accounts

  Demo tenant admin:  admin@tenanttask.local / Admin123!
  Demo tenant user:   user@tenanttask.local / User123!
  Other tenant user:  user@othertenant.local / Other123!

The Admin role can use the admin-only user-list endpoint. Both Admin and User
can work with tasks in their own tenant.

Run the .NET tests:

  dotnet test TenantTaskManager.sln

Run the Angular tests:

  cd src/TenantTaskManager.Web
  npm test -- --watch=false


Architecture overview

The backend follows an onion/layered structure:

- TenantTaskManager.Domain contains the Tenant, UserAccount, and TaskItem
  entities and their business rules. It has no infrastructure dependencies.
- TenantTaskManager.Application contains use-case handlers and abstractions
  for persistence, authentication, and the current tenant.
- TenantTaskManager.Infrastructure implements EF Core persistence, password
  hashing, JWT creation, development seeding, and the optimized task query.
- TenantTaskManager.Api contains the HTTP controllers, JWT middleware setup,
  current-tenant implementation, and centralized exception handling.
- TenantTaskManager.Web is a standalone Angular SPA.
- TenantTaskManager.Desktop is the WPF companion application.

The tenant ID is not accepted from either client. Login creates a JWT with a
tenant_id claim. The API reads that claim and EF Core global query filters apply
it to tenant-owned data. Requests for another tenant's task behave as if the
task does not exist.

The task-list read path is separated from the task repository. It uses
AsNoTracking and projects directly to TaskDto in SQL. Its filter and ordering
match the composite index on TenantId, IsCompleted, and CreatedAtUtc.


Key decisions and trade-offs

- SQL Server LocalDB was used instead of SQLite so development uses the same
  SQL Server provider expected by the assignment.
- A shared database with TenantId filters keeps the sample small. Separate
  databases or schemas would provide stronger isolation but add operational
  complexity.
- JWTs keep the Angular and WPF clients stateless. The Angular client stores
  its token in sessionStorage, which limits persistence but still requires the
  usual protection against script injection.
- The Angular application uses standalone components and reactive forms to
  avoid unnecessary module and form-state code.
- The WPF client uses a small API service with window code-behind. MVVM would
  be a better choice if the desktop application grew beyond this assignment.
- Integration tests replace SQL Server with an isolated EF Core in-memory
  database. They test the complete HTTP/authentication pipeline, while the
  SQL-specific migration and generated query are checked separately.
- Expected API exceptions are mapped to consistent problem responses. Each
  response includes a trace ID that can be matched to structured logs.


Libraries used and why

- Entity Framework Core SQL Server: database access, migrations, global query
  filters, indexes, and async queries.
- ASP.NET Core JwtBearer and Microsoft.IdentityModel.JsonWebTokens: JWT token
  creation and validation using standard middleware.
- Microsoft.Extensions.Identity.Core: the built-in PasswordHasher rather than
  storing or implementing password hashes directly.
- xUnit: unit and integration testing for the .NET projects.
- Microsoft.AspNetCore.Mvc.Testing: in-process API integration tests through
  WebApplicationFactory.
- EF Core InMemory: an isolated database for API and repository tests.
- coverlet.collector: optional .NET code-coverage collection.
- Angular reactive forms and router: form validation and protected client-side
  navigation.
- RxJS: Angular HTTP request composition and response handling.
- Vitest and jsdom: fast Angular unit tests without launching a full browser.

WPF uses only framework libraries. No UI toolkit was added for the small
desktop scope.


What I would improve with more time

- Deploy the API and web client to Azure and store the JWT secret in Key Vault.
- Add refresh tokens, token revocation, and account lockout/rate limiting.
- Add an Admin screen for creating and managing users and tenants.
- Add paging, searching, due dates, task assignment, and task deletion.
- Move the WPF application to MVVM and add desktop unit tests.
- Run integration tests against a temporary SQL Server instance in CI.
- Add end-to-end browser tests for the Angular workflow.


Known limitations

- The Angular development proxy and WPF client expect the API on port 5010.
- There is no refresh-token flow. Users sign in again when a JWT expires.
- Development passwords are sample credentials and must not be used in a real
  deployment.
- The sample uses one shared database and depends on application-enforced
  tenant filters rather than database-level row security.
- Azure deployment and Key Vault configuration are not included.


CI

.github/workflows/ci.yml builds the .NET solution, runs all .NET tests, builds
the Angular application, and runs the Angular tests for pushes and pull
requests to main.