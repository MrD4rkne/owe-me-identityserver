[![Quality Check](https://github.com/MrD4rkne/owe-me-identityserver/actions/workflows/sonarqube.yml/badge.svg)](https://github.com/MrD4rkne/owe-me-identityserver/actions/workflows/sonarqube.yml)

# Owe me identity server

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
