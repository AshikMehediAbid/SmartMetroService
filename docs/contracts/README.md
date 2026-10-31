# API contracts (client-facing)

A contract is the short list of things the Angular client's implementer cannot get from reading the API code: type
traps, values that come from the token or a cookie rather than the body, statuses not to rely on, redirects, and
breaking changes. It is **not** a copy of the request and response shapes; those are read from the controller and its
DTOs, or from `/openapi/v1.json` on a Development host. Conventions that hold for every route live in
[`../context/api-conventions.md`](../context/api-conventions.md), not here.

- A contract is written or updated, in the same PR, only when a **routed** HTTP surface the client calls changes.
  Verify the route exists on a controller action; a manager method is not an endpoint.
- Precedence: live code, then tests, then `docs/adr/`, then the contract. If a contract and the code disagree, the
  code is right and the contract is the bug; fix it in the same PR.
- Name: `<area>-<endpoint-or-feature>.md` (e.g. `payment-callbacks.md`). Start from
  [`_CONTRACT_TEMPLATE.md`](./_CONTRACT_TEMPLATE.md).
- No index here; `ls`.

No contracts exist yet. The first candidates, because their traps are easy to miss, are the payment flow (purchase
returns a URL or `null`; callbacks redirect to `BaseFeUrl` with `status`), the refresh-token cookie, and the scanner.
