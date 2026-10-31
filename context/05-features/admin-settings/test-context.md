---
type: feature-test-context
title: Admin settings test contract
description: Test expectations for administrator authorization and fare-setting persistence.
tags: [admin, settings, tests]
spec_version: 1
last_review: 2026-10-01
---
# Admin Settings Test Context

No admin-settings-specific tests were identified in the current test project.

| Criterion | Test expectation |
| --- | --- |
| AC1 | Unauthenticated and non-admin requests fail; Admin role succeeds for both methods. |
| AC2 | Read returns persisted settings and handles absent configuration as agreed. |
| AC3 | Valid updates persist; invalid bounds/shape are rejected without partial changes. |
| AC4 | Station and scanner fare behavior observes the updated settings. |

Include both manual JWT and Keycloak role claim paths when auth configuration is changed.
