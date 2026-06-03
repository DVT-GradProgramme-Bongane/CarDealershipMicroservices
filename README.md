# CarDealershipMicroservices
# Dealer Microservices Platform

A microservices-based car dealership platform orchestrated with Docker Compose. It consists of an API Gateway, nine domain services (a mix of Node.js and .NET), a PostgreSQL database, and RabbitMQ for messaging.

## Architecture

All services run on a shared Docker bridge network (`dealer-network`). The API Gateway is the single public entry point and routes requests to the internal services. Services talk to PostgreSQL for persistence and RabbitMQ for asynchronous messaging.

```
                    ┌─────────────────┐
   Client ───────►  │   API Gateway   │  :3000
                    │  (Node.js)      │
                    └────────┬────────┘
                             │  /api/<service>/*
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
  inventory (.NET)     staff (Node)         client (Node)
  new-car-sales (.NET) used-car-sales(Node) financing (.NET)
  accessories (Node)   maintenance (.NET)   notification (Node)
        │                    │                    │
        └──────────┬─────────┴──────────┬─────────┘
                   ▼                    ▼
            ┌────────────┐       ┌──────────────┐
            │ PostgreSQL │       │   RabbitMQ   │
            │   :5432    │       │ :5672/:15672 │
            └────────────┘       └──────────────┘
```

## Services & Ports

| Service                  | Stack   | Port  |
|--------------------------|---------|-------|
| API Gateway              | Node.js | 3000  |
| Inventory                | .NET    | 5001  |
| Staff                    | Node.js | 5002  |
| Client                   | Node.js | 5003  |
| New Car Sales            | .NET    | 5004  |
| Used Car Sales           | Node.js | 5005  |
| Financing                | .NET    | 5006  |
| Accessories & Suppliers  | Node.js | 5007  |
| Maintenance              | .NET    | 5008  |
| Notification             | Node.js | 5009  |
| PostgreSQL               | —       | 5432  |
| RabbitMQ (AMQP)          | —       | 5672  |
| RabbitMQ (Management UI) | —       | 15672 |

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (Engine 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) v2 (`docker compose`, bundled with recent Docker Desktop)

## Project Structure

The Compose file expects each service in its own folder with a `Dockerfile` inside. Adjust folder names to match your repo if they differ.

```
.
├── docker-compose.yml
├── .env                  # your local config (copied from .env.example)
├── .env.example
├── init.sql              # runs once on first Postgres start
├── ApiGateway/
│   ├── Dockerfile
│   ├── server.js
│   └── package.json
├── InventoryService/
│   └── Dockerfile
├── StaffService/
│   └── Dockerfile
├── ClientService/
│   └── Dockerfile
├── NewCarSalesService/
│   └── Dockerfile
├── UsedCarSalesService/
│   └── Dockerfile
├── FinancingService/
│   └── Dockerfile
├── AccessoriesSuppliersService/
│   └── Dockerfile
├── MaintenanceService/
│   └── Dockerfile
├── NotificationService/
│   └── Dockerfile
└── proto/                # shared gRPC contracts (optional, see below)
```

