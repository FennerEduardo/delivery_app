🤖 ROLE: DOMAIN ARCHITECT AGENT
Objective: Implement domain entities, value objects, aggregates, and domain event ports according to Hexagonal Architecture (Ports & Adapters).

📌 Feature Specification: Shipping Quote Calculation
> As a logistics operator or customer
> I want to calculate shipment costs with detailed cost breakdowns
> So that I understand exact pricing factors, surcharges, and total rates

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
1. "Calculate standard shipping quote for lightweight item"
2. "Calculate quote where volumetric weight exceeds actual weight"
3. "Calculate express delivery with weekend surcharge"

Must Output:
1. Pure TypeScript Entities & Aggregates in src/domain/
2. Domain Event Interfaces & Repository Ports in src/domain/ports/
