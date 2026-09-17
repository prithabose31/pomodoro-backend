# FocusFlow --- Backend

This repository contains the ASP.NET Core backend for FocusFlow, a
full-stack productivity application for task management, weekly
planning, Pomodoro sessions, and productivity tracking.

## Related Repository

**Frontend:** https://github.com/prithabose31/pomodoro-frontend

## Architecture

The backend is organized into four main projects:

``` text
PomodoroApp.API
        ↓
PomodoroApp.Application
        ↓
PomodoroApp.Core

PomodoroApp.Infrastructure
        ↕
     Database
```

### Projects

#### `PomodoroApp.API`

HTTP/API layer.

Responsibilities include:

-   Controllers
-   Dependency injection
-   CORS
-   Authentication configuration
-   Swagger configuration
-   Application startup
-   Database migration execution

#### `PomodoroApp.Application`

Application/business layer.

Contains services such as:

-   `AuthService`
-   `TaskService`
-   `CategoryService`
-   `PomodoroService`

#### `PomodoroApp.Core`

Core domain layer containing entities, DTOs, interfaces, and
domain-level abstractions.

#### `PomodoroApp.Infrastructure`

Data-access and external implementation layer.

Contains:

-   Entity Framework Core `AppDbContext`
-   PostgreSQL configuration
-   Repositories
-   EF Core migrations

## Tech Stack

-   C#
-   ASP.NET Core 8
-   Entity Framework Core 8
-   PostgreSQL
-   Npgsql
-   JWT
-   HTTP-only cookies
-   BCrypt password hashing
-   Swagger / OpenAPI
-   Docker
-   Railway

The API targets `.NET 8` and uses Npgsql for PostgreSQL access.

## Database

The EF Core model contains:

``` text
Users
RefreshTokens
Categories
Tasks
Subtasks
PomodoroSessions
```

Relationships include:

-   A user can have multiple categories.
-   A user can have multiple tasks.
-   A category can contain multiple tasks.
-   A task can contain multiple subtasks.
-   A user can have refresh tokens.
-   Pomodoro sessions can be associated with a user and task.

## Authentication

FocusFlow uses JWT access tokens and refresh tokens stored in secure
HTTP-only cookies.

### Local authentication

Registration:

``` http
POST /api/auth/register
```

Login:

``` http
POST /api/auth/login
```

Current user:

``` http
GET /api/auth/me
```

Refresh:

``` http
POST /api/auth/refresh
```

Logout:

``` http
POST /api/auth/logout
```

Passwords are hashed using BCrypt before being stored.

The access token is generated as a JWT. Refresh tokens are generated
randomly, hashed before database storage, and the raw token is sent
through an HTTP-only cookie.

## API Areas

### Authentication

``` text
/api/auth
```

### Tasks

``` text
/api/tasks
```

Supported operations include:

-   Get tasks
-   Create task
-   Update task
-   Delete task
-   Log time against a task
-   Update weekly goal

### Categories

``` text
/api/categories
```

Supported operations include:

-   Get categories
-   Create category
-   Update category
-   Delete category

### Pomodoro

``` text
/api/pomodoro
```

Used to log and retrieve Pomodoro session history.

## CORS

The production API allows the deployed FocusFlow frontend:

``` text
https://focusflowapp.up.railway.app
```

Credentials are allowed because authentication uses cookies.

## Database Migrations

The application runs EF Core database migrations during startup:

``` csharp
db.Database.Migrate();
```

This allows the Railway PostgreSQL database to be initialized/updated
when the backend starts.

## Docker

The backend uses a multi-stage Docker build:

1.  .NET 8 SDK image for restore and publish
2.  ASP.NET 8 runtime image for execution

The container listens using the Railway-provided `PORT` environment
variable.

## Local Development

### Prerequisites

-   .NET 8 SDK
-   PostgreSQL
-   Git

### Configure the database

Set the `DefaultConnection` connection string in your local development
configuration.

Do not commit passwords, JWT secrets, or other credentials.

### Run the API

From the API project:

``` bash
dotnet run
```

### Build

``` bash
dotnet build
```

### EF Core migrations

From the solution root:

``` bash
dotnet ef migrations add MigrationName \
  --project ./PomodoroApp.Infrastructure \
  --startup-project ./PomodoroApp.API
```

Apply the migration:

``` bash
dotnet ef database update \
  --project ./PomodoroApp.Infrastructure \
  --startup-project ./PomodoroApp.API
```

## Deployment

The backend is deployed to Railway using the repository's Dockerfile.

Production secrets and database configuration are supplied through
Railway environment variables rather than committed configuration files.

## Important Production Note

OAuth configuration contains provider-specific callback URLs and
credentials. These must be configured for the deployed environment
before describing a specific OAuth provider as production-ready.
