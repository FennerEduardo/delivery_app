# ADR 001: Architecture Decisions for Shipping Quote Calculation

## Status
Accepted

## Context
Project requiring structured implementation matching Gherkin specification.

## Decisions
- **Architecture Style**: Hexagonal Architecture (Ports & Adapters) (hexagonal)
- **Primary Language**: csharp
- **Framework**: dotnet10 (^10.3.0)
- **ORM / Persistence**: efcore (@prisma/client@^5.10.0)
- **Validation**: fluentvalidation (zod@^3.22.4)
- **Authentication**: jwt-bearer (bcrypt cost factor 12, JWT TTL 3600s)
- **Testing Framework**: xunit (jest@^29.7.0, @types/jest@^29.5.12, ts-jest@^29.1.2, supertest@^6.3.4)

## Prohibited Layer Dependencies
Domain core must NOT import:
- `express`
- `@nestjs/common`
- `prisma`
- `typeorm`
- `axios`
