---
name: harness
description: Run any SmartMetroService harness workflow by name, including one added after setup. Only when the user types /harness.
argument-hint: <workflow> [arguments]
disable-model-invocation: true
---

The request is: $ARGUMENTS
Its first word is a workflow name and the rest is that workflow's argument.
List `harness/workflows/`. With no name, or a name that has no file there,
show the names and stop. Otherwise read `harness/workflows/<name>.md` in full
before doing anything else, then follow its numbered steps in order without
skipping, merging, or changing its output format. Where that file refers to
its argument, use the rest of the request. If a step says stop, stop.
