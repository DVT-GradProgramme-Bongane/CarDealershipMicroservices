#!/usr/bin/env bash
# scaffold-tests.sh
# Run from the solution root to scaffold or append tests for a single service.
#
# Usage:
#   ./scripts/scaffold-tests.sh <ServiceName>
#
# Examples:
#   ./scripts/scaffold-tests.sh StaffService
#   ./scripts/scaffold-tests.sh ClientService
#
# Requirements:
#   - claude CLI (Claude Code) installed and authenticated
#   - .NET SDK installed
#   - Docker running (for integration tests via Testcontainers)

set -euo pipefail

# ── Colours for terminal output ───────────────────────────────────────────────

BOLD='\033[1m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
RESET='\033[0m'

info()    { echo -e "${CYAN}▶${RESET} $*"; }
success() { echo -e "${GREEN}✓${RESET} $*"; }
warn()    { echo -e "${YELLOW}⚠${RESET} $*"; }
error()   { echo -e "${RED}✗${RESET} $*"; }
section() { echo -e "\n${BOLD}── $* ${RESET}$(printf '─%.0s' {1..40})\n"; }

# ── Spinner for long-running steps ────────────────────────────────────────────
# Shows the user something is happening while the agent works

SPINNER_PID=""

start_spinner() {
  local msg="$1"
  local frames=("⠋" "⠙" "⠹" "⠸" "⠼" "⠴" "⠦" "⠧" "⠇" "⠏")
  (
    i=0
    while true; do
      printf "\r${CYAN}${frames[$((i % 10))]}${RESET} %s" "$msg"
      sleep 0.1
      i=$((i + 1))
    done
  ) &
  SPINNER_PID=$!
  disown "$SPINNER_PID"
}

stop_spinner() {
  if [ -n "$SPINNER_PID" ] && kill -0 "$SPINNER_PID" 2>/dev/null; then
    kill "$SPINNER_PID" 2>/dev/null
    wait "$SPINNER_PID" 2>/dev/null || true
    printf "\r\033[K"  # clear spinner line
  fi
  SPINNER_PID=""
}

# Always stop spinner on exit
trap 'stop_spinner' EXIT

# ── Validate input ────────────────────────────────────────────────────────────

