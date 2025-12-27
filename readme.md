# ![RealWorld Example App](logo.png)

ASP.NET Core codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld-example-apps) spec and API.

## [RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with ASP.NET Core (with Feature orientation) including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the ASP.NET Core community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

## How it works

This is using ASP.NET Core with:

- CQRS and [MediatR](https://github.com/jbogard/MediatR)
  - [Simplifying Development and Separating Concerns with MediatR](https://blogs.msdn.microsoft.com/cdndevs/2016/01/26/simplifying-development-and-separating-concerns-with-mediatr/)
  - [CQRS with MediatR and AutoMapper](https://lostechies.com/jimmybogard/2015/05/05/cqrs-with-mediatr-and-automapper/)
  - [Thin Controllers with CQRS and MediatR](https://codeopinion.com/thin-controllers-cqrs-mediatr/)
- [AutoMapper](http://automapper.org)
- [Fluent Validation](https://github.com/JeremySkinner/FluentValidation)
- Feature folders and vertical slices
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/) on SQLite for demo purposes. Can easily be anything else EF Core supports. Open to porting to other ORMs/DBs.
- Built-in Swagger via [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Bullseye](https://github.com/adamralph/bullseye) for building!
- JWT authentication using [ASP.NET Core JWT Bearer Authentication](https://github.com/aspnet/Security/tree/master/src/Microsoft.AspNetCore.Authentication.JwtBearer).
- Use [dotnet-format](https://github.com/dotnet/format) for style checking
- `.editorconfig` to enforce some usage patterns

This basic architecture is based on this reference architecture: [https://github.com/jbogard/ContosoUniversityCore](https://github.com/jbogard/ContosoUniversityCore)

## Getting started

Install the .NET Core SDK and lots of documentation: [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core)

Documentation for ASP.NET Core: [https://docs.microsoft.com/en-us/aspnet/core/](https://docs.microsoft.com/en-us/aspnet/core/)

## Docker Build

There is a 'Makefile' for OS X and Linux:

- `make build` executes `docker-compose build`
- `make run` executes `docker-compose up`

The above might work for Docker on Windows

## Local building

- It's just another C# file!   `dotnet run -p build/build.csproj`

## Swagger URL

- `http://localhost:5000/swagger`

## GitHub Actions build

![Build and Test](https://github.com/gothinkster/aspnetcore-realworld-example-app/workflows/Build%20and%20Test/badge.svg)

---

## Recent Improvements

The following enhancements have been added to improve performance, security, and observability:

### 🚀 Cursor-Based Pagination

- Implemented cursor-based pagination for the Articles endpoint, replacing traditional offset-based pagination for better performance with large datasets
- Uses `AsNoTracking()` to improve query performance and prevent unnecessary database connections from being held open
- Response now includes `NextCursor` and `HasMore` fields for seamless infinite scrolling support
- Backward compatible: offset-based pagination still works for existing clients

**Usage:**
```
GET /articles?limit=10
→ Response: { articles: [...], nextCursor: "abc123", hasMore: true }

GET /articles?cursor=abc123
→ Returns the next page of results
```

### 🛡️ Rate Limiting (Brute Force Protection)

- Added `AddRateLimiter()` with two distinct policies using the Fixed Window algorithm:
  - **"general"**: 100 requests per minute per IP for all API endpoints
  - **"login"**: 5 requests per minute per IP for the login endpoint (brute force protection)
- Returns `429 Too Many Requests` when the limit is exceeded
- Applied `[EnableRateLimiting("login")]` attribute to the login endpoint

### 📊 Structured Logging with Serilog

- Added `RequestLoggingMiddleware` for comprehensive HTTP request logging including method, path, status code, and response time
- Configured rolling file output to `logs/conduit-{date}.log`
- Automatic log retention: logs older than 30 days are automatically deleted
- Added `Serilog.Sinks.File` NuGet package (version 6.0.0) for file-based logging
- Log level overrides for Microsoft and Entity Framework Core namespaces to reduce noise

### ⚙️ Other Changes

- Updated SDK version to `8.0.100` in `global.json` for compatibility
- Added package version to central package management (`Directory.Packages.props`)
