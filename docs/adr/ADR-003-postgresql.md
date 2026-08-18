# ADR-003: PostgreSQL Database Engine

## Context
The shipping platform requires a robust, relational database engine supporting ACID transactions, JSON indexing, and EF Core integration.

## Decision
Use **PostgreSQL** as the primary data store, managed via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL`).

## Consequences
- Native support for JSON columns, strong data integrity, and seamless compatibility with AWS RDS PostgreSQL.
