---
name: open-pr
kind: workflow
description: >-
  Open an existing GitHub pull request's file diffs in the current
  editor. The argument is the PR number. If that PR does not exist,
  say so and stop.
arguments: pull request number, e.g. 12
---

Open the file diffs for an existing pull request in the editor window
that is already open. `$ARGUMENTS` is the pull request number, for
example `/open-pr 12`.

This command only reads. It does not create a pull request, push,
commit, comment, review, merge, check out a branch, or re-run checks.
It does not open a browser. After the report below, stop.

`REPO` is `AshikMehediAbid/SmartMetroService` (`AGENTS.md` §3). Run the
snippets in bash from the repo checkout or any of its worktrees.

## Step 1 — Argument

`$ARGUMENTS` must be digits only. If it is missing or contains anything
else, stop and say the argument is the pull request number, for example
`12`.

## Step 2 — Look up the pull request

```bash
gh pr view "$ARGUMENTS" --repo "$REPO" --json number,title,state,baseRefName,url
```

- If it fails with `Could not resolve to a PullRequest`, say
  `PR <id> does not exist` and stop.
- Any other failure (auth, network): print that error and stop. Do not
  describe it as a missing pull request.

Keep `number`, `title`, `state` and `baseRefName`.

## Step 3 — Fetch the commits and write diff sides

The tabs must show the pull request, so both sides are blobs. Do not
open files from the working tree. Leave the temp directory on disk for
the rest of the session so the tabs keep their contents.

Run the block as one bash script. Set `PR_ID` and `BASE`
(`baseRefName` from step 2). It checks the checkout the same way as
[`review-pr`](./review-pr.md) step 3 and fetches the PR head and the
base branch into scratch refs `refs/sms-review/pr-<id>/source` and
`.../target`, without moving `refs/remotes/origin/*`. Do not check
those refs out and do not push them.

