# TwitterBackend

A Twitter-inspired backend API built with **C# and .NET 10**, focusing on layered architecture, secure authentication, request validation, and reusable application infrastructure.

## Overview

TwitterBackend is a backend project designed to explore maintainable API development through separation of concerns, a custom mediator implementation, and centralized request-processing behaviors.

## Architecture

The solution is divided into four projects:

| Project | Responsibility |
|---|---|
| **WebApi** | ASP.NET Core API entry point, HTTP endpoints, authentication configuration, and global exception handling. |
| **Application** | Application use cases, request and handler contracts, validation, and pipeline behaviors. |
| **Domain** | Core domain entities and interfaces. |
| **Infrastructure** | Entity Framework Core, SQL Server integration, repositories, and authentication-related services. |

## Features

### Authentication and Security

- JWT bearer authentication.
- Access-token and refresh-token handling.
- Password hashing.
- Refresh-token persistence and logout-related token handling.
- Configurable token expiration, issuer, and audience.
- Authentication and authorization middleware integration.

### Custom Mediator

- Custom mediator implementation for dispatching requests to their corresponding handlers.
- Generic request and handler abstractions.
- Pipeline behaviors for centralized application concerns.
- Cached handler invokers to reduce repeated reflection-based dispatch setup.

### Request Validation

- FluentValidation integration.
- Registration and login request validation.
- Email and input-length validation.
- Password complexity requirements.
- Date-of-birth and minimum-age validation where configured.

### Data Access

- Entity Framework Core.
- Microsoft SQL Server.
- Application database context abstraction.
- Repository abstractions and implementations.
- Cancellation-token support for relevant asynchronous operations.

### Pipeline Behaviors

The application includes implementations for:

- **Logging:** Centralized request-processing logs.
- **Exception handling:** Consistent handling of application exceptions.
- **Validation:** Request validation before handler execution.
- **Transactions:** Transaction handling for requests marked as transactional.
- **Idempotency:** Support for preventing duplicate processing of eligible requests.

These features are still subject to ongoing development and testing.

## Technology Stack

- **Language:** C#
- **Framework:** .NET 10 / ASP.NET Core
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer
- **Validation:** FluentValidation
- **Architecture:** Layered architecture
- **Request Handling:** Custom Mediator and Pipeline Behaviors

## Project Structure

- `WebApi` — API configuration and HTTP interface.
- `Application` — Application logic and request processing.
- `Domain` — Domain entities and abstractions.
- `Infrastructure` — Database access and external implementation details.

## Development Goals

- Maintain clear separation between application logic, domain models, and infrastructure.
- Centralize cross-cutting concerns through mediator pipeline behaviors.
- Improve security and consistency in authentication flows.
- Support cancellation in asynchronous operations.
- Build a maintainable foundation for a Twitter-style backend.
## Project Status

This project is under active development. Features and architecture may evolve as implementation and testing continue.
