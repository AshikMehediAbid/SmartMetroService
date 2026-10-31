# SmartMetroService context

This folder is the compact, maintained specification for AI-assisted work on SmartMetroService. It follows the spec-led AIDLC pattern in Context-Boilerplate, adapted to this repository rather than copied as a generic template.

## Start here

1. Read `architecture-map.md` and `agent-rules.md` before every task.
2. Load only the shared and feature documents named by the map.
3. For a behavior change, agree on the requirement first, update the affected feature contract, then implement and test it.
4. Update the technical and test contracts when code changes make them stale.

The repository currently contains the .NET API and its tests; no client application is included. Existing code is evidence of current behavior, not an automatic endorsement of every behavior as a product requirement. Open questions are labeled instead of guessed. Secrets and environment-specific values do not belong in this folder.

See `WORKFLOW.md` for the end-to-end process and `index.yml` for the machine-readable map.
