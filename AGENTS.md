# AI Agent Guidelines & Engineering Rails

This repository enforces strict engineering boundaries for AI agents operating on this codebase.

## 1. Core Principles
- **No Unapproved Architecture Changes**: Domain layer MUST NOT depend on Infrastructure or API layers.
- **Specification Driven**: Every business rule change MUST be reflected in a Gherkin specification inside `specs/features/`.
- **Database Safety**: Never modify database schemas without creating an EF Core Migration.
- **Test Integrity**: Never bypass, comment out, or delete failing tests. Fix the underlying root cause.
- **No Hardcoded Secrets**: Secrets and tokens must be loaded from environment variables.

## 2. Gherkin AI Workflow (`gherkin-ai` v2.0.0-beta.1)
AI agents must follow the closed-loop specification workflow:
```text
Gherkin (.feature) → ghk validate → Code Implementation → Automated Tests → Human Review
```

## 3. Layer Responsibilities
- `Shipping.Domain`: Core Entities, Aggregates, Value Objects (`Money`, `Weight`, `Dimensions`), and `ShippingCostCalculator`. No external dependencies.
- `Shipping.Application`: CQRS Commands/Queries, MediatR Handlers, DTOs, FluentValidation rules.
- `Shipping.Infrastructure`: EF Core `ShippingDbContext`, PostgreSQL mappings, Repositories.
- `Shipping.Api`: RESTful Controllers, RFC 7807 `ProblemDetails`, Health checks, Serilog logging.
- `apps/web`: Enterprise Stand-alone Angular frontend app.
