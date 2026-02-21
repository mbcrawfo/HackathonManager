# HackathonManager Overview

This project is a web application that organizations can use to manage hackathons within their org.

## Project Structure

```
/<project-root>
    /.github -  Github CI configuration.  Do not modify unless explicitly instructed to do so.
    /docker -  Dockerfiles and supporting configuration.
    /docs
        /database -  Database schema documentation.  It is automatically generated, do not modify.
    /scripts -  Helper scripts for developer workflows and CI usage.
    /src
        /hackathon-spa -  The front end Vue SPA application.
        /HackathonManager -  The back end .Net REST API.
        /HackathonManager.Migrator -  Database migration application.
    /tests
        /HackathonManager.Tests -  .Net test project for the back end including database tests, unit tests, and integration tests.
```

The .Net projects make use of `Directory.Build.props` for standardized settings, global package management in `Directory.Packages.props`, and lock files to produce frozen dependency trees.

A node.js package.json is configured at the root of the project so that scripts and tools can be used throughout the repo.  Specific applications using node, such as hackathon-spa, are configured as workspaces.

## Project Architecture

### Database

The application's data store is a PostgreSQL database. Postgres v18 is used for its support of UUIDv7.  The easiest way to see the current database schema is to look at the documentation in `docs/database`.

The database is managed with the HackathonManager.Migrator tool, which is a .Net 9 command line tool that applied migrations using DbUp.

 - Schema is updated using sql migration files located in `src/HackathonManager.Migrator/Migrations`.
 - Functions, views, etc. are managed with sql files in `src/HackathonManager.Migrator/Everytime`.
 - Tests of database features can be found in the back end test project in the folder `tests/HackathonManager.Tests/DatabaseTests`.

### Back End API

The REST API back end is built using .Net 9 using FastEndpoints.

Features:
 - Uses a Vertical Slice Architecture structure.
 - OpenAPI/Swagger documentation.
 - RESTful resource based urls and use of HTTP verbs.
 - Standard usage of HTTP status codes to indicate success or the type of error.
 - RFC-9457 Problem Details error responses.

Core libraries:
 - FastEndpoints
 - FluentAssertions
 - Microsoft.EntityFrameworkCore
 - NodaTime
 - Npgsql

Unit tests for the API can be found in the folder `tests/HackathonManager.Tests/UnitTests`.  Integration tests of the API are found in the folder `tests/HackathonManager.Tests/IntegrationTests`.

### Front End SPA

The SPA application is built using Vue 3 and Typescript.

Details coming soon.

## Coding Standards

### C#

Coding standards:
- Always prefer using modern .Net features.
- Always use nullable reference types.
- Always perform null checks using `is null` or `is not null` instead of equality operators.
- Use pattern matching when checking multiple constant values, for example `enumVar is MyEnum.Val1 or MyEnum.Val2`.
- When validating parameters, use exception helper methods such as `ArgumentNullException.ThrowIfNull(variable)` whenever available.
- Always include curly braces when they are optional.
- Always give enum members an explicit value.
- Enum member values must always start with 1 UNLESS the enum includes a member such as 'None' or 'Not Provided'.
- Use sealed classes or records unless the type is meant to be extended.

Tools:
- Always use `dotnet build` to get compiler feedback after making code changes.  The project uses nullable reference types, warnings as errors, and includes several Roslyn analyzer packages to enforce code quality.
- Always run tests using `dotnet test` after making changes.
- When you complete changes to C# code or project files, you should always run Csharpier to format them correct.
  - `dotnet csharpier check <file_or_directory>`: check formatting of files
  - `dotnet csharpier format <file_or_directory>`: apply formatting to files

### SQL:

After modifying sql files, always run use sqlfluff to lint the changes.
- `npm run sqlfluff lint <paths...>`: lint the provided files
- `npm run sqlfluff format <paths...>`: apply automatic formatting to the provided files

Sqlfluff runs inside of a docker container with the project root folder mapped to the `/sql` folder in the container.  Any paths provided as arguments must be prefixed with `/sql`, for example `path/to/file.sql` would become `/sql/path/to/file.sql`.

### Typescript/Vue

Coding standards:
- Always use types.  If you must use `any`, you must include a comment explaining why it is used.
- Always prefer using arrow functions.
- Always prefer using async/await instead of chaining promises with `.then(...)`.

Always run Prettier after modifying ts or vue files.
  - `npm run format`: reformat files with prettier
  - `npm run format:check`: use prettier to check formatting

After modifying code in the hackathon-spa project:
- Use ESLint to check linting.
  - `npm run spa lint`: run ESLint and automatically apply fixes
  - `npm run spa lint:check`: run ESLint to check linting
- Run `npm run spa check` to run the Typescript compiler and validate type usage.