```bash
PR_ID="<id>"; REPO="AshikMehediAbid/SmartMetroService"; BASE="<baseRefName>"
case "$PR_ID" in ''|*[!0-9]*) printf 'the argument is the pull request number\n'; exit 1 ;; esac
case "$BASE" in ''|*[!A-Za-z0-9._/-]*) printf 'unusable base branch name\n'; exit 1 ;; esac

cd "$(git rev-parse --show-toplevel)" || { printf 'Run this inside a checkout of %s.\n' "$REPO"; exit 1; }
url=$(git remote get-url origin); url=${url%/}; url=${url%.git}; url=${url#*github.com[:/]}
if [ "$(printf '%s' "$url" | tr '[:upper:]' '[:lower:]')" != "$(printf '%s' "$REPO" | tr '[:upper:]' '[:lower:]')" ]; then
  printf 'origin is %s, not %s.\n' "$url" "$REPO"; exit 1
fi

SCRATCH="refs/sms-review/pr-$PR_ID"
if ! git fetch --refmap= --no-tags origin \
    "+refs/pull/$PR_ID/head:$SCRATCH/source" \
    "+refs/heads/$BASE:$SCRATCH/target" >/dev/null 2>&1; then
  printf 'PR %s exists but its commits could not be fetched\n' "$PR_ID"; exit 1
fi
TARGET_REV="$SCRATCH/target"; SOURCE_REV="$SCRATCH/source"
if ! base=$(git merge-base "$TARGET_REV" "$SOURCE_REV"); then
  printf 'PR %s has no merge base with %s\n' "$PR_ID" "$BASE"; exit 1
fi

tmp=$(mktemp -d "${TMPDIR:-/tmp}/pr-${PR_ID}-diffs.XXXXXX")
manifest="$tmp/manifest.tsv"; skipped="$tmp/skipped.txt"; files="$tmp/files.txt"
: > "$manifest"; : > "$skipped"; : > "$files"

to_editor_path() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s\n' "$1"; fi
}
is_binary() {
  local added deleted
  read -r added deleted _ < <(git -c core.quotePath=false --no-pager diff --numstat --find-renames --find-copies "$TARGET_REV...$SOURCE_REV" -- "$1")
  [ "$added" = "-" ] && [ "$deleted" = "-" ]
}
is_gitlink() {
  [ "$(git ls-tree "$1" -- "$2" | cut -d ' ' -f 1)" = "160000" ]
}
write_blob() {   # dest rev path; an empty rev writes an empty file
  mkdir -p "$(dirname "$1")"
  if [ -z "$2" ]; then : > "$1"; else git cat-file blob "$2:$3" > "$1"; fi
}

while IFS= read -r -d '' status; do
  IFS= read -r -d '' path_a || break
  path_b=""
  case "$status" in R*|C*) IFS= read -r -d '' path_b || break ;; esac
  [ -n "$status" ] || continue
  rel="$path_a"; left_rev="$base"; left_path="$path_a"; right_rev="$SOURCE_REV"; right_path="$path_a"
  case "$status" in
    A*) left_rev="" ;;
    D*) right_rev="" ;;
    R*|C*) rel="$path_b"; right_path="$path_b" ;;
  esac
  printf '%s\n' "$rel" >> "$files"
  skip=0
  case "$rel" in *$'\n'*|*$'\t'*) skip=1 ;; esac
  [ "$skip" -eq 0 ] && is_binary "$rel" && skip=1
  [ "$skip" -eq 0 ] && [ -n "$left_rev" ] && is_gitlink "$left_rev" "$left_path" && skip=1
  [ "$skip" -eq 0 ] && [ -n "$right_rev" ] && is_gitlink "$right_rev" "$right_path" && skip=1
  if [ "$skip" -eq 1 ]; then printf '%s\n' "$rel" >> "$skipped"; continue; fi
  left_file="$tmp/left/$rel"; right_file="$tmp/right/$rel"
  if ! write_blob "$left_file" "$left_rev" "$left_path" || ! write_blob "$right_file" "$right_rev" "$right_path"; then
    printf '%s\n' "$rel" >> "$skipped"; continue
  fi
  printf '%s\t%s\t%s\n' "$(to_editor_path "$left_file")" "$(to_editor_path "$right_file")" "$rel" >> "$manifest"
done < <(git -c core.quotePath=false --no-pager diff -z --name-status --find-renames --find-copies "$TARGET_REV...$SOURCE_REV")

git update-ref -d "$SCRATCH/target"
git update-ref -d "$SCRATCH/source"

printf 'TEMP %s\n' "$tmp"
printf 'MANIFEST %s\n' "$manifest"
printf 'FILES %s\n' "$files"
```

`manifest.tsv` has one row per text file: editor path of the left side,
editor path of the right side, repo-relative label. Added files have an
empty left file. Deleted files have an empty right file. Renames and
copies use the old path at the merge base and the new path at the PR
head. The repo-relative path is kept under `left/` and `right/` so each
tab is that file.

## Step 4 — Open every text diff in this window

Pick one editor binary, in this order:

1. `cursor`, when `CURSOR_AGENT` is set.
2. `code`, when that command exists.
3. Otherwise skip the tabs.

Invoke that command by name from `PATH`. Do not call a second install
by its full path. For each manifest row, once, in order:

```text
<binary> -r --diff <left> <right>
```

Do not pass `--wait`. Do not open a second window. Wait until that
process returns before the next row. Open every row. There is no cap.

If that command exits non-zero, stop opening more, print its error, and
still give the report with the number of tabs opened before the
failure.

If no binary was chosen, do not invent another view. Say the IDE step
was skipped, and still give the report with `Opened: 0 diff tabs`.
The file list below is still required.

## Step 5 — Report

Print the title as plain text. Then list every changed path from
`files.txt`, one per line, in that order. That order is the diff order,
including skipped paths where they appear. Use `none` when the file is
empty.

```text
PR <id>
Title: <title>
State: <state>
Opened: <n> diff tabs
Skipped: <paths, or none>
IDE: <opened or skipped>
Files:
- <repo-relative path>
- <repo-relative path>
```

`Skipped` is the binary files, gitlinks, and any path whose blob could
not be written, on one line. Use `none` when that list is empty. A path
that is skipped is still listed under `Files`.

Then stop.
