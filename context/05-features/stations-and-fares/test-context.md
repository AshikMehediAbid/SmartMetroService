---
type: feature-test-context
title: Station and fare test contract
description: Test expectations for station ordering and fare calculation.
tags: [station, fare, tests]
spec_version: 1
last_review: 2026-10-01
---
# Station and Fare Test Context

No station/fare-specific tests were identified in the current test project.

| Criterion | Test expectation |
| --- | --- |
| AC1 | Ascending/descending listing and correct adjacent-distance values. |
| AC2 | Duplicate names, invalid insertion position, and contiguous order after insertion/move. |
| AC3 | First/middle/last station move updates expected neighboring distance records only. |
| AC4 | Missing station, referenced station, and subsequent order after deletion. |
| AC5 | Pair fare and origin-to-destinations results; boundary/missing-station behavior. |

Agree on fare formula, rounding, same-station behavior, and active-station policy before encoding those as expected values.
