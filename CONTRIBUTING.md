# Contributing Guidelines

## 1. Branch Strategy
- `main`: Production-ready code.
- `develop`: Integration branch for active development.
- `feature/<name>`: New feature implementations.
- `fix/<name>`: Bug fixes.

## 2. Commit Message Conventions
Follow [Conventional Commits](https://www.conventionalcommits.org/):
- `feat: add volumetric weight calculator`
- `fix: resolve optimistic concurrency exception handling`
- `test: add unit tests for distance surcharges`
- `docs: update ADR-002 for CQRS rationale`

## 3. Definition of Done (DoD)
- [ ] Requirements and acceptance criteria defined.
- [ ] Gherkin scenario created/updated in `specs/features/`.
- [ ] Unit tests created with >85% coverage.
- [ ] Code formatted (`pnpm lint` / `dotnet format`).
- [ ] CI pipeline passes (`.github/workflows/ci.yml`).
