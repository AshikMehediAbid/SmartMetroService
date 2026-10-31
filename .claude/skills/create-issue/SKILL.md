---
name: create-issue
description: Run the SmartMetroService create-issue workflow: check for duplicates, draft a GitHub issue for approval, then create it. Only when the user types /create-issue.
argument-hint: <what is broken or wanted>
disable-model-invocation: true
---

Read `harness/workflows/create-issue.md` in full before doing anything else, then follow
its numbered steps in order without skipping, merging, or changing its output
format. Wherever that file refers to its argument, use this request: $ARGUMENTS
If a step says stop, stop.
