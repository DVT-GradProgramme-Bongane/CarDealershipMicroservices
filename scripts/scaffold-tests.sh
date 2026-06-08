#!/usr/bin/env bash
# scaffold-tests.sh
# Usage:
#   ./scripts/scaffold-tests.sh <ServiceName>

set -euo pipefail

BOLD='\033[1m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
RED='\033[0;31m'; CYAN='\033[0;36m'; RESET='\033[0m'

info()    { echo -e "${CYAN}▶${RESET} $*"; }
success() { echo -e "${GREEN}✓${RESET} $*"; }
warn()    { echo -e "${YELLOW}⚠${RESET} $*"; }
error()   { echo -e "${RED}✗${RESET} $*"; }
section() { echo -e "\n${BOLD}── $* ${RESET}$(printf '─%.0s' {1..40})\n"; }

SPINNER_PID=""
start_spinner() {
  local msg="$1"; local frames=("⠋" "⠙" "⠹" "⠸" "⠼" "⠴" "⠦" "⠧" "⠇" "⠏")
  ( i=0; while true; do
      printf "\r${CYAN}${frames[$((i % 10))]}${RESET} %s" "$msg"
      sleep 0.1; i=$((i + 1))
    done ) &
  SPINNER_PID=$!; disown "$SPINNER_PID"
}
stop_spinner() {
  if [ -n "$SPINNER_PID" ] && kill -0 "$SPINNER_PID" 2>/dev/null; then
    kill "$SPINNER_PID" 2>/dev/null
    wait "$SPINNER_PID" 2>/dev/null || true
    printf "\r\033[K"
  fi
  SPINNER_PID=""
}
trap 'stop_spinner' EXIT

# ── Validate input ────────────────────────────────────────────────────────────

