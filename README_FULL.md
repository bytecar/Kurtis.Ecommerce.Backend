# Kurtis Ecommerce — Complete Documentation

**Generated:** 2025-11-01T06:01:48.518008Z

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Modules & Projects](#modules--projects)
- [Database Schema & Migrations](#database-schema--migrations)
- [Endpoints (Full Reference)](#endpoints-full-reference)
- [Security & Identity](#security--identity)
- [Testing Strategy](#testing-strategy)
- [CI / GitHub Actions](#ci--github-actions)
- [Docker & Deployment](#docker--deployment)
- [Developer Tips & Troubleshooting](#developer-tips--troubleshooting)
- [Appendix](#appendix)

## Overview
Kurtis Ecommerce is a microservices backend with three ASP.NET Core Web APIs (Catalog, Inventory, Users) backed by a shared SQL Server database. It uses EF Core for transactional writes and Dapper for read-optimized queries. ASP.NET Identity is integrated for secure user management.

## Architecture
(see mermaid diagram below)

```mermaid
flowchart LR
  subgraph APIs
    Catalog[Catalog API]
    Inventory[Inventory API]
    Users[Users API]
  end
  Clients[Clients (Web/Mobile)]
  Clients -->|HTTP(S)| Catalog
  Clients -->|HTTP(S)| Inventory
  Clients -->|HTTP(S)| Users
  Catalog -->|EF Core / Dapper| KurtisDb[(KurtisDb)]
  Inventory -->|EF Core| KurtisDb
  Users -->|Identity| KurtisDb
```

## Modules & Projects
- `Kurtis.Common` — domain models (Product, Brand, Category, Inventory, ApplicationUser)
- `Kurtis.DAL` — EF Core `KurtisDbContext`, repositories (Generic, Product, Inventory), UnitOfWork, migrations
- `Kurtis.DAL.Dapper` — Dapper connection factory and read repositories (CatalogDapperRepository)
- `Kurtis.Api.Catalog` — product endpoints (search, detail, CRUD)
- `Kurtis.Api.Inventory` — inventory endpoints (availability, decrement)
- `Kurtis.Api.Users` — ASP.NET Identity-based auth endpoints (register, login). Seeds admin user.
- `Kurtis.Tests` — unit & API tests (xUnit)
- `Kurtis.IntegrationTests` — integration tests using Testcontainers

## Database Schema & Migrations
- Identity tables (AspNetUsers, AspNetRoles, etc.) are created by IdentityDbContext.
- Domain tables: Brands, Categories, Products, ProductCollections, Inventories, Users (via Identity)
- Key constraints: FK Products->Brands, Products->Categories, Inventories->Products
- Indexes: IX_Products_Brand_Category, IX_Inventories_Product_Size (unique)

### Applying migrations
```bash
# from solution root
dotnet ef migrations add InitialCreate -p Kurtis.DAL -s Kurtis.Api.Users
dotnet ef database update -p Kurtis.DAL -s Kurtis.Api.Users
```

Alternatively run `sql/kurtis_deploy_full.sql` on your SQL Server instance.

## Endpoints (Full Reference)
> Note: Use Swagger UI at `/swagger` for full interactive exploration.

### Users API
- `POST /api/users/register` — register a user  
  Body: `{ "username": "...", "email":"...", "password":"..." }`  
  Response: `200 OK` or `400 BadRequest`

- `POST /api/users/login` — login  
  Body: `{ "email":"...", "password":"..." }`  
  Response: `{ "token": "<jwt>" }`

### Catalog API
- `GET /api/products` — list/search  
  Query: `q`, `page`, `pageSize`  
- `GET /api/products/{id}` — product details  
- `POST /api/products` — create product (admin)  
- `PUT /api/products/{id}` — update product (admin)  
- `DELETE /api/products/{id}` — delete product (admin)

### Inventory API
- `GET /api/inventory/{productId}` — list inventory items by product  
- `POST /api/inventory/decrement` — decrement stock (admin) Body: `{ productId, size, quantity }`

## Security & Identity
- Identity uses `ApplicationUser` (int keys) integrated into `KurtisDbContext`.
- JWT authentication configured in each API; tokens contain role claims.
- Seed admin user created at startup (configurable via `Seed:AdminEmail` and `Seed:AdminPassword` env vars).

## Testing Strategy
- **Unit tests**: xUnit + FluentAssertions test DAL repositories and controller logic with InMemory provider.
- **API tests**: WebApplicationFactory used to exercise endpoints.
- **Integration tests**: DotNet.Testcontainers spins up SQL Server 2022 container to run full end-to-end tests.
- **Coverage**: coverlet collector + reportgenerator produce HTML coverage reports deployed to GitHub Pages.

## CI / GitHub Actions
- Workflow file: `.github/workflows/ci.yml` — restores, builds, runs unit tests, runs integration tests, generates coverage and deploys to GH-Pages.

## Docker & Deployment
- `docker-compose.yml` spins up SQL Server 2022 and three API services.
- Use `.env` for secrets (SA_PASSWORD, JWT_KEY, ADMIN_EMAIL, ADMIN_PASSWORD).
- Steps:
  1. `cp .env.sample .env` and edit secrets.
  2. `docker-compose up --build`

## Developer Tips & Troubleshooting
- If EF tools fail on .NET 10 preview, switch target to `net8.0` for development and use .NET 10 SDK when stable.
- Ensure SA_PASSWORD meets SQL Server complexity rules.
- To debug migrations: `dotnet ef migrations script -p Kurtis.DAL -s Kurtis.Api.Users -o script.sql`

## Appendix
- Stored proc `DecrementStockAtomic` available in `sql/kurtis_deploy_full.sql`.
- Example curl to login:
```bash
curl -X POST http://localhost:5003/api/users/login -H "Content-Type: application/json" -d '{"email":"admin@kurtis.local","password":"Admin123!"}'
```

---
