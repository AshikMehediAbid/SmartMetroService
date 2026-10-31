<!-- Line 1: the understanding-check result (> 🧠✅, or the override block). harness/playbooks/understanding-check.md -->

**In short:** <!-- one or two plain sentences, no jargon: what this PR does and why, for someone who reads nothing else -->

Closes #<!-- issue number; the merge closes the issue -->

## Summary
- <!-- bullet describing change 1 -->
- <!-- bullet describing change 2 -->

## Changes Made
### API / managers
- <!-- controller/manager: what changed and why -->

### Database
- <!-- migration <timestamp>_<Name>: tables, columns, indexes — or "none" -->

### Tests
- <!-- new or updated tests -->

## Contracts and ADRs
- **Contract**: <!-- docs/contracts/<area>-<endpoint>.md added or updated in this PR, or "none — no client-facing route changed" -->
- **ADR**: <!-- docs/adr/NNNN-<slug>.md added in this PR, or "none" -->

## Merge Danger
- **Door**: <!-- one-way (hard to undo: a migration, a data rewrite, a wallet/payment change, a contract the Angular app depends on) or two-way (a revert undoes it) -->
- **Blast radius**: <!-- who or what breaks if this is wrong, e.g. "every wallet top-up", "admin station edits" -->

## Security Review
| Area | Finding |
|------|---------|
| Identity and ownership | <!-- e.g. "User from the token; another user's ticket → 404, tested" --> |
| Auth | <!-- e.g. "[Authorize] on the new action; admin route requires Admin" --> |
| Money and payments | <!-- e.g. "Fare computed server-side; debit and ticket saved together; double submit tested" or "N/A" --> |
| Input validation / injection | <!-- e.g. "DTO validated; no raw SQL" --> |
| Secrets / data exposure | <!-- e.g. "No new config secrets; no PII or tokens logged" --> |

## Agent Reviews
- **Security**: <!-- summary of security-reviewer findings, or "No issues found" -->
- **Domain invariants**: <!-- summary of domain-invariant-reviewer findings, or "N/A — no fare/wallet/payment/ticket/endpoint changes" -->
- **Test coverage**: <!-- summary of test-coverage-reviewer findings, or "Coverage in place" -->
- **Data access**: <!-- summary of data-access-reviewer findings, or "N/A — no data access changes" -->
- **Architecture**: <!-- summary of architecture-guardian findings, or "N/A — no new manager/repository/controller" -->
- **Final review**: <!-- final-reviewer verdict: APPROVED, and the fix rounds it took -->

## Test Plan
- [ ] <!-- bash scripts/ci.sh — N passed, N failed (the known AuthFlowTests baseline failure counts here), against SMARTMETRO_TEST_DB -->
- [ ] <!-- manual check against the local stack -->

<!-- If an agent authored this PR, say which one here. -->
