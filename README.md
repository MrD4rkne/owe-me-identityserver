[![Quality Check](https://github.com/MrD4rkne/owe-me-identityserver/actions/workflows/sonarqube.yml/badge.svg)](https://github.com/MrD4rkne/owe-me-identityserver/actions/workflows/sonarqube.yml)

# Owe me identity server

## Technologies

- **.NET 9.0**: Core framework for building the API.
- **PostgreSQL**: Database for data persistence.
- **Docker**: Containerization for development and deployment.
- **GitHub Actions**: CI/CD pipeline for automated builds and tests.

## Development

### Prerequisites
- .NET 9.0 SDK
- Docker (for local development)
- PostgreSQL (via Docker Compose)

### Local Setup
```bash
# Clone and navigate the ecosystem
git clone https://github.com/MrD4rkne/oweme-ecosystem/
cd owe-me-identityserver

# Start
docker-compose -f ../compose.yaml -f compose.override.yaml up --build
```

## Configuration

### Connection string

Set up the `ConnectionStrings:DefaultConnection` with the connection string to your database, f.e. in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=oweme;User Id=sa;Password=Your_password123;"
  }
}
```

### Migrations and database seeding

App has a hosted service that can apply migrations and seed the database on application startup. You can configure this behavior using the following settings:

- `Migrations:Apply` = `true` in `appsettings.json` will apply any pending migrations to the database on application startup.
- `Migrations:Seed` = `true` in `appsettings.json` will seed the database with initial data on application startup.

```json
{
  "Migrations": {
    "Apply": true,
    "Seed": true
  }
}
```

Note that this only creates records in the database. It won't delete or update existing records.
