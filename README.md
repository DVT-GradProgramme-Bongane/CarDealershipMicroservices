# CarDealershipMicroservices
# Dealer Microservices Platform

A microservices-based car dealership platform orchestrated with Docker Compose. It consists of an API Gateway (Node.js) and a set of .NET domain services, backed by a PostgreSQL database and RabbitMQ for messaging.

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
  inventory (.NET)     staff (.NET)         client (.NET)
  new-car-sales (.NET) used-car-sales(.NET) accessories (.NET)
  notification (.NET)
        │                    │                    │
        └──────────┬─────────┴──────────┬─────────┘
                   ▼                    ▼
            ┌────────────┐       ┌──────────────┐
            │ PostgreSQL │       │   RabbitMQ   │
            │   :5432    │       │ :5672/:15672 │
            └────────────┘       └──────────────┘
```

> **Note:** The **Financing** and **Maintenance** services are planned but **not yet created**. They are intentionally omitted from the tables and routing below until they are implemented.

## Services & Ports

| Service                  | Stack   | Port  | Status        |
|--------------------------|---------|-------|---------------|
| API Gateway              | Node.js | 3000  | Available     |
| Inventory                | .NET    | 5001  | Available     |
| Staff                    | .NET    | 5002  | Available     |
| Client                   | .NET    | 5003  | Available     |
| New Car Sales            | .NET    | 5004  | Available     |
| Used Car Sales           | .NET    | 5005  | Available     |
| Accessories & Suppliers  | .NET    | 5007  | Available     |
| Notification             | .NET    | 5009  | Available     |
| Financing                | .NET    | 5006  | Not yet built |
| Maintenance              | .NET    | 5008  | Not yet built |
| PostgreSQL               | —       | 5432  | Available     |
| RabbitMQ (AMQP)          | —       | 5672  | Available     |
| RabbitMQ (Management UI) | —       | 15672 | Available     |

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (Engine 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) v2 (`docker compose`, bundled with recent Docker Desktop)

## Project Structure

The Compose file expects each service in its own folder with a `Dockerfile` inside. Adjust folder names to match your repo if they differ.

```
.
├── docker-compose.yml
├── .env                  # your local config (copied from .env.example)
├── init.sql              # schema — runs once on first Postgres start
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
├── AccessoriesSuppliersService/
│   └── Dockerfile
├── NotificationService/
│   └── Dockerfile
└── Shared/                # shared gRPC contracts
```

## Setup

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd <your-repo>
```

### 2. Create your environment file

Copy the environment variables from the chat and adjust values as needed:

```bash
.env
```

Fill in the required credentials (database user/password, etc.) before starting. **These values are not committed to the repo** — the working credentials for local development will be provided in the chat. The `*_SERVICE_URL` values use Docker's internal DNS (the service name resolves to the container), so leave them as-is unless you rename services.

### 3. Add `init.sql` and seed files

PostgreSQL runs any `.sql` or `.sh` file mounted into `/docker-entrypoint-initdb.d/` on **first startup only** (when the data volume is empty), in **alphabetical order**. The schema lives in `init.sql`; test data is included in the `init.sql` file.

### 4. Build and start everything

The recommended way to bring the stack up is to target the gateway with `--build`. Using `--build` ensures images are rebuilt whenever there are changes in any of the Dockerfiles:

```bash
docker compose up api-gateway --build
```

Add `-d` to run in the background:

```bash
docker compose up api-gateway --build -d
```

The first run builds all images and may take several minutes. Services with `depends_on ... condition: service_healthy` will wait for PostgreSQL and RabbitMQ to pass their health checks before starting.

### 5. Verify

- API Gateway health: <http://localhost:3000/health>
- RabbitMQ Management UI: <http://localhost:15672> (login credentials provided in the chat)
- A service through the gateway, e.g. <http://localhost:3000/api/accessories-suppliers/suppliers>
- To access the front end <http://localhost:3000>

---

## Database Schema & Seed Data

The database is initialized from two files mounted into the Postgres container's init directory. They only run on a **fresh data volume** (first start, or after `docker compose down -v`).

### Files

| File         | Mounted as                              | Runs                          | Purpose                          |
|--------------|-----------------------------------------|-------------------------------|----------------------------------|
| `init.sql`   | `/docker-entrypoint-initdb.d/01-init.sql` | Always (on fresh volume)    | Schemas, tables, indexes         |

