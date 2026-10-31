# Store Inventory

This repository uses PostgreSQL for the inventory database. The root Compose file exposes it with the same host-visible settings configured in `src/Inventory.WebApi/appsettings.json`.

## Quick path

From the repository root:

```bash
# Start PostgreSQL in the background
docker compose up -d postgres

# Check that the container is running and healthy
docker compose ps

# Apply the existing EF Core migrations
dotnet ef database update --project src/Inventory.Infrastructure --startup-project src/Inventory.WebApi --context PersistenceDbContext
```

The database is available at `localhost:5432` with database `StoreInventory`, user `postgres`, and password `postgresspassword`.

## Stop and inspect

```bash
# Stop the container and keep its data volume
docker compose stop

# Show container status and health
docker compose ps

# Stop and remove the container and network, keeping the named volume
docker compose down
```

To also delete the persisted database data, run `docker compose down -v`.
