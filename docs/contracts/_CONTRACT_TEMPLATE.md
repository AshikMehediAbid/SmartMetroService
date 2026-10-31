# {{FEATURE_OR_ENDPOINT_NAME}}

**Issue:** #{{N}}

`{{METHOD}} {{PATH}}`, {{auth: Bearer | Bearer + Admin | anonymous | refresh-token cookie}}. Shapes and validation:
read the controller and its DTOs; do not copy them here.

What the code does not tell a client:

- {{a type trap, e.g. an enum that serializes as a string}}
- {{a value taken from the token or a cookie, not the body}}
- {{a status, redirect or body the client must not key logic off}}

Breaking changes for existing clients: {{none | what}}

Decisions: `docs/adr/` {{NNNN}}
