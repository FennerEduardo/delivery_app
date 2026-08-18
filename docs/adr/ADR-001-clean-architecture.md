# ADR-001: Clean Architecture Adoption

## Context
The Logistics Shipping Platform requires high maintainability, testability, and independence from frameworks, UI, and database infrastructure.

## Decision
We adopt **Clean Architecture** (Onion Architecture) with explicit project boundary layers:
1. `Shipping.Domain` (Core business logic, entities, value objects, domain services)
2. `Shipping.Application` (CQRS handlers, use cases, DTOs, validators)
3. `Shipping.Infrastructure` (EF Core, PostgreSQL, persistence)
4. `Shipping.Api` (REST API controllers, middleware)

## Alternatives
- **3-Tier Architecture**: Tightly couples business logic to database entities and controllers. Rejected due to maintainability risks.
- **Microservices**: Adds unnecessary deployment complexity for the initial scope. Rejected in favor of a Modular Monolith.

## Consequences
- **Positive**: Domain logic is 100% unit-testable without database or API dependencies.
- **Trade-off**: Requires explicit DTO mapping between API and Domain layers.