### Re-running the seed

Because init scripts only fire on an empty volume, to test properly you need to run the following commands:

```bash
docker compose down -v
docker compose up api-gateway --build
```
for the api, and

```bash
docker compose up frontend --build
```
to test the front end with the api


### Seeded test data reference

The seed uses **fixed UUIDs** so you can reference the same records across services. The IDs below are stable across rebuilds.

**Staff** (`staff.employees`)

| ID                                     | Name           | Role            |
|----------------------------------------|----------------|-----------------|
| `a36e24b6-4d43-4760-848b-90b0cdbec584` | Thabo Nkosi    | salesperson     |
| `b1111111-1111-1111-1111-111111111111` | Lerato Mokoena | salesperson     |
| `b2222222-2222-2222-2222-222222222222` | Pieter van Wyk | finance_manager |
| `b3333333-3333-3333-3333-333333333333` | Naledi Dlamini | mechanic        |
| `b4444444-4444-4444-4444-444444444444` | Johan Botha    | manager         |

**Clients** (`clients.customers`)

| ID                                     | Name          |
|----------------------------------------|---------------|
| `92511f1f-7727-4ed8-9cb7-fbc285212d67` | Sipho Khumalo |
| `c1111111-1111-1111-1111-111111111111` | Anele Mthembu |
| `c2222222-2222-2222-2222-222222222222` | Karen Smit    |

**Cars** (`inventory.cars`)

| ID                                     | Make / Model      | Type | Status    |
|----------------------------------------|-------------------|------|-----------|
| `9a000f24-d78d-4d76-a19b-aee9000db82a` | Toyota Corolla    | new  | available |
| `d1111111-1111-1111-1111-111111111111` | Volkswagen Polo   | new  | available |
| `d2222222-2222-2222-2222-222222222222` | Audi A3           | used | available |
| `d3333333-3333-3333-3333-333333333333` | BMW 320i          | used | available |
| `d4444444-4444-4444-4444-444444444444` | Mitsubishi Triton | used | reserved  |

**Pre-existing sales / related records**

| ID                                     | Record                              |
|----------------------------------------|-------------------------------------|
| `e1111111-1111-1111-1111-111111111111` | New sale (Polo → Anele, completed)  |
| `f1111111-1111-1111-1111-111111111111` | Used sale (A3 → Karen, completed)   |
| `aa111111-1111-1111-1111-111111111111` | Financing application (approved)    |
| `ab111111…` / `ab222222…`              | Accessory suppliers                 |
| `ac111111…` / `ac222222…`              | Accessory items                     |
| `ae111111-1111-1111-1111-111111111111` | Maintenance job (scheduled)         |

> The Toyota Corolla, client Sipho Khumalo, and salesperson Thabo Nkosi are left **available/unused** so you can create a brand-new sale referencing them without colliding with the pre-seeded transactions.

---

## Using the API Gateway

The gateway is **pure routing — no business logic**. It forwards any request under `/api/<service>/*` to the matching service, stripping the prefix.

| Gateway path                     | Forwards to                            |
|----------------------------------|----------------------------------------|
| `/api/inventory/*`               | `inventory-service:5001/*`             |
| `/api/staff/*`                   | `staff-service:5002/*`                 |
| `/api/client/*`                  | `client-service:5003/*`                |
| `/api/new-car-sales/*`           | `new-car-sales-service:5004/*`         |
| `/api/used-car-sales/*`          | `used-car-sales-service:5005/*`        |
| `/api/accessories-suppliers/*`   | `accessories-suppliers-service:5007/*` |
| `/api/notification/*`            | `notification-service:5009/*`          |
| `/api/maintenance/*`             | `maintenance-service:5008/*`           |

> Routes for `/api/financing/*` (5006) will be added once those services are created.

Example:

```bash
# Hits the gateway, which proxies to http://maintenance-service:5008/maintenance
curl http://localhost:3000/api/maintenance/maintenance
```

---

## Querying the Services

All examples go through the **gateway on port 3000**. The gateway strips the `/api/<service>` prefix and forwards the rest to the service. Routes follow each controller's `[Route]` and the standard REST verbs (`GET` list, `GET /{id}`, `POST`, `PATCH`).

> **Note:** route paths below reflect the controllers as currently built. If a service's controller uses a different `[Route]`, adjust the path after the service prefix accordingly. For pretty-printed JSON, pipe any command through `| jq`.

