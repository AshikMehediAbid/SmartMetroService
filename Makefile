# Local CI — the gate that must be green before any PR is opened
# (AGENTS.md §7). Every target calls scripts/ci.sh, which the pre-push
# hook and .github/workflows/ci.yml call too, so they stay in step.
#
# Tests need SMARTMETRO_TEST_DB (a throwaway database whose name contains
# "test"): they delete the database they connect to. See AGENTS.md §2a.

.PHONY: ci build test install-hooks

## ci: everything the PR gate runs
ci:
	bash scripts/ci.sh

## build: build the solution and the test project, no tests
build:
	bash scripts/ci.sh --build-only

## test: same as ci (the tests always need a fresh build)
test:
	bash scripts/ci.sh

## install-hooks: point git at the repo-tracked hooks
install-hooks:
	git config core.hooksPath .githooks
	@echo "hooks installed (core.hooksPath = .githooks)"