if [ $# -ne 1 ]; then
  error "Usage: $0 <ServiceName>"
  error "Example: $0 StaffService"
  exit 1
fi

SERVICE_NAME="$1"
# Convert PascalCase to kebab-case for workflow filename: StaffService → staff-service
SERVICE_NAME_LOWER=$(echo "$SERVICE_NAME" | sed 's/\([A-Z]\)/-\1/g' | sed 's/^-//' | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Tests live INSIDE the service folder
SERVICE_DIR="$SOLUTION_ROOT/$SERVICE_NAME"
TESTS_DIR="$SERVICE_DIR/$SERVICE_NAME.Tests"
WORKFLOW_FILE="$SOLUTION_ROOT/.github/workflows/$SERVICE_NAME_LOWER-ci.yml"

section "CarDealership Test Scaffold"
info "Service:       $SERVICE_NAME"
info "Service path:  $SERVICE_DIR"
info "Tests path:    $TESTS_DIR"
info "Solution root: $SOLUTION_ROOT"

# ── Validate service exists ───────────────────────────────────────────────────

section "Pre-flight checks"

if [ ! -d "$SERVICE_DIR" ]; then
  error "Service directory not found: $SERVICE_DIR"
  echo ""
  echo "Available .NET services:"
  find "$SOLUTION_ROOT" -maxdepth 2 -name "*.csproj" \
    ! -path "*/Tests/*" \
    ! -path "*/.git/*" \
    | xargs -I{} dirname {} \
    | xargs -I{} basename {} \
    | sort
  exit 1
fi

if [ ! -f "$SERVICE_DIR/$SERVICE_NAME.csproj" ]; then
  error "No .csproj found in $SERVICE_NAME — is this a .NET service?"
  exit 1
fi
success "Service directory found"

if [ ! -f "$SOLUTION_ROOT/skills/SKILL.md" ]; then
  error "SKILL.md not found at $SOLUTION_ROOT/skills/SKILL.md"
  exit 1
fi
success "SKILL.md found"

if [ ! -f "$SOLUTION_ROOT/skills/PROMPT.md" ]; then
  error "PROMPT.md not found at $SOLUTION_ROOT/skills/PROMPT.md"
  exit 1
fi
success "PROMPT.md found"

# Check claude CLI
if ! command -v claude &>/dev/null; then
  error "claude CLI not found on PATH"
  echo ""
  echo "Install Claude Code: https://docs.anthropic.com/en/docs/claude-code"
  exit 1
fi
success "claude CLI found: $(claude --version 2>/dev/null || echo 'installed')"

# ── Detect run mode ───────────────────────────────────────────────────────────

section "Run mode detection"

# Clean up empty dotnet scaffold default if it exists and has no real tests
DEFAULT_TEST_FILE="$TESTS_DIR/UnitTest1.cs"
if [ -f "$DEFAULT_TEST_FILE" ]; then
  warn "Found empty dotnet default test file — removing before agent runs"
  rm "$DEFAULT_TEST_FILE"
  info "Removed: $DEFAULT_TEST_FILE"
fi

if [ -d "$TESTS_DIR" ]; then
  EXISTING_TESTS=$(find "$TESTS_DIR" -name "*.cs" ! -name "*.csproj" 2>/dev/null | wc -l | tr -d ' ')
  if [ "$EXISTING_TESTS" -gt 0 ]; then
    RUN_MODE="append"
    info "Mode: APPEND — $TESTS_DIR exists with $EXISTING_TESTS test file(s)"
    info "Agent will read existing tests and add coverage for new endpoints only"
  else
    RUN_MODE="scaffold"
    info "Mode: SCAFFOLD (first-time) — test project exists but has no test files"
  fi
else
  RUN_MODE="scaffold"
  info "Mode: SCAFFOLD (first-time) — $SERVICE_NAME.Tests does not exist yet"
fi

# ── Build the prompt ──────────────────────────────────────────────────────────

section "Building agent prompt"

PROMPT=$(sed \
  -e "s|%%SERVICE_NAME%%|$SERVICE_NAME|g" \
  -e "s|%%SERVICE_NAME_LOWER%%|$SERVICE_NAME_LOWER|g" \
  -e "s|%%TESTS_DIR%%|$TESTS_DIR|g" \
  -e "s|%%SERVICE_DIR%%|$SERVICE_DIR|g" \
  -e "s|%%SOLUTION_ROOT%%|$SOLUTION_ROOT|g" \
  "$SOLUTION_ROOT/skills/PROMPT.md")

success "Prompt built ($(echo "$PROMPT" | wc -w | tr -d ' ') words)"

# ── Run the agent ─────────────────────────────────────────────────────────────

section "Running agent"

echo -e "${YELLOW}The agent is now working. This takes 3-8 minutes for a full scaffold.${RESET}"
echo -e "${YELLOW}Progress is streamed below as the agent reports each step.${RESET}"
echo ""

cd "$SOLUTION_ROOT"

# --verbose streams agent reasoning and tool calls to the terminal in real time
# --max-turns 80 gives enough budget for: reads (10) + writes (20) + builds (10) + fixes (40)
# Remove --print so output streams live rather than buffering until completion
echo "$PROMPT" | claude \
  --verbose \
  --max-turns 80 \
  --allowedTools "Read,Write,Edit,Bash" \
  -p -

AGENT_EXIT=$?

if [ $AGENT_EXIT -ne 0 ]; then
  warn "Agent exited with code $AGENT_EXIT — checking what was produced..."
fi

# ── Post-run verification ─────────────────────────────────────────────────────

section "Verifying output"

PASS=true

# 1. Test project folder
if [ ! -d "$TESTS_DIR" ]; then
  error "Test project not created at expected path: $TESTS_DIR"
  error "Remember: tests live inside the service folder, not at solution root"
  PASS=false
else
  success "Test project exists at $TESTS_DIR"
fi

# 2. Test project csproj
if [ -d "$TESTS_DIR" ] && [ ! -f "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" ]; then
  error "Missing .csproj in test project"
  PASS=false
elif [ -d "$TESTS_DIR" ]; then
  success "Test .csproj found"
fi

# 3. Actual test files (not just the scaffold boilerplate)
if [ -d "$TESTS_DIR" ]; then
  TEST_FILE_COUNT=$(find "$TESTS_DIR" -name "*Tests.cs" ! -name "UnitTest1.cs" 2>/dev/null | wc -l | tr -d ' ')
  if [ "$TEST_FILE_COUNT" -eq 0 ]; then
    error "No real test files found — agent may have run out of turns before writing tests"
    PASS=false
  else
    success "Found $TEST_FILE_COUNT test file(s)"
    find "$TESTS_DIR" -name "*Tests.cs" | while read -r f; do
      info "  $(basename "$f")"
    done
  fi
fi

# 4. CI workflow (scaffold only)
if [ "$RUN_MODE" = "scaffold" ]; then
  if [ ! -f "$WORKFLOW_FILE" ]; then
    warn "CI workflow not created: $WORKFLOW_FILE"
    warn "You can run the agent again or create it manually"
  else
    success "CI workflow created at .github/workflows/$SERVICE_NAME_LOWER-ci.yml"
  fi
fi

# 5. In solution file
SLN_FILE=$(find "$SOLUTION_ROOT" -maxdepth 1 -name "*.sln" | head -1)
if [ -n "$SLN_FILE" ]; then
  if dotnet sln "$SLN_FILE" list 2>/dev/null | grep -q "$SERVICE_NAME.Tests"; then
    success "Test project registered in solution"
  else
    warn "Test project not in solution — running: dotnet sln add"
    dotnet sln "$SLN_FILE" add "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" && \
      success "Added to solution" || \
      error "Failed to add to solution — add manually"
  fi
fi

# 6. Build
if [ -d "$TESTS_DIR" ] && [ "$PASS" = "true" ]; then
  echo ""
  info "Building test project..."
  start_spinner "dotnet build running..."
  if dotnet build "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" --nologo -q 2>&1; then
    stop_spinner
    success "Build passed"
  else
    stop_spinner
    error "Build failed"
    PASS=false
  fi
fi

# 7. Unit tests
if [ -d "$TESTS_DIR" ] && [ "$PASS" = "true" ]; then
  echo ""
  info "Running unit tests..."
  start_spinner "dotnet test running..."
  if dotnet test "$TESTS_DIR/$SERVICE_NAME.Tests.csproj" \
      --filter "FullyQualifiedName~Unit" \
      --nologo -q 2>&1; then
    stop_spinner
    success "Unit tests passed"
  else
    stop_spinner
    warn "Unit tests failed — this may indicate real issues in the service under test"
    warn "Review the test output — failing tests are expected and useful"
  fi
fi

# ── Summary ───────────────────────────────────────────────────────────────────

section "Summary"

if [ "$PASS" = "true" ]; then
  success "Scaffold complete for $SERVICE_NAME"
  echo ""
  echo "  Tests location:  $TESTS_DIR"
  echo "  Run unit tests:  dotnet test $SERVICE_NAME/$SERVICE_NAME.Tests --filter \"FullyQualifiedName~Unit\""
  echo "  Run int. tests:  dotnet test $SERVICE_NAME/$SERVICE_NAME.Tests --filter \"FullyQualifiedName~Integration\""
  echo ""
  info "Integration tests require Docker — start it before running"
  info "Review tests before committing — the agent may have left // SCAFFOLD markers"
else
  error "Scaffold finished with issues"
  echo ""
  echo "  Common causes:"
  echo "  • Agent hit turn limit before writing tests → re-run the script"
  echo "  • Test folder created at wrong path → check $TESTS_DIR"
  echo "  • Build errors → check agent output above for compiler messages"
  echo ""
  warn "The agent outputs a summary of blockers at the end of its run — scroll up to read it"
fi