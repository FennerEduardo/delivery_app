# ADR-004: Stand-Alone Angular Feature Architecture

## Context
The frontend application requires modular organization, maintainability, and clean separation between UI components and domain HTTP integration.

## Decision
Adopt a **Stand-Alone Angular** architecture in `apps/web`, organized by features (`dashboard`, `customers`, `shipments`) paired with shared modules in `libs/frontend`.

---

# ADR-005: Multi-Stage Docker Containerization

## Context
The application needs to run consistently across developer machines, CI pipelines, and production environments.

## Decision
Use multi-stage Docker builds for backend (`Dockerfile.backend`) and frontend (`Dockerfile.frontend`), orchestrated via `docker-compose.yml`.

---

# ADR-006: AWS Target Cloud Architecture

## Context
The application must be designed for eventual migration to a cloud infrastructure.

## Decision
Target AWS services: S3 + CloudFront (Angular), ALB + ECS Fargate (.NET API), and RDS PostgreSQL.

---

# ADR-007: JWT Authentication & Role-Based Access Control

## Context
Endpoints must enforce authentication and authorization.

## Decision
Implement JWT Bearer token authentication supporting `Admin`, `Operator`, and `Customer` roles.

---

# ADR-008: Optimistic Concurrency Control

## Context
Concurrent status updates to shipments must prevent lost updates.

## Decision
Use EF Core `RowVersion` optimistic concurrency tokens on `Shipment` aggregate root.

---

# ADR-009: Shipping Cost Calculation Engine Strategy

## Context
Shipping cost calculation involves multiple rules (weight, volumetric weight, distance, value, delivery window).

## Decision
Encapsulate pricing in `IShippingCostCalculator` using the Strategy Pattern, returning an itemized breakdown (`ShippingQuote`).

---

# ADR-010: AI-Assisted Engineering with Gherkin AI (`gherkin-ai` v2.0.0-beta.1)

## Context
Leverage AI coding agents while enforcing strict quality rails and spec verification.

## Decision
Utilize `gherkin-ai` (`ghk` CLI) to parse BDD specifications in `specs/features/`, build context bundles in `.ghe/`, and run closed-loop verification harnesses.
