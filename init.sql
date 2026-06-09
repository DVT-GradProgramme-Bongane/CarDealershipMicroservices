-- init.sql — initializes schemas and tables for the car dealership system

-- ============================================================
-- SCHEMAS
-- ============================================================
CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS staff;
CREATE SCHEMA IF NOT EXISTS clients;
CREATE SCHEMA IF NOT EXISTS new_sales;
CREATE SCHEMA IF NOT EXISTS used_sales;
CREATE SCHEMA IF NOT EXISTS financing;
CREATE SCHEMA IF NOT EXISTS accessories;
CREATE SCHEMA IF NOT EXISTS maintenance;
CREATE SCHEMA IF NOT EXISTS notifications;

-- Required for gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================================
-- inventory.cars
-- ============================================================
CREATE TABLE IF NOT EXISTS inventory.cars (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vin         VARCHAR(17) UNIQUE NOT NULL,
    make        VARCHAR NOT NULL,
    model       VARCHAR NOT NULL,
    year        INT,
    color       VARCHAR,
    price       DECIMAL(12,2),
    mileage     INT,
    type        VARCHAR CHECK (type IN ('new', 'used')),
    status      VARCHAR CHECK (status IN ('available', 'sold', 'reserved', 'in-service')) DEFAULT 'available',
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- staff.employees
-- ============================================================
CREATE TABLE IF NOT EXISTS staff.employees (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name  VARCHAR NOT NULL,
    last_name   VARCHAR NOT NULL,
    role        VARCHAR CHECK (role IN ('salesperson', 'finance_manager', 'mechanic', 'manager')),
    email       VARCHAR UNIQUE NOT NULL,
    phone       VARCHAR,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- clients.customers
-- ============================================================
CREATE TABLE IF NOT EXISTS clients.customers (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name  VARCHAR NOT NULL,
    last_name   VARCHAR NOT NULL,
    email       VARCHAR UNIQUE NOT NULL,
    phone       VARCHAR,
    id_number   VARCHAR,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- new_sales.transactions
-- ============================================================
CREATE TABLE IF NOT EXISTS new_sales.transactions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    car_id      UUID REFERENCES inventory.cars(id),
    client_id   UUID REFERENCES clients.customers(id),
    staff_id    UUID REFERENCES staff.employees(id),
    sales_price  DECIMAL(12,2),
    status      VARCHAR CHECK (status IN ('pending', 'completed', 'cancelled')) DEFAULT 'pending',
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- used_sales.transactions
-- ============================================================
CREATE TABLE IF NOT EXISTS used_sales.transactions (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    car_id       UUID REFERENCES inventory.cars(id),
    client_id    UUID REFERENCES clients.customers(id),
    staff_id     UUID REFERENCES staff.employees(id),
    sale_price   DECIMAL(12,2),
    trade_in_id  UUID REFERENCES inventory.cars(id),
    status       VARCHAR CHECK (status IN ('pending', 'completed', 'cancelled')) DEFAULT 'pending',
    created_at   TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- financing.applications
-- sale_id can reference either new or used sale, so no FK enforced
-- ============================================================
CREATE TABLE IF NOT EXISTS financing.applications (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_id         UUID,
    client_id       UUID REFERENCES clients.customers(id),
    loan_amount     DECIMAL(12,2),
    term_months     INT,
    monthly_payment DECIMAL(12,2),
    status          VARCHAR CHECK (status IN ('pending', 'approved', 'rejected')) DEFAULT 'pending',
    created_at      TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- accessories.suppliers
-- ============================================================
CREATE TABLE IF NOT EXISTS accessories.suppliers (
    id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name     VARCHAR NOT NULL,
    contact  VARCHAR,
    email    VARCHAR
    );

-- ============================================================
-- accessories.items
-- ============================================================
CREATE TABLE IF NOT EXISTS accessories.items (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    supplier_id UUID REFERENCES accessories.suppliers(id),
    name        VARCHAR NOT NULL,
    price       DECIMAL(12,2),
    stock       INT DEFAULT 0
    );

-- ============================================================
-- accessories.orders
-- ============================================================
CREATE TABLE IF NOT EXISTS accessories.orders (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id     UUID REFERENCES accessories.items(id),
    quantity    INT NOT NULL,
    status      VARCHAR CHECK (status IN ('ordered', 'received')) DEFAULT 'ordered',
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- maintenance.jobs
-- ============================================================
CREATE TABLE IF NOT EXISTS maintenance.jobs (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    car_id      UUID REFERENCES inventory.cars(id),
    client_id   UUID REFERENCES clients.customers(id),
    staff_id    UUID REFERENCES staff.employees(id),
    description VARCHAR,
    status      VARCHAR CHECK (status IN ('scheduled', 'in-progress', 'completed')) DEFAULT 'scheduled',
    scheduled   TIMESTAMP,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- notifications.log
-- ============================================================
CREATE TABLE IF NOT EXISTS notifications.log (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type  VARCHAR NOT NULL,
    payload     JSONB,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
    );

-- ============================================================
-- INDEXES (helpful for common lookups)
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_cars_status        ON inventory.cars(status);
CREATE INDEX IF NOT EXISTS idx_cars_type          ON inventory.cars(type);
CREATE INDEX IF NOT EXISTS idx_new_sales_car      ON new_sales.transactions(car_id);
CREATE INDEX IF NOT EXISTS idx_used_sales_car     ON used_sales.transactions(car_id);
CREATE INDEX IF NOT EXISTS idx_financing_client   ON financing.applications(client_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_car    ON maintenance.jobs(car_id);
CREATE INDEX IF NOT EXISTS idx_notifications_type ON notifications.log(event_type);

-- ============================================================
-- SEED DATA (for local testing)
-- Fixed UUIDs so you can reference them across services.
-- ============================================================

-- ---------- staff.employees ----------
INSERT INTO staff.employees (id, first_name, last_name, role, email, phone) VALUES
                                                                                ('a36e24b6-4d43-4760-848b-90b0cdbec584', 'Thabo',   'Nkosi',    'salesperson',     'thabo.nkosi@dealer.test',    '+27 11 555 0101'),
                                                                                ('b1111111-1111-1111-1111-111111111111', 'Lerato',  'Mokoena',  'salesperson',     'lerato.mokoena@dealer.test', '+27 11 555 0102'),
                                                                                ('b2222222-2222-2222-2222-222222222222', 'Pieter',  'van Wyk',  'finance_manager', 'pieter.vanwyk@dealer.test',  '+27 11 555 0103'),
                                                                                ('b3333333-3333-3333-3333-333333333333', 'Naledi',  'Dlamini',  'mechanic',        'naledi.dlamini@dealer.test', '+27 11 555 0104'),
                                                                                ('b4444444-4444-4444-4444-444444444444', 'Johan',   'Botha',    'manager',         'johan.botha@dealer.test',    '+27 11 555 0105')
    ON CONFLICT (id) DO NOTHING;

-- ---------- clients.customers ----------
INSERT INTO clients.customers (id, first_name, last_name, email, phone, id_number) VALUES
                                                                                       ('92511f1f-7727-4ed8-9cb7-fbc285212d67', 'Sipho',   'Khumalo', 'sipho.khumalo@example.com',  '+27 82 555 0201', '8801015800083'),
                                                                                       ('c1111111-1111-1111-1111-111111111111', 'Anele',   'Mthembu', 'anele.mthembu@example.com',  '+27 82 555 0202', '9203124800087'),
                                                                                       ('c2222222-2222-2222-2222-222222222222', 'Karen',   'Smit',    'karen.smit@example.com',     '+27 82 555 0203', '8506220500081')
    ON CONFLICT (id) DO NOTHING;

-- ---------- inventory.cars ----------
INSERT INTO inventory.cars (id, vin, make, model, year, color, price, mileage, type, status) VALUES
                                                                                                 ('9a000f24-d78d-4d76-a19b-aee9000db82a', 'JTDBR32E230012345', 'Toyota',     'Corolla',    2024, 'White',  385000.00, 15,     'new',  'available'),
                                                                                                 ('d1111111-1111-1111-1111-111111111111', '1HGCM82633A123456', 'Volkswagen', 'Polo',       2025, 'Silver', 329000.00, 8,      'new',  'available'),
                                                                                                 ('d2222222-2222-2222-2222-222222222222', 'WAUZZZ8K9AA123457', 'Audi',       'A3',         2021, 'Black',  410000.00, 42000,  'used', 'available'),
                                                                                                 ('d3333333-3333-3333-3333-333333333333', 'WBA3A5C50CF123458', 'BMW',        '320i',       2020, 'Blue',   375000.00, 68000,  'used', 'available'),
                                                                                                 ('d4444444-4444-4444-4444-444444444444', 'MMBJYKB40JH123459', 'Mitsubishi', 'Triton',     2019, 'Grey',   295000.00, 95000,  'used', 'reserved')
    ON CONFLICT (id) DO NOTHING;

-- ---------- new_sales.transactions ----------
INSERT INTO new_sales.transactions (id, car_id, client_id, staff_id, sales_price, status) VALUES
    ('e1111111-1111-1111-1111-111111111111',
     'd1111111-1111-1111-1111-111111111111',
     'c1111111-1111-1111-1111-111111111111',
     'b1111111-1111-1111-1111-111111111111',
     325000.00, 'completed')
    ON CONFLICT (id) DO NOTHING;

-- ---------- used_sales.transactions ----------
INSERT INTO used_sales.transactions (id, car_id, client_id, staff_id, sale_price, trade_in_id, status) VALUES
    ('f1111111-1111-1111-1111-111111111111',
     'd2222222-2222-2222-2222-222222222222',
     'c2222222-2222-2222-2222-222222222222',
     'b1111111-1111-1111-1111-111111111111',
     400000.00, NULL, 'completed')
    ON CONFLICT (id) DO NOTHING;

-- ---------- financing.applications ----------
INSERT INTO financing.applications (id, sale_id, client_id, loan_amount, term_months, monthly_payment, status) VALUES
    ('aa111111-1111-1111-1111-111111111111',
     'e1111111-1111-1111-1111-111111111111',
     'c1111111-1111-1111-1111-111111111111',
     300000.00, 60, 6200.00, 'approved')
    ON CONFLICT (id) DO NOTHING;

-- ---------- accessories.suppliers ----------
INSERT INTO accessories.suppliers (id, name, contact, email) VALUES
                                                                 ('ab111111-1111-1111-1111-111111111111', 'AutoParts SA',   'Riaan Pretorius', 'sales@autopartssa.test'),
                                                                 ('ab222222-2222-2222-2222-222222222222', 'CarTrim Imports', 'Fatima Patel',    'orders@cartrim.test')
    ON CONFLICT (id) DO NOTHING;

-- ---------- accessories.items ----------
INSERT INTO accessories.items (id, supplier_id, name, price, stock) VALUES
                                                                        ('ac111111-1111-1111-1111-111111111111', 'ab111111-1111-1111-1111-111111111111', 'All-weather floor mats', 1250.00, 40),
                                                                        ('ac222222-2222-2222-2222-222222222222', 'ab222222-2222-2222-2222-222222222222', 'Roof rack cross bars',   2899.00, 12)
    ON CONFLICT (id) DO NOTHING;

-- ---------- accessories.orders ----------
INSERT INTO accessories.orders (id, item_id, quantity, status) VALUES
                                                                   ('ad111111-1111-1111-1111-111111111111', 'ac111111-1111-1111-1111-111111111111', 20, 'received'),
                                                                   ('ad222222-2222-2222-2222-222222222222', 'ac222222-2222-2222-2222-222222222222', 10, 'ordered')
    ON CONFLICT (id) DO NOTHING;

-- ---------- maintenance.jobs ----------
INSERT INTO maintenance.jobs (id, car_id, client_id, staff_id, description, status, scheduled) VALUES
    ('ae111111-1111-1111-1111-111111111111',
     'd2222222-2222-2222-2222-222222222222',
     'c2222222-2222-2222-2222-222222222222',
     'b3333333-3333-3333-3333-333333333333',
     'Pre-delivery inspection and service', 'scheduled', now() + interval '2 days')
    ON CONFLICT (id) DO NOTHING;

-- ---------- notifications.log ----------
INSERT INTO notifications.log (id, event_type, payload) VALUES
    ('af111111-1111-1111-1111-111111111111', 'sale.completed',
     '{"sale_id": "e1111111-1111-1111-1111-111111111111", "channel": "email"}'::jsonb)
    ON CONFLICT (id) DO NOTHING;