# ADR-002: Pragma CQRS with MediatR

## Context
Command operations (creating shipments, calculating quotes, confirming orders) involve domain invariants and state changes, whereas Query operations (listing shipments, getting history) require fast read projections.

## Decision
We implement **CQRS** (Command Query Responsibility Segregation) in `Shipping.Application` using **MediatR**. Commands enforce business rules via FluentValidation, while Queries return lightweight DTO projections.

## Alternatives
- **Direct Repository calls from Controllers**: Leads to bloated controllers and duplicate validation logic.
- **Event Sourcing with CQRS**: Overly complex for the current domain scope.

## Consequences
- **Positive**: Clear separation of read/write concerns, streamlined MediatR pipeline behaviors for logging and validation.
- **Trade-off**: Additional class files for command, query, and handler pairs.