### New Car Sales (`/api/new-car-sales`)

```bash
# List all new-car sales
curl http://localhost:3000/api/new-car-sales/new-sales

# Get one sale by ID (use the pre-seeded sale)
curl http://localhost:3000/api/new-car-sales/new-sales/e1111111-1111-1111-1111-111111111111

# Create a new sale (uses the unused seeded car/client/staff)
curl http://localhost:3000/api/new-car-sales/new-sales \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "carId":"9a000f24-d78d-4d76-a19b-aee9000db82a",
    "clientId":"92511f1f-7727-4ed8-9cb7-fbc285212d67",
    "staffId":"a36e24b6-4d43-4760-848b-90b0cdbec584",
    "salesPrice":385000
  }'

# Update a sale's status (triggers the sale.new.completed event when set to "completed")
curl http://localhost:3000/api/new-car-sales/new-sales/<sale-id>/status \
  --request PATCH \
  --header "Content-Type: application/json" \
  --data '"completed"'
```

Creating a sale publishes a `sale.new.created` event to RabbitMQ and calls the Inventory service over gRPC to mark the car `reserved`. Completing a sale publishes `sale.new.completed` and marks the car `sold`.

### Used Car Sales (`/api/used-car-sales`)

```bash
# List all used-car sales
curl http://localhost:3000/api/used-car-sales/used-sales

# Get one used sale by ID (pre-seeded)
curl http://localhost:3000/api/used-car-sales/used-sales/f1111111-1111-1111-1111-111111111111

# Create a used-car sale (BMW 320i → client Anele, salesperson Lerato)
curl http://localhost:3000/api/used-car-sales/used-sales \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "carId":"d3333333-3333-3333-3333-333333333333",
    "clientId":"c1111111-1111-1111-1111-111111111111",
    "staffId":"b1111111-1111-1111-1111-111111111111",
    "salesPrice":375000
  }'
```

### Inventory (`/api/inventory`)

```bash
# List all cars
curl http://localhost:3000/api/inventory/inventory

# Get one car by ID
curl http://localhost:3000/api/inventory/inventory/9a000f24-d78d-4d76-a19b-aee9000db82a

# Add a new car
curl http://localhost:3000/api/inventory/inventory \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "vin":"NEWVIN00000000001",
    "make":"Ford",
    "model":"Ranger",
    "year":2025,
    "color":"Red",
    "price":549000,
    "mileage":5,
    "type":"new",
    "status":"available"
  }'
```

### Staff (`/api/staff`)

```bash
# List all employees
curl http://localhost:3000/api/staff/

# Get one employee by ID
curl http://localhost:3000/api/staff/b4444444-4444-4444-4444-444444444444

# Add an employee
curl http://localhost:3000/api/staff \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "firstName":"Zanele",
    "lastName":"Maseko",
    "role":"salesperson",
    "email":"zanele.maseko@dealer.test",
    "phone":"+27 11 555 0199"
  }'
```

### Client (`/api/client`)

```bash
# List all customers
curl http://localhost:3000/api/client/clients

# Get one customer by ID
curl http://localhost:3000/api/client/clients/92511f1f-7727-4ed8-9cb7-fbc285212d67

# Add a customer
curl http://localhost:3000/api/client/clients \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "firstName":"Mandla",
    "lastName":"Zulu",
    "email":"mandla.zulu@example.com",
    "phone":"+27 82 555 0299",
    "idNumber":"9001015800085"
  }'
```

### Accessories & Suppliers (`/api/accessories-suppliers`)

```bash
# List suppliers
curl http://localhost:3000/api/accessories-suppliers/suppliers

# List accessory items
curl http://localhost:3000/api/accessories-suppliers/accessories

# List orders
curl http://localhost:3000/api/accessories-suppliers/accessories/orders

# Place an order for a seeded item
curl http://localhost:3000/api/accessories-suppliers/accessories/orders \
  --request POST \
  --header "Content-Type: application/json" \
  --data '{
    "itemId":"ac111111-1111-1111-1111-111111111111",
    "quantity":5,
    "status":"ordered"
  }'
```

### Notification (`/api/notification`)

The Notification service consumes events from RabbitMQ (it binds a queue to the `dealership` topic exchange) and writes them to `notifications.log`. Trigger it indirectly by creating or completing a sale, then read the log:

