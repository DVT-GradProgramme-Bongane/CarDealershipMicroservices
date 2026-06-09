#!/usr/bin/env bash
# generate-manifest.sh
# Usage: generate-manifest.sh <ServiceName> <SolutionRoot>

if [ $# -ne 2 ]; then
  echo 'Error: requires 2 arguments: <ServiceName> <SolutionRoot>' >&2
  exit 1
fi

SERVICE_NAME="$1"
SOLUTION_ROOT="$2"
SERVICE_DIR="$SOLUTION_ROOT/$SERVICE_NAME"
MANIFEST_FILE="$SERVICE_DIR/.scaffold-manifest.json"

echo '[manifest] Checking paths...' >&2

if [ ! -d "$SERVICE_DIR" ]; then
  echo "Error: Service directory not found: $SERVICE_DIR" >&2
  exit 1
fi

if [ ! -f "$SERVICE_DIR/$SERVICE_NAME.csproj" ]; then
  echo "Error: No .csproj found at $SERVICE_DIR/$SERVICE_NAME.csproj" >&2
  exit 1
fi

safe_grep() { grep "$@" 2>/dev/null || true; }
safe_find() { find "$@" 2>/dev/null || true; }

# ---- csproj ----

echo '[manifest] Reading csproj...' >&2

CSPROJ="$SERVICE_DIR/$SERVICE_NAME.csproj"

TARGET_FRAMEWORK=$(safe_grep -o '<TargetFramework>[^<]*</TargetFramework>' "$CSPROJ" | sed 's/<[^>]*>//g' | head -1)
TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net8.0}"
DOTNET_VERSION="${TARGET_FRAMEWORK#net}"

ROOT_NAMESPACE=$(safe_grep -o '<RootNamespace>[^<]*</RootNamespace>' "$CSPROJ" | sed 's/<[^>]*>//g' | head -1)
ROOT_NAMESPACE="${ROOT_NAMESPACE:-$SERVICE_NAME}"

# ---- Program.cs ----

echo '[manifest] Reading Program.cs...' >&2

PROGRAM_CS="$SERVICE_DIR/Program.cs"
DB_CONTEXT="UnknownDbContext"
GRPC_SERVICES=""
HAS_GRPC="false"
SERVICE_INTERFACE=""
SERVICE_IMPL=""
ENDPOINT_GROUPS=""

if [ -f "$PROGRAM_CS" ]; then
  _val=$(safe_grep -o 'AddDbContext<[A-Za-z]*>' "$PROGRAM_CS" | sed 's/AddDbContext<//;s/>//' | head -1)
  if [ -n "$_val" ]; then DB_CONTEXT="$_val"; fi

  _val=$(safe_grep -o 'MapGrpcService<[A-Za-z]*>' "$PROGRAM_CS" | sed 's/MapGrpcService<//;s/>//' | paste -sd ',' -)
  if [ -n "$_val" ]; then GRPC_SERVICES="$_val"; HAS_GRPC="true"; fi

  _line=$(safe_grep -o 'AddScoped<I[A-Za-z]*, [A-Za-z]*>' "$PROGRAM_CS" | head -1)
  if [ -n "$_line" ]; then
    SERVICE_INTERFACE=$(echo "$_line" | sed 's/AddScoped<//;s/>//' | cut -d',' -f1 | tr -d ' ')
    SERVICE_IMPL=$(echo "$_line" | sed 's/AddScoped<//;s/>//' | cut -d',' -f2 | tr -d ' ')
  fi

  _val=$(safe_grep -o 'MapGroup("[^"]*")' "$PROGRAM_CS" | sed 's/MapGroup("//;s/")//' | paste -sd ',' -)
  if [ -n "$_val" ]; then ENDPOINT_GROUPS="$_val"; fi
fi

# ---- Folder scan ----

echo '[manifest] Scanning folders...' >&2

ENDPOINT_FILES=""
while IFS= read -r f; do
  name=$(basename "$f")
  if echo "$name" | grep -qi 'endpoint'; then
    if [ -n "$ENDPOINT_FILES" ]; then ENDPOINT_FILES="$ENDPOINT_FILES,"; fi
    ENDPOINT_FILES="$ENDPOINT_FILES$name"
  fi
