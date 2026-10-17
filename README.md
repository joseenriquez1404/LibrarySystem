# LibrarySystem

A REST API for a library and loan system, built with ASP.NET Core (.NET 10), Entity Framework Core and SQL Server. It is a learning project meant for my portfolio, and it is still in development.

The goal: a librarian lends and receives books, a reader browses and reserves, and an admin manages everything, with the business rules covered by tests and a reproducible deployment.

## Project status

The project moves in phases. The full checklist is in [`LibrarySystem/TODO.txt`](LibrarySystem/TODO.txt) (written in Spanish).

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Project base, models, migrations and a real database | Done |
| 2 | Authentication and roles | Almost done. Missing: the final password hash (bcrypt or Argon2) and per-resource permissions, which depend on loans. The refresh token was postponed. |
| 3 | Catalog | In progress. Categories and authors are done. Books, copies, search and pagination are missing. |
| 4 | Loans, returns, renewals, fines and reservations | Pending |
| 5 | Tests, concurrency and auditing | Pending. There is no test project yet. |
| 6 | Docker, CI and documentation | Pending |

## What works today

- **JWT authentication.** Reader registration and login with a 60-minute access token.
- **Three roles:** reader, librarian and admin, stored in the database. Only an admin changes roles, and nobody can change their own role.
- **Login rate limiting:** 5 attempts per minute per IP, configurable. It answers 429 with a `Retry-After` header.
- **Tokens that reflect the user's current state.** If a user is deactivated or their role changes, the token stops working on the next request (401).
- **User management** for admins: create users (with any role), list them, change roles and activate or deactivate. Users are never deleted, only deactivated.
- **Audit log.** Creating users, changing roles and activating or deactivating are recorded with the previous and new state, and admins read them through the API.
- **Partial catalog.** CRUD for categories and authors. Category names are unique, and a category or author that has books cannot be deleted.
- **Uniform errors.** Domain exceptions are translated to `ProblemDetails` responses with a `code` field in a single place.
- **Interactive documentation** with OpenAPI and Scalar in Development.

## Endpoints

All of them require a token, except `health`, `register` and `login`.

| Method and route | Who |
| --- | --- |
| `GET /health` | Public |
| `POST /api/auth/register` | Public, creates a reader |
| `POST /api/auth/login` | Public, rate limited |
| `GET /api/users/me` | Any authenticated user |
| `GET /api/users`, `GET /api/users/{id}` | Admin |
| `POST /api/users` | Admin |
| `PUT /api/users/{id}/role` | Admin |
| `PUT /api/users/{id}/active` | Admin |
| `GET /api/audit` | Admin, with `entity`, `actorId` and `limit` filters |
| `GET /api/categories`, `GET /api/categories/{id}` | Any authenticated user |
| `POST`, `PUT /{id}` and `DELETE /{id}` on `/api/categories` | Librarian and admin |
| `GET /api/authors`, `GET /api/authors/{id}` | Any authenticated user |
| `POST`, `PUT /{id}` and `DELETE /{id}` on `/api/authors` | Librarian and admin |

## Stack

- .NET 10 and ASP.NET Core with controllers
- Entity Framework Core 10 with SQL Server
- JWT with `Microsoft.AspNetCore.Authentication.JwtBearer`
- Built-in ASP.NET Core rate limiting
- OpenAPI and Scalar

## Running it

You need the .NET 10 SDK and a SQL Server instance. The default Development configuration uses `localhost` with Windows authentication.

```bash
git clone https://github.com/joseenriquez1404/LibrarySystem.git
cd LibrarySystem
dotnet tool restore
dotnet run --project LibrarySystem
```

In Development the app applies the migrations and loads sample data when the users table is empty. The API listens on `http://localhost:5126`.

- Interactive documentation: `http://localhost:5126/scalar/v1`
- OpenAPI specification: `http://localhost:5126/openapi/v1.json`

### Sample users

They only exist in Development, created by the seeder.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@biblioteca.local` | `Admin123!` |
| Librarian | `bibliotecario@biblioteca.local` | `Biblio123!` |
| Reader | `lector1@biblioteca.local` | `Lector123!` |

### Configuration

The JWT signing key is the only secret. The issuer, audience, token lifetime and rate limit live in `appsettings.json`.

| Variable | Description |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Key` | Signing key, at least 32 characters. The app does not start without it. |

[`.env.example`](.env.example) shows the format. The key in `appsettings.Development.json` is for local development only. Any other environment must define its own.

### Migrations

```bash
dotnet dotnet-ef migrations add <Name> --project LibrarySystem/LibrarySystem.csproj
dotnet dotnet-ef database update --project LibrarySystem/LibrarySystem.csproj
```

## Planned business rules

They are implemented in phase 4, each with at least one test:

1. A reader has at most 3 active loans.
2. The loan period is 14 days, with up to 2 renewals of 7 days.
3. A loan cannot be renewed if it is overdue or if another reader has an active reservation for the book.
4. Fine of 5 pesos per day late, capped at 100 pesos per loan.
5. A reader with pending fines or overdue loans cannot borrow again.
6. Only a book with no available copies can be reserved, and the queue is first come, first served.
7. A reservation expires 3 days after a copy becomes available for that reader.
8. A copy has at most one active loan at a time.
9. A book with borrowed copies or loan history is never deleted, only deactivated.

## Structure

```
LibrarySystem/
├── Controllers/   HTTP endpoints
├── Service/       Business logic and data access
├── Dtos/          Requests and responses
├── Models/        Entities and enums
├── Data/          DbContext and seeder
├── Exceptions/    Domain exceptions and their HTTP translation
├── Migrations/    EF Core migrations
└── Program.cs     Configuration and startup
skills/            Style guides I use to generate code with Claude Code
```

## Decisions and known debt

- **Password hashing.** It currently uses ASP.NET Core's `PasswordHasher` (salted PBKDF2). The plan is to switch to Argon2id.
- **Layers.** Services use `AppDbContext` directly. Splitting domain, application and infrastructure with repositories is pending.
- **Pagination.** Listings return everything for now. It comes back with book search.
- **Refresh token.** Postponed. Only an access token exists.

## AI usage

I use Claude Code as a development assistant. [`LibrarySystem/CLAUDE.md`](LibrarySystem/CLAUDE.md) and the [`skills/`](skills/) folder hold the instructions and conventions I give it. The design decisions and the code review are mine.