if [ $# -ne 1 ]; then
  error "Usage: $0 <ServiceName>  e.g.  $0 StaffService"
  exit 1
fi

SERVICE_NAME="$1"
SERVICE_NAME_LOWER=$(echo "$SERVICE_NAME" \
  | sed 's/\([A-Z]\)/-\1/g' | sed 's/^-//' | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SERVICE_DIR="$SOLUTION_ROOT/$SERVICE_NAME"
TESTS_DIR="$SERVICE_DIR/$SERVICE_NAME.Tests"
WORKFLOW_FILE="$SOLUTION_ROOT/.github/workflows/$SERVICE_NAME_LOWER-ci.yml"
MANIFEST_FILE="$SERVICE_DIR/.scaffold-manifest.json"

section "CarDealership Test Scaffold — $SERVICE_NAME"
info "Service:  $SERVICE_DIR"
info "Tests:    $TESTS_DIR"

# ── Pre-flight checks ─────────────────────────────────────────────────────────

section "Pre-flight checks"

[ ! -d "$SERVICE_DIR" ] && { error "Service not found: $SERVICE_DIR"; exit 1; }
[ ! -f "$SERVICE_DIR/$SERVICE_NAME.csproj" ] && { error "No .csproj in $SERVICE_NAME"; exit 1; }
[ ! -f "$SOLUTION_ROOT/skills/SKILL.md" ]   && { error "Missing skills/SKILL.md"; exit 1; }
[ ! -f "$SOLUTION_ROOT/skills/PROMPT.md" ]  && { error "Missing skills/PROMPT.md"; exit 1; }
command -v claude &>/dev/null              || { error "claude CLI not found — install Claude Code"; exit 1; }
success "All pre-flight checks passed"

# ── Generate manifest (replaces raw file reads by the agent) ─────────────────

section "Generating service manifest"
info "Extracting service facts — agent reads this instead of raw source files"
info "This cuts agent token usage by ~80%"

# Run manifest generator — stop spinner first to avoid subshell conflict with set -e
info "Running manifest generator..."
if ! "$SCRIPT_DIR/generate-manifest.sh" "$SERVICE_NAME" "$SOLUTION_ROOT"; then
  error "Manifest generation failed — check generate-manifest.sh is executable"
  error "Run manually to see errors: bash -x $SCRIPT_DIR/generate-manifest.sh $SERVICE_NAME $SOLUTION_ROOT"
  exit 1
fi

# Manifest path is fixed — generator always writes here
MANIFEST_FILE="$SERVICE_DIR/.scaffold-manifest.json"

if [ ! -f "$MANIFEST_FILE" ]; then
  error "Manifest file not created at expected path: $MANIFEST_FILE"
  exit 1
fi

success "Manifest written to $MANIFEST_FILE"

# Show what was extracted so developer can verify before agent runs
echo ""
python3 -m json.tool "$MANIFEST_FILE" 2>/dev/null || cat "$MANIFEST_FILE"
echo ""

# ── Detect run mode ───────────────────────────────────────────────────────────

section "Run mode detection"

# Clean up dotnet scaffold default if agent was previously interrupted
if [ -f "$TESTS_DIR/UnitTest1.cs" ]; then
  warn "Removing empty dotnet default test file from previous run"
  rm "$TESTS_DIR/UnitTest1.cs"
fi

# Clean up corrupted coverage mapping files that cause false build failures
find "${TESTS_DIR:-/nonexistent}" -name ".msCoverageSourceRootsMapping*" \
  -delete 2>/dev/null && true

HAS_TESTS=$(python3 -c "
import json, sys
m = json.load(open('$MANIFEST_FILE'))
print(m['existingTests']['hasTests'])
" 2>/dev/null || echo "False")

if [ "$HAS_TESTS" = "True" ]; then
  RUN_MODE="append"
  EXISTING_COUNT=$(python3 -c "
import json
m = json.load(open('$MANIFEST_FILE'))
print(m['existingTests']['fileCount'])
")
  info "Mode: APPEND — $EXISTING_COUNT existing test file(s) found"
else
  RUN_MODE="scaffold"
  info "Mode: SCAFFOLD (first-time)"
fi

# ── Build the prompt from manifest ───────────────────────────────────────────

section "Building agent prompt"

# Read manifest as compact JSON for injection into prompt
MANIFEST_JSON=$(cat "$MANIFEST_FILE")

PROMPT=$(sed \
  -e "s|%%SERVICE_NAME%%|$SERVICE_NAME|g" \
  -e "s|%%SERVICE_NAME_LOWER%%|$SERVICE_NAME_LOWER|g" \
  -e "s|%%TESTS_DIR%%|$TESTS_DIR|g" \
  -e "s|%%SERVICE_DIR%%|$SERVICE_DIR|g" \
  -e "s|%%SOLUTION_ROOT%%|$SOLUTION_ROOT|g" \
  "$SOLUTION_ROOT/skills/PROMPT.md")

# Append the manifest to the prompt — agent reads this, not raw source files
PROMPT="$PROMPT

## Pre-extracted service manifest

The following facts have already been extracted from the service source files.
Use this manifest directly. Do NOT re-read the raw source files for these facts —
doing so wastes tokens. Only read source files if you need something not in this manifest.

\`\`\`json
$MANIFEST_JSON
\`\`\`

The manifest contains no credentials. All credential handling is done via
Testcontainers at runtime as described in SKILL.md."

success "Prompt built"

# ── Run the agent ─────────────────────────────────────────────────────────────

section "Running agent"

echo -e "${YELLOW}Agent is working — progress is streamed below.${RESET}"
echo -e "${YELLOW}Typical duration: 3-5 minutes (down from 10 with manifest).${RESET}"
echo ""

cd "$SOLUTION_ROOT"

echo "$PROMPT" | claude \
  --verbose \
  --max-turns 60 \
  --allowedTools "Read,Write,Edit,Bash" \
  -p -

AGENT_EXIT=$?
[ $AGENT_EXIT -ne 0 ] && warn "Agent exited with code $AGENT_EXIT — checking output..."

# ── Post-run verification ─────────────────────────────────────────────────────

section "Verifying output"

PASS=true

# Test project
if [ ! -d "$TESTS_DIR" ]; then
  error "Test project not found at: $TESTS_DIR"
  PASS=false
else
  success "Test project exists"
fi

# Real test files
if [ -d "$TESTS_DIR" ]; then
  TEST_COUNT=$(find "$TESTS_DIR" -name "*Tests.cs" ! -name "UnitTest1.cs" \
    2>/dev/null | wc -l | tr -d ' ')
  if [ "$TEST_COUNT" -eq 0 ]; then
    error "No test files generated — agent likely ran out of turns"
    PASS=false
  else
    success "Found $TEST_COUNT test file(s):"
    find "$TESTS_DIR" -name "*Tests.cs" ! -name "UnitTest1.cs" \
      | while read -r f; do info "  $(basename "$f")"; done
  fi
fi

# CI workflow
if [ "$RUN_MODE" = "scaffold" ] && [ ! -f "$WORKFLOW_FILE" ]; then
  warn "CI workflow not created — can re-run or create manually"
fi

# Solution registration
SLN_FILE=$(find "$SOLUTION_ROOT" -maxdepth 1 -name "*.sln" | head -1)
if [ -n "$SLN_FILE" ] && [ -d "$TESTS_DIR" ]; then
  if ! dotnet sln "$SLN_FILE" list 2>/dev/null | grep -q "$SERVICE_NAME.Tests"; then
    info "Adding test project to solution..."
    dotnet sln "$SLN_FILE" add "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" \
      && success "Added to solution" \
      || warn "Add to solution failed — do manually: dotnet sln add $TESTS_DIR/$SERVICE_NAME.Tests.csproj"
  else
    success "Test project in solution"
  fi
fi

# Clean coverage artifacts before build (prevents macOS false failure)
find "${TESTS_DIR:-/nonexistent}" -name ".msCoverageSourceRootsMapping*" \
  -delete 2>/dev/null && true
find "${TESTS_DIR:-/nonexistent}" -path "*/bin/*" -name "*.msCoverage*" \
  -delete 2>/dev/null && true

# Build
if [ -d "$TESTS_DIR" ]; then
  echo ""
  info "Building..."
  start_spinner "dotnet build..."
  BUILD_OUTPUT=$(dotnet build "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" \
    --nologo 2>&1)
  BUILD_EXIT=$?
  stop_spinner

  if [ $BUILD_EXIT -eq 0 ]; then
    success "Build passed"
  else
    error "Build failed:"
    echo "$BUILD_OUTPUT" | grep -E "error|Error" | head -20
    PASS=false
  fi
fi

# Unit tests
if [ "$PASS" = "true" ] && [ -d "$TESTS_DIR" ]; then
  echo ""
  info "Running unit tests..."
  start_spinner "dotnet test (unit)..."
  TEST_OUTPUT=$(dotnet test "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" \
    --filter "FullyQualifiedName~Unit" \
    --nologo 2>&1)
  TEST_EXIT=$?
  stop_spinner

  echo "$TEST_OUTPUT" | tail -5
  if [ $TEST_EXIT -eq 0 ]; then
    success "Unit tests passed"
  else
    warn "Unit tests failed — this may expose real issues in the service"
    warn "Review the failures above before assuming the tests are wrong"
  fi
fi

# Cleanup manifest (contains no credentials but no need to commit it)
[ -f "$MANIFEST_FILE" ] && rm "$MANIFEST_FILE"

# ── Summary ───────────────────────────────────────────────────────────────────

section "Summary"

if [ "$PASS" = "true" ]; then
  success "Scaffold complete — $SERVICE_NAME"
  echo ""
  echo "  Unit tests:        dotnet test $SERVICE_NAME/$SERVICE_NAME.Tests --filter \"FullyQualifiedName~Unit\""
  echo "  Integration tests: dotnet test $SERVICE_NAME/$SERVICE_NAME.Tests --filter \"FullyQualifiedName~Integration\""
  echo ""
  info "Integration tests need Docker running (Testcontainers handles infra)"
  info "Review generated tests — look for // SCAFFOLD markers needing attention"
  info "Commit when satisfied"
else
  error "Scaffold finished with issues — scroll up to review agent output"
  echo ""
  echo "  Re-run:  ./scripts/scaffold-tests.sh $SERVICE_NAME"
  echo ""
  warn "If the agent repeatedly hits turn limit, the service may be too large"
  warn "for one session — check SKILL.md Step 1 for per-step running instructions"
fi