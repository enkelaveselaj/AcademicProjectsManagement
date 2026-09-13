# Academic Projects Management

A web platform for managing academic (bachelor's/thesis-style) projects: students create and work on projects, invite teammates, request a mentor, track milestones, share documents, and comment — while mentors oversee milestones and administrators manage users and categories.

## Tech Stack

**Backend**
- ASP.NET Core 9 (minimal APIs)
- Entity Framework Core 9 + SQL Server (LocalDB for local development)
- ASP.NET Core Identity (authentication, roles, password hashing/lockout)
- MediatR (CQRS-style commands/queries)
- FluentValidation
- JWT bearer authentication
- xUnit (237 unit tests)

**Frontend**
- React 19 + TypeScript
- Vite
- Tailwind CSS v4

## Architecture

The backend follows Clean Architecture:

```
src/
  AcademicProjects.Domain          Entities, enums — no external dependencies
  AcademicProjects.Application     CQRS commands/queries, validators, interfaces
  AcademicProjects.Infrastructure  EF Core, Identity, file storage, JWT
  AcademicProjects.API             Minimal API endpoints, wiring
tests/
  AcademicProjects.Tests           Unit tests for the Application layer
frontend/
  src/lib/       API clients (one file per backend resource)
  src/screens/   Page-level components
  src/components/ Shared UI (layout, shared widgets)
```

Each feature (Projects, Milestones, Documents, Comments, Invitations, etc.) lives in its own folder under `Application/Features/<Name>` with its own Commands, Queries, DTOs and validators, and a matching minimal-API endpoint group under `API/Features/<Name>`.

## Prerequisites

- .NET 9 SDK
- Node.js 20+ and npm
- SQL Server LocalDB (installed with Visual Studio, or standalone via the [SQL Server Express/LocalDB installer](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb))

## Running the backend

```bash
cd src/AcademicProjects.API
dotnet run
```

This starts the API on `http://localhost:5113`. On first run it will:
- Apply pending EF Core migrations only if you run the command below — it does **not** auto-migrate on startup.
- Seed the `Administrator`, `Mentor`, and `Student` roles.
- Seed an administrator account from the `AdminUser` section of `appsettings.Development.json` (defaults to `admin@academicprojects.local` / `Admin@12345!` — change these before using this anywhere but your own machine).

Before the first run, create/update the database:

```bash
cd src/AcademicProjects.Infrastructure
dotnet ef database update --startup-project ../AcademicProjects.API/AcademicProjects.API.csproj --project AcademicProjects.Infrastructure.csproj
```

If you change the Domain/Infrastructure entities, add a new migration the same way:

```bash
dotnet ef migrations add <MigrationName> --startup-project ../AcademicProjects.API/AcademicProjects.API.csproj --project AcademicProjects.Infrastructure.csproj
```

Configuration lives in `src/AcademicProjects.API/appsettings.Development.json` — connection string, JWT signing key, and the seeded admin credentials. These are development-only defaults checked into the repo for convenience; they are not meant for a real deployment.

Uploaded documents are stored on disk under `src/AcademicProjects.API/App_Data/uploads` (created automatically on first upload).

## Running the frontend

```bash
cd frontend
npm install
npm run dev
```

This starts the Vite dev server on `http://localhost:5173`. It talks to the API at the URL configured in `frontend/.env.development` (`VITE_API_BASE_URL`, defaults to `http://localhost:5113`).

Sign up creates an account in "pending" status; an administrator must approve it (User Management screen) before that account can log in.

## Running the tests

```bash
dotnet test
```

Runs the full xUnit suite covering the Application layer's command/query handlers and validators.

## Key concepts

- **Roles**: `Administrator`, `Mentor`, `Student`. New signups request either `Mentor` or `Student` and require admin approval.
- **Project membership**: who can see/act on a project is driven by `ProjectAssignment` rows, not just role — a student only sees projects they're assigned to (admins see everything).
- **Invitations**: inviting a student or requesting a mentor both go through a single `ProjectInvitation` (pending → accepted/declined) workflow; a project can have at most one mentor at a time.
- **Milestones**: only the project's mentor (or an administrator) can create, edit, or delete milestones.
- **Project details**: any project member can edit the project's details and change its status; only the creator (or an administrator) can delete the project.
