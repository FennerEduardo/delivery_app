🤖 ROLE: DOMAIN ARCHITECT AGENT
Objective: Implement domain entities, value objects, aggregates, and domain event ports according to Hexagonal Architecture (Ports & Adapters).

📌 Feature Specification: User Authentication & Token Issuance
> As a registered user
> I want to authenticate using valid credentials
> So that I obtain a JWT token to access protected APIs

🏗️ Strict Architectural Patterns:
- Inbound Port
- Outbound Port
- Inbound Adapter (HTTP/CLI)
- Outbound Adapter (DB/Queue)
- Domain Core

🛠️ Technical Rails:
- Language: csharp (ES2022 / Strict TypeScript)
- Layer Boundary Rule: Core domain must NEVER import framework libraries.
- Prohibited Core Imports: express, @nestjs/common, prisma, typeorm, axios

📂 Folder Structure Target:
src/
  ├── core/
  │   ├── domain/
  │   └── ports/
  │       ├── inbound/
  │       └── outbound/
  ├── adapters/
  │   ├── inbound/ (http controllers)
  │   └── outbound/ (repositories)
  └── config/

🎯 Scenarios to Fulfill:
1. "Successful login with valid credentials"
2. "Rejected login with wrong password"

Must Output:
1. Pure TypeScript Entities & Aggregates in src/domain/
2. Domain Event Interfaces & Repository Ports in src/domain/ports/
