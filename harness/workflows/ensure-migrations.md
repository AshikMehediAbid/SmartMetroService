---
name: ensure-migrations
kind: workflow
description: >-
  Check that schema changes on this branch have a correctly generated EF
  Core migration (with its Designer file and an updated model snapshot),
  that the mapping matches it, and that it is safe for existing rows.
  Use whenever a change touches entities, mapping, keys, indexes or
  column types.
---

The schema is EF Core code-first (`AGENTS.md` §8 *Data and
migrations*). Migrations live in `SmartMetroService.Storage/Migrations`;
the mapping is in `SmartMetroService.Storage/Sql/MyApplicationDbContext.cs`
plus EF conventions on the entities in `SmartMetroService.Domain`.

Commands run from the worktree root. `dotnet ef` needs the tool
(`dotnet tool install --global dotnet-ef`) and the startup project:

```bash
EF="dotnet ef --project SmartMetroService.Storage --startup-project SmartMetroService.Api"
```

## Step 0 — Existing migrations are untouched

```bash
git fetch origin
git diff --name-status origin/master...HEAD -- SmartMetroService.Storage/Migrations/
```

Allowed: `A` (added) migration and Designer files, and `M` on
`MyApplicationDbContextModelSnapshot.cs`. Any other modified, renamed or
deleted migration is a stop: report it. A migration on `master` is never
edited; the fix is a new migration.

## Step 1 — Is a migration needed?

List what the change's code now relies on in the database: tables,
columns, types, nullability, keys, foreign keys and delete behavior,
indexes (unique ones especially). Then ask EF whether the model and the
snapshot differ:

```bash
$EF migrations has-pending-model-changes
```

- Exit 0 / "No changes": the model matches the snapshot. If the branch
  already added a migration, go to step 3; otherwise report "no
  migration needed" and stop.
- Changes found and no migration on the branch yet: go to step 2.
- An error: report it verbatim (it usually means the startup project
  does not build or configuration is missing) and stop.

## Step 2 — Generate it

Ask the user for the name, suggesting one in the repo's style from the
change (PascalCase words joined by underscores, e.g.
`Add_Unique_Index_On_UserWallet_UserId`). Then:

```bash
$EF migrations add <Name>
```

This writes `<timestamp>_<Name>.cs`, `<timestamp>_<Name>.Designer.cs`
and updates `MyApplicationDbContextModelSnapshot.cs`. Never hand-write a
migration or remove the Designer file: EF finds migrations only through
the `[Migration]` attribute in it (known gap G9 shows what happens
otherwise).

If another open branch also adds a migration, one of the two will have
to regenerate on top of the other after merge (the snapshot conflicts).

## Step 3 — Read it

Open the generated `Up` and `Down` and walk this checklist (sources:
`AGENTS.md` §8, `docs/context/data-model.md`):

- [ ] `Up` contains only the intended changes; nothing unrelated slipped
      in from earlier un-migrated model edits.
- [ ] `Down` reverses `Up`.
- [ ] Money is `decimal(18,2)`, set with `HasPrecision(18, 2)` in the
      mapping (EF warns when it is left to the default); no new
      `float`/`real` money column.
- [ ] Instants are `datetime2` and the code writes UTC.
- [ ] Strings that are indexed have a length (not `nvarchar(max)`).
- [ ] Foreign keys have indexes; delete behavior is deliberate
      (`Cascade` only where deleting the parent should delete the rows).
- [ ] Uniqueness the domain relies on is a unique index.
- [ ] Enum columns stay `int`; no enum member was renumbered.
- [ ] Destructive changes (drop, rename, narrowing a type, `NOT NULL` on
      a populated column, a new unique index on data that may hold
      duplicates) are flagged to the user, with a data step
      (`migrationBuilder.Sql`) that keeps existing rows valid.
- [ ] `migrationBuilder.Sql` only where the builder cannot express it,
      with a comment saying why.

## Step 4 — Apply it to a throwaway database

Apply the whole migration chain to an empty test database, never to a
database that holds data you need (`golden-principles.md` §9):

```bash
ConnectionStrings__DefaultConnection="$SMARTMETRO_TEST_DB" $EF database update
ConnectionStrings__DefaultConnection="$SMARTMETRO_TEST_DB" $EF migrations has-pending-model-changes
```

`SMARTMETRO_TEST_DB` must name a throwaway database whose name contains
`test` (`AGENTS.md` §3). If it is not set, say the migration was not
applied and report it as not verified. Drop and recreate that database
first if an earlier run left it in a different state.

## Step 5 — Build and test

```bash
bash scripts/ci.sh
```

Report builds, warnings and test counts; the tests use `EnsureCreated`,
not migrations, so step 4 is the only check that the migration itself
runs.

## Step 6 — Report

Migration file names, a summary of the schema changes, the checklist
result, whether step 4 applied it to a throwaway database (or why not),
and the gate result. Hand the migration to `data-access-reviewer` in a
separate context.