done < <(safe_find "$SERVICE_DIR" -name '*.cs' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Tests/*' -not -path '*/Migrations/*')

ENTITIES=""
while IFS= read -r f; do
  dir=$(dirname "$f")
  if echo "$dir" | grep -q '/Models/\|/Entities/'; then
    name=$(basename "$f" .cs)
    if [ -n "$ENTITIES" ]; then ENTITIES="$ENTITIES,"; fi
    ENTITIES="$ENTITIES$name"
  fi
done < <(safe_find "$SERVICE_DIR" -name '*.cs' -not -name '*Context*' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Tests/*' -not -path '*/Migrations/*')

# ---- RabbitMQ ----

echo '[manifest] Checking RabbitMQ...' >&2

HAS_RABBITMQ="false"
while IFS= read -r f; do
  if safe_grep -q 'RabbitMQ\|IPublisher\|IConsumer\|MassTransit' "$f"; then
    HAS_RABBITMQ="true"
    break
  fi
done < <(safe_find "$SERVICE_DIR" -name '*.cs' -not -path '*/bin/*' -not -path '*/obj/*')

# ---- Migrations ----

MIGRATION_COUNT=0
MIGRATION_STRATEGY="EnsureCreatedAsync"
if [ -d "$SERVICE_DIR/Migrations" ]; then
  MIGRATION_COUNT=$(safe_find "$SERVICE_DIR/Migrations" -name '*.cs' -not -name '*Snapshot*' | wc -l | tr -d ' ')
  if [ "$MIGRATION_COUNT" -gt 0 ]; then MIGRATION_STRATEGY="MigrateAsync"; fi
fi

# ---- docker-compose ports ----

echo '[manifest] Reading docker-compose...' >&2

POSTGRES_PORT="5432"
RABBITMQ_PORT="5672"
COMPOSE="$SOLUTION_ROOT/docker-compose.yml"
if [ -f "$COMPOSE" ]; then
  _pg=$(safe_grep -o '[0-9]*:5432' "$COMPOSE" | head -1 | cut -d: -f1)
  if [ -n "$_pg" ]; then POSTGRES_PORT="$_pg"; fi
  _mq=$(safe_grep -o '[0-9]*:5672' "$COMPOSE" | head -1 | cut -d: -f1)
  if [ -n "$_mq" ]; then RABBITMQ_PORT="$_mq"; fi
fi

# ---- Existing tests ----

TESTS_DIR="$SERVICE_DIR/$SERVICE_NAME.Tests"
HAS_EXISTING_TESTS="false"
EXISTING_TEST_COUNT=0
EXISTING_TEST_FILES=""

if [ -d "$TESTS_DIR" ]; then
  while IFS= read -r f; do
    name=$(basename "$f")
    if [ -n "$EXISTING_TEST_FILES" ]; then EXISTING_TEST_FILES="$EXISTING_TEST_FILES,"; fi
    EXISTING_TEST_FILES="$EXISTING_TEST_FILES$name"
    EXISTING_TEST_COUNT=$((EXISTING_TEST_COUNT + 1))
  done < <(safe_find "$TESTS_DIR" -name '*Tests.cs' -not -name 'UnitTest1.cs')
  if [ "$EXISTING_TEST_COUNT" -gt 0 ]; then HAS_EXISTING_TESTS="true"; fi
fi

# ---- Proto namespace ----

PROTO_NAMESPACE=""
if [ -d "$SOLUTION_ROOT/Shared" ]; then
  while IFS= read -r f; do
    _ns=$(safe_grep 'csharp_namespace' "$f" | sed 's/.*"\(.*\)".*/\1/' | head -1)
    if [ -n "$_ns" ]; then PROTO_NAMESPACE="$_ns"; break; fi
  done < <(safe_find "$SOLUTION_ROOT/Shared" -name '*.proto')
fi

# ---- Write manifest using printf (no heredoc to avoid encoding issues) ----

echo '[manifest] Writing manifest...' >&2

printf '{\n' > "$MANIFEST_FILE"
printf '  "serviceName": "%s",\n'       "$SERVICE_NAME"       >> "$MANIFEST_FILE"
printf '  "serviceDir": "%s",\n'        "$SERVICE_DIR"        >> "$MANIFEST_FILE"
printf '  "testsDir": "%s",\n'          "$TESTS_DIR"          >> "$MANIFEST_FILE"
printf '  "targetFramework": "%s",\n'   "$TARGET_FRAMEWORK"   >> "$MANIFEST_FILE"
printf '  "dotnetVersion": "%s",\n'     "$DOTNET_VERSION"     >> "$MANIFEST_FILE"
printf '  "rootNamespace": "%s",\n'     "$ROOT_NAMESPACE"     >> "$MANIFEST_FILE"
printf '  "dbContext": "%s",\n'         "$DB_CONTEXT"         >> "$MANIFEST_FILE"
printf '  "serviceInterface": "%s",\n'  "$SERVICE_INTERFACE"  >> "$MANIFEST_FILE"
printf '  "serviceImpl": "%s",\n'       "$SERVICE_IMPL"       >> "$MANIFEST_FILE"
printf '  "grpcServices": "%s",\n'      "$GRPC_SERVICES"      >> "$MANIFEST_FILE"
printf '  "hasGrpc": %s,\n'              "$HAS_GRPC"           >> "$MANIFEST_FILE"
printf '  "hasRabbitMq": %s,\n'          "$HAS_RABBITMQ"       >> "$MANIFEST_FILE"
printf '  "endpointGroups": "%s",\n'   "$ENDPOINT_GROUPS"    >> "$MANIFEST_FILE"
printf '  "endpointFiles": "%s",\n'    "$ENDPOINT_FILES"     >> "$MANIFEST_FILE"
printf '  "entities": "%s",\n'         "$ENTITIES"           >> "$MANIFEST_FILE"
printf '  "migrationStrategy": "%s",\n' "$MIGRATION_STRATEGY" >> "$MANIFEST_FILE"
printf '  "migrationCount": %s,\n'       "$MIGRATION_COUNT"    >> "$MANIFEST_FILE"
printf '  "infrastructure": { "postgresPort": "%s", "rabbitmqPort": "%s" },\n' "$POSTGRES_PORT" "$RABBITMQ_PORT" >> "$MANIFEST_FILE"
printf '  "existingTests": { "hasTests": %s, "fileCount": %s, "files": "%s" },\n' "$HAS_EXISTING_TESTS" "$EXISTING_TEST_COUNT" "$EXISTING_TEST_FILES" >> "$MANIFEST_FILE"
printf '  "protoNamespace": "%s",\n'   "$PROTO_NAMESPACE"    >> "$MANIFEST_FILE"
printf '  "note": "No credentials. Testcontainers generates ephemeral credentials at runtime."\n' >> "$MANIFEST_FILE"
printf '}\n'                               >> "$MANIFEST_FILE"

echo '[manifest] Done' >&2
exit 0
