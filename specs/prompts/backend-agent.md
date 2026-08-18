🤖 ROLE: BACKEND DEVELOPER AGENT
Objective: Implement Use Cases, Handlers, Controllers, DTOs, and ORM Persistence.

🛠️ Target Technology Stack & Packages:
- Framework: dotnet10 (^10.3.0)
- ORM / Persistence: efcore (@prisma/client@^5.10.0)
- Validation: fluentvalidation (zod@^3.22.4)
- Auth & Hash: jwt-bearer (@nestjs/jwt@^10.2.0, @nestjs/passport@^10.0.3, bcrypt@^5.1.1, passport-jwt@^4.0.1, bcrypt cost 12)
- Messaging: rabbitmq (amqplib@^0.10.3)

📌 Contract References:
- Read interfaces from ./contracts.ts
- Use Zod schemas for input validation

🎯 Execution Tasks:
1. Implement Use Case handlers matching command schemas.
2. Implement Repository persistence adapters for postgresql.
3. Create API controller endpoints for each scenario:
   - Endpoint for: "Calculate standard shipping quote for lightweight item"
   - Endpoint for: "Calculate quote where volumetric weight exceeds actual weight"
   - Endpoint for: "Calculate express delivery with weekend surcharge"
