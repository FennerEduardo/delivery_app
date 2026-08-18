# 🥒 Logistics Shipping Platform — Monorepo

[![Gherkin AI Engine](https://img.shields.io/badge/Gherkin--AI-v2.0.0--beta.1-emerald.svg)](https://fennereduardo.com/pages/GherkinIATool/)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-19.0-red.svg)](https://angular.dev/)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](https://www.docker.com/)

> **Enterprise-Grade Logistics & Shipping Quotation System** built with Clean Architecture, CQRS, DDD, Stand-Alone Angular, PostgreSQL, Docker, AWS target cloud architecture, and spec-driven agentic verification powered by **[Gherkin AI](https://fennereduardo.com/pages/GherkinIATool/)**.

Created by **[Fenner Eduardo](https://fennereduardo.com/)** | Official Tooling: [gherkin-ai CLI on npm](https://www.npmjs.com/package/gherkin-ai/v/2.0.0-beta.1)

---

## 🌟 Overview

The **Logistics Shipping Platform** demonstrates production-ready software engineering standards for technical interview challenges (**Tech Lead .NET, Angular & AWS**). It includes:

1. **Gherkin BDD Rules Engine (`gherkin-ai` v2.0.0-beta.1)**: Executable business specifications in `specs/features/` managing volumetric weight, pricing tiers, distance surcharges, commercial value, delivery modes, and status traceability.
2. **Backend .NET 10 Clean Architecture**: Onion architecture with MediatR (CQRS), FluentValidation, EF Core PostgreSQL persistence, optimistic concurrency control (`RowVersion`), RFC 7807 `ProblemDetails`, Serilog, and Health Checks.
3. **Stand-Alone Angular Frontend**: Responsive glassmorphism dashboard, reactive forms, real-time live quotation calculation with **itemized price breakdown explanation**, and status history timeline.
4. **DevOps & Cloud Readiness**: Multi-stage Docker builds, `docker-compose.yml`, GitHub Actions CI pipeline, and AWS Target Cloud Architecture specifications (ALB, ECS Fargate, RDS PostgreSQL, CloudFront, S3).

---

## 🏗️ Monorepo Structure

```text
delivery_app/
├── apps/
│   └── web/                     # Stand-alone Angular 19 enterprise application
├── libs/
│   ├── frontend/                # Reusable Angular models, HTTP services & UI components
│   └── contracts/               # Generated OpenAPI TypeScript contracts
├── backend/
│   ├── Shipping.Domain/         # Core Aggregates, Value Objects, Domain Services & Calculator
│   ├── Shipping.Application/    # CQRS Commands & Queries (MediatR), FluentValidation
│   ├── Shipping.Infrastructure/ # EF Core, PostgreSQL DbContext & Repositories
│   └── Shipping.Api/            # ASP.NET Core Web API, Controllers, Middleware, Serilog
├── tests/
│   └── Shipping.UnitTests/      # xUnit + FluentAssertions unit test suite
├── specs/
│   └── features/                # Gherkin scenario feature specifications (.feature)
├── docs/
│   ├── adr/                     # Architecture Decision Records (ADR-001 to ADR-010)
│   ├── architecture/            # Architectural diagrams & AWS Target Architecture
│   └── engineering/             # DoD, Coding Standards, Git Branching
├── docker/                      # Dockerfile.backend & Dockerfile.frontend
├── docker-compose.yml           # Local multi-container stack orchestration
├── gherkin-ai.config.json       # gherkin-ai CLI configuration
├── pnpm-workspace.yaml          # pnpm monorepo workspace configuration
└── README.md
```

---

## 🚀 Quick Start with Docker

Run the entire application stack (PostgreSQL 16, ASP.NET Core Web API, and Angular Frontend) using Docker Compose with a single command:

```bash
pnpm docker:up
```

Alternatively, invoke `docker compose` directly:

```bash
docker compose up --build
```

### Access Endpoints
- **Angular Frontend Dashboard**: `http://localhost:4200`
- **ASP.NET Core Swagger OpenAPI UI**: `http://localhost:5000/swagger`
- **Health Checks**:
  - Live check: `http://localhost:5000/health/live`
  - Ready check: `http://localhost:5000/health/ready`

To stop containers:
```bash
pnpm docker:down
```

---

## 🥒 Gherkin AI CLI Integration (`ghk` v2.0.0-beta.1)

This project incorporates the **[gherkin-ai](https://fennereduardo.com/pages/GherkinIATool/)** engine to maintain architectural guardrails, generate contract schemas, and run closed-loop agentic verification.

```bash
# Validate project architecture compliance
pnpm ghk:validate

# Generate contract TypeScript interfaces & OpenAPI schemas from Gherkin specs
pnpm ghk:generate

# Rebuild project context bundle under .ghe/
pnpm ghk:context
```

---

## 🧪 Running Tests Locally

### Backend Unit Tests (.NET SDK)
```bash
dotnet test tests/Shipping.UnitTests/Shipping.UnitTests.csproj
```

### Frontend Workspace
```bash
pnpm dev      # Start Vite dev server for Angular frontend
pnpm build    # Production build for Angular frontend
```

---

## 📋 Architecture Decision Records (ADRs)

Key architectural decisions are documented under `docs/adr/`:
- **ADR-001**: Clean Architecture Layer Boundaries
- **ADR-002**: CQRS Implementation with MediatR
- **ADR-003**: PostgreSQL Relational Persistence Engine
- **ADR-004**: Stand-Alone Angular Architecture
- **ADR-005**: Multi-Stage Docker Containerization
- **ADR-006**: AWS Target Cloud Infrastructure
- **ADR-007**: JWT Authentication & Role-Based Authorization
- **ADR-008**: Optimistic Concurrency Control with RowVersion
- **ADR-009**: Shipping Cost Engine Strategy
- **ADR-010**: AI-Assisted Engineering with Gherkin AI

---

## 👨‍💻 Credits & Author

Created by **Fenner Eduardo**:
- **Website & Tooling**: [fennereduardo.com](https://fennereduardo.com/)
- **Gherkin AI CLI**: [npmjs.com/package/gherkin-ai](https://www.npmjs.com/package/gherkin-ai/v/2.0.0-beta.1)
