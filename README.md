# Bulky_MVC

`Bulky_MVC` is an ASP.NET Core MVC e-commerce application for book sales, with separate admin and customer experiences, role-based behavior, Stripe integration, and containerized local development.

## Features

- Admin area for categories, products, companies, orders, and user management
- Customer area for browsing products, cart, checkout, and order flow
- Role-based behavior (`Admin`, `Employee`, `Company`, `Customer`)
- Entity Framework Core + SQL Server
- Optional external login providers (Facebook, Microsoft)
- Dockerized app + database setup
- GitHub Actions CI/CD pipeline with Docker build, health check, and optional ECR push

## Tech Stack

- `.NET 8`
- `ASP.NET Core MVC`
- `Entity Framework Core`
- `SQL Server 2022` (container in Docker setup)
- `Stripe`
- `Docker / Docker Compose`
- `GitHub Actions`
- `Amazon Web Services (AWS)` - ECR for container image hosting

## Repository Structure

- `BulkyWeb` - main web application (MVC, areas, views, services)
- `Bulky.DataAccess` - DbContext, repositories, migrations, DB initializer
- `Bulky.Models` - domain models and view models
- `Bulky.Utility` - shared utility classes and constants
- `BulkyBook.Test` - test project
- `.github/workflows/cicd.yml` - CI/CD workflow
- `docker-compose.yml` - base Docker setup for app + SQL Server

## Prerequisites

- `.NET SDK 8`
- `Docker Desktop` (for containerized run)
- (Optional) `Visual Studio 2022`

## Running Locally (Docker - Recommended)

1. Clone the repository.
2. Create a `.env` file in the repository root.
3. Add at minimum:

```env
ConnectionStrings__DefaultConnection=
```

4. Optional keys (for OAuth / Stripe):

```env
AUTH_FACEBOOK_APP_ID=
AUTH_FACEBOOK_APP_SECRET=
AUTH_MICROSOFT_CLIENT_ID=
AUTH_MICROSOFT_CLIENT_SECRET=
Stripe__PublishableKey=
Stripe__SecretKey=
```

5. Start containers:

```bash
docker compose up --build
```

6. Open the app:

- `http://localhost:32768`
- Health endpoint: `http://localhost:32768/health`

## Running Locally (Without Docker)

1. Open `Bulky.sln`.
2. Configure your connection string using environment variables or user secrets.
3. Apply migrations/database update.
4. Run `BulkyWeb`.

## Authentication Notes

- External providers are registered only when their required credentials are available.
- If Facebook or Microsoft keys are missing, the app still runs (provider is skipped).
- For Microsoft login in local Docker, register redirect URI:

`http://localhost:32768/signin-microsoft`

## CI/CD Notes

Workflow file: `.github/workflows/cicd.yml`

Pipeline behavior:

- Builds and starts Docker services
- Performs health check with retries
- Always prints logs on failure
- Stops containers in cleanup step
- Pushes to ECR only on `push` to `master`

Required GitHub secret:

- `CONNECTIONSTRINGS__DEFAULTCONNECTION`

Optional GitHub secrets:

- `AUTH_FACEBOOK_APP_ID`
- `AUTH_FACEBOOK_APP_SECRET`
- `AUTH_MICROSOFT_CLIENT_ID`
- `AUTH_MICROSOFT_CLIENT_SECRET`
- `STRIPE_PUBLISHABLE_KEY`
- `STRIPE_SECRET_KEY`
- AWS/ECR secrets for publish step

## Default Seeded Admin

The database initializer seeds an admin user/roles. If you changed seed settings, use your current seeded credentials.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Open a pull request

