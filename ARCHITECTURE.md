# Architecture Overview — Logistics Shipping Platform

## 1. Architectural Style
The system is built as a pragmatic Modular Monolith following **Clean Architecture**, **Domain-Driven Design (DDD)**, and **Command Query Responsibility Segregation (CQRS)** patterns.

```text
[ Angular Frontend ]
         │ (HTTP REST / JSON)
         ▼
[ Shipping.Api (ASP.NET Core Web API) ]
         │
         ▼
[ Shipping.Application (CQRS / MediatR) ]
         │
         ▼
[ Shipping.Domain (Entities, Value Objects, Pricing Engine) ]
         ▲
         │ (Implements Interfaces)
[ Shipping.Infrastructure (EF Core, PostgreSQL) ]
```

## 2. Monorepo Organization
- `apps/web`: Stand-alone Angular Web Application.
- `libs/frontend`: Reusable models, HTTP services, and UI components.
- `backend/`: .NET 10 Clean Architecture solution (`Domain`, `Application`, `Infrastructure`, `Api`).
- `specs/features/`: Executable Gherkin business feature specifications.
- `docs/adr/`: Architecture Decision Records (ADR 001 - 010).

## 3. AWS Cloud Target Architecture
For production cloud deployment, the architecture targets AWS services:
- **CloudFront + S3**: Angular static asset hosting.
- **ALB (Application Load Balancer)**: API Routing and TLS termination.
- **ECS Fargate**: Containerized execution of .NET Web API.
- **Amazon RDS for PostgreSQL**: Multi-AZ Managed relational persistence.
- **AWS Secrets Manager & CloudWatch**: Centralized secret storage and structured observability.