```bash
# List logged notifications
curl http://localhost:3000/api/notification/notifications
```

You can also confirm message flow in the RabbitMQ Management UI (<http://localhost:15672>): the `dealership` exchange and the notification service's bound queue should both appear, with the message count incrementing as you create sales.

---

## Messaging (RabbitMQ)

- **Exchange:** `dealership` (topic, durable). Declared by publishers.
- **Routing keys:** 
  - `sale.new.created`
  - `sale.new.completed`
  - `sale.used.created`
  - `sale.used.completed`
  - `financing.approved`
  - `financing.rejected`
  - `maintenance.completed`
  - `accessory.order.placed`
  - `accessory.stock.low`
- **Queues/bindings:** declared by **consumers** (e.g. the Notification service binds a queue with a `sale.*` pattern, `QueueName = "notification-queue"`). A publisher alone does **not** create a queue — if no queue is bound to the exchange, published messages are dropped silently. If you expect a message but see no queue, check that a consumer has declared and bound one.

---

## Environment Variables

Defined in `.env`. Sensitive values (DB user, DB password, RabbitMQ credentials) are **not** included in the repo and will be provided in the chat.

| Variable            | Description                                  |
|---------------------|----------------------------------------------|
| `POSTGRES_HOST`     | DB hostname (Docker service name)            |
| `POSTGRES_PORT`     | DB port                                      |
| `POSTGRES_USER`     | DB user (provided in chat)                   |
| `POSTGRES_PASSWORD` | DB password (provided in chat)               |
| `POSTGRES_DB`       | Database name                                |
| `RABBITMQ_URL`      | RabbitMQ connection string                   |
| `*_SERVICE_URL`     | Internal URLs used by the gateway            |

---

## Shared gRPC Contracts

If services communicate via gRPC, all of them must use the **same `.proto` files** to stay compatible:

1. The team leader defines and owns the contracts in a shared `proto/` directory.
2. Each service's Dockerfile copies them in during build, e.g. `COPY ./ ./
3. When a contract changes, the leader distributes the update and every service rebuilds.

Keeping a single source of truth prevents version drift between producers and consumers.

---

## Common Commands

```bash
# Start (frontend)
docker compose up frontend --build
```
```bash
# Start (foreground, rebuilds images on Dockerfile changes)
docker compose up api-gateway --build
```
```bash
# Start (background)
docker compose up api-gateway --build -d
```
```bash
# View logs (all services)
docker compose logs -f
```
```bash
# View logs for one service
docker compose logs -f api-gateway
```
```bash
# Stop containers (keeps volumes/data)
docker compose down
```
```bash
# Stop and wipe the database volume (fresh init.sql + seed run next time)
docker compose down -v
```
```bash
# Rebuild a single service
docker compose build inventory-service
docker compose up -d inventory-service
```
```bash
# List running containers
docker compose ps
```

---

## Notes on the .NET Services

All domain services are built on .NET. Each sets `ASPNETCORE_URLS=http://+:<port>` so Kestrel binds to all interfaces inside the container on the correct port. Make sure each service's `Dockerfile` exposes and listens on the matching port.

---

## Troubleshooting

- **A service fails to connect to Postgres on startup.** The dependent services wait for Postgres's health check, but your app should also retry connections — containers can be marked healthy before your migrations finish.
- **Database changes in `init.sql` not applied.** These only run when the data volume is empty. Run `docker compose down -v` to reset, then `up` again.
- **Foreign key violation on creating a sale.** The referenced `carId` / `clientId` / `staffId` must already exist. Use the seeded IDs above, or create the parent records first.
- **No queue appears in RabbitMQ.** Publishing to an exchange does not create a queue. A consumer must declare and bind one. Verify the Notification service is running and connected.
- **Port already in use.** Another process is bound to one of the host ports. Stop it, or change the left-hand side of the `ports` mapping in `docker-compose.yml`.
- **Cannot reach a network domain during build.** If your environment restricts outbound network access, update your network/proxy settings to allow the relevant package registries (npm, NuGet, etc.).
- **Gateway returns 502.** The target service isn't up yet or crashed. Check `docker compose logs -f <service-name>`.
- **Changes to a Dockerfile aren't picked up.** Make sure you include `--build` (e.g. `docker compose up api-gateway --build`) so images are rebuilt.