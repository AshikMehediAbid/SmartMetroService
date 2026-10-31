---
name: check-pr-comments
description: Run the SmartMetroService check-pr-comments workflow after a push to a PR branch: test-merge against master, list unresolved review threads and check status. Only when the user types /check-pr-comments.
argument-hint: [pr-number]
disable-model-invocation: true
---

Read `harness/workflows/check-pr-comments.md` in full before doing anything else, then follow
its numbered steps in order without skipping, merging, or changing its output
format. Wherever that file refers to its argument, use this request: $ARGUMENTS
If a step says stop, stop.