## Setup

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd <your-repo>
```

### 2. Create your environment file

Copy the example and adjust values as needed:

```bash
cp .env.example .env
```

The defaults work out of the box for local development. The `*_SERVICE_URL` values use Docker's internal DNS (the service name resolves to the container), so leave them as-is unless you rename services.

### 3. Add `init.sql`

PostgreSQL runs any `.sql` file mounted into `/docker-entrypoint-initdb.d/` on **first startup only** (when the data volume is empty). Put your schema and seed data here. An empty file is fine if you don't need it yet:

```bash
touch init.sql
```

### 4. Build and start everything

```bash
docker compose up --build
```

Add `-d` to run in the background:

```bash
docker compose up --build -d
```

The first run builds all images and may take several minutes. Services with `depends_on ... condition: service_healthy` will wait for PostgreSQL and RabbitMQ to pass their health checks before starting.

### 5. Verify

- API Gateway health: <http://localhost:3000/health>
- RabbitMQ Management UI: <http://localhost:15672> (default login `guest` / `guest`)
- A service through the gateway, e.g. <http://localhost:3000/api/inventory/...>

## Using the API Gateway

The gateway is **pure routing — no business logic**. It forwards any request under `/api/<service>/*` to the matching service, stripping the prefix.

| Gateway path                     | Forwards to                          |
|----------------------------------|--------------------------------------|
| `/api/inventory/*`               | `inventory-service:5001/*`           |
| `/api/staff/*`                   | `staff-service:5002/*`               |
| `/api/client/*`                  | `client-service:5003/*`              |
| `/api/new-car-sales/*`           | `new-car-sales-service:5004/*`       |
| `/api/used-car-sales/*`          | `used-car-sales-service:5005/*`      |
| `/api/financing/*`               | `financing-service:5006/*`           |
| `/api/accessories-suppliers/*`   | `accessories-suppliers-service:5007/*`|
| `/api/maintenance/*`             | `maintenance-service:5008/*`         |
| `/api/notification/*`            | `notification-service:5009/*`        |

Example:

```bash
# Hits the gateway, which proxies to inventory-service:5001/cars
curl http://localhost:3000/api/inventory/cars
```

## Environment Variables

Defined in `.env.example`:

| Variable             | Default                  | Description                          |
|----------------------|--------------------------|--------------------------------------|
| `POSTGRES_HOST`      | `postgres`               | DB hostname (Docker service name)    |
| `POSTGRES_PORT`      | `5432`                   | DB port                              |
| `POSTGRES_USER`      | `dealer`                 | DB user                              |
| `POSTGRES_PASSWORD`  | `dealer123`              | DB password                          |
| `POSTGRES_DB`        | `dealer_db`              | Database name                        |
| `RABBITMQ_URL`       | `amqp://rabbitmq:5672`   | RabbitMQ connection string           |
| `*_SERVICE_URL`      | `http://<service>:<port>`| Internal URLs used by the gateway    |

> **Note:** `RABBITMQ_URL` uses the hostname `rabbitmq`, which is set via `hostname:` on the `rabbitmq-service` container. Keep them in sync if you change either.

## Shared gRPC Contracts (optional)

If services communicate via gRPC, all of them must use the **same `.proto` files** to stay compatible:

1. The team leader defines and owns the contracts in a shared `proto/` directory.
2. Each service's Dockerfile copies them in during build, e.g. `COPY proto/ ./proto/`.
3. When a contract changes, the leader distributes the update and every service rebuilds.

Keeping a single source of truth prevents version drift between producers and consumers.

## Common Commands

```bash
# Start (foreground)
docker compose up --build

# Start (background)
docker compose up --build -d

# View logs (all services)
docker compose logs -f

# View logs for one service
docker compose logs -f api-gateway

# Stop containers (keeps volumes/data)
docker compose down

# Stop and wipe the database volume (fresh init.sql run next time)
docker compose down -v

# Rebuild a single service
docker compose build inventory-service
docker compose up -d inventory-service

# List running containers
docker compose ps
```

## Notes on the .NET Services

The .NET services (Inventory, New Car Sales, Financing, Maintenance) set `ASPNETCORE_URLS=http://+:<port>` so Kestrel binds to all interfaces inside the container on the correct port. Make sure each service's `Dockerfile` exposes and listens on the matching port.

## Troubleshooting

- **A service fails to connect to Postgres on startup.** The dependent services wait for Postgres's health check, but your app should also retry connections — containers can be marked healthy before your migrations finish.
- **Database changes in `init.sql` not applied.** `init.sql` only runs when the data volume is empty. Run `docker compose down -v` to reset, then `up` again.
- **Port already in use.** Another process is bound to one of the host ports. Stop it, or change the left-hand side of the `ports` mapping in `docker-compose.yml`.
- **Cannot reach a network domain during build.** If your environment restricts outbound network access, update your network/proxy settings to allow the relevant package registries (npm, NuGet, etc.).
- **Gateway returns 502.** The target service isn't up yet or crashed. Check `docker compose logs -f <service-name>`.