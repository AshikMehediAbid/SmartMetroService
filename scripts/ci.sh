#!/usr/bin/env bash
#
# The local CI gate (AGENTS.md §7). One script so the Makefile, the
# pre-push hook and .github/workflows/ci.yml cannot drift apart.
#
#   bash scripts/ci.sh                      build both, then run the tests
#   bash scripts/ci.sh --build-only         build both, no tests (what CI runs today)
#   bash scripts/ci.sh --filter <expr>      build both, run matching tests only
#
# Why the tests need SMARTMETRO_TEST_DB: the test host runs as Development,
# so it loads your user secrets, and AuthFlowTests calls EnsureDeleted() on
# the database it connects to. A bare `dotnet test` therefore wipes your dev
# database (golden-principles.md §9). This script points
# ConnectionStrings__DefaultConnection (environment variables override user
# secrets) at SMARTMETRO_TEST_DB, and refuses to run without it or when the
# database name does not contain "test".
#
# The test project is not in SmartMetroService.slnx, so it is built and
# tested by path; `dotnet test` at the root would find no tests and pass.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

build_only=0
filter=""
while [ $# -gt 0 ]; do
  case "$1" in
    --build-only) build_only=1; shift ;;
    --filter) filter="${2:?--filter needs an expression}"; shift 2 ;;
    *) echo "ci.sh: unknown argument: $1" >&2; exit 2 ;;
  esac
done

echo "── build: SmartMetroService.slnx ──────────────"
dotnet build SmartMetroService.slnx -nologo
echo "── build: SmartMetroService.Tests ─────────────"
dotnet build SmartMetroService.Tests -nologo

if [ "$build_only" = 1 ]; then
  echo "ci.sh: build only — tests not run."
  exit 0
fi

if [ -z "${SMARTMETRO_TEST_DB:-}" ]; then
  cat >&2 <<'MSG'
ci.sh: SMARTMETRO_TEST_DB is not set, so the tests were not run and the gate fails.
       The tests delete the database they connect to. Point them at a throwaway one, e.g. LocalDB:
         export SMARTMETRO_TEST_DB='Server=(localdb)\MSSQLLocalDB;Database=SmartMetro_Test;Trusted_Connection=True;TrustServerCertificate=True'
MSG
  exit 1
fi

# The database name must say it is a test database: a last guard against
# pasting the dev connection string.
db_name="$(printf '%s' "$SMARTMETRO_TEST_DB" | tr '[:upper:]' '[:lower:]' \
  | sed -nE 's/.*(^|;)[[:space:]]*(database|initial catalog)[[:space:]]*=[[:space:]]*([^;]*).*/\3/p')"
case "$db_name" in
  *test*) ;;
  *)
    echo "ci.sh: the database in SMARTMETRO_TEST_DB is '${db_name:-<none>}'. Its name must contain 'test'," >&2
    echo "       because the tests delete it. Use a throwaway database." >&2
    exit 1
    ;;
esac

export ConnectionStrings__DefaultConnection="$SMARTMETRO_TEST_DB"
echo "── test: SmartMetroService.Tests (database: $db_name) ──"
if [ -n "$filter" ]; then
  dotnet test SmartMetroService.Tests --no-build -nologo --filter "$filter"
else
  dotnet test SmartMetroService.Tests --no-build -nologo
fi
