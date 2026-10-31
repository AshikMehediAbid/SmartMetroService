---
name: check-pr-comments
kind: workflow
description: >-
  After pushing to a PR branch, check the PR once: test-merge the
  branch against fresh master, list unresolved review threads, review
  verdicts and check status, and wait for running checks to finish. No
  polling for new comments.
arguments: pull request number (optional; defaults to the PR for the current branch)
---

After every `git push` to a PR branch, check the PR **once**. Run it
again when the user asks (for example "check the PR comments").

Don't poll for new review threads. Nothing posts on our PRs
automatically: no review bot posts comments, `review-pr` reports in
the conversation, and a reviewer agent you start yourself signals
completion when it returns. Human reviewers take hours, not minutes.

`REPO` is `AshikMehediAbid/SmartMetroService` (`AGENTS.md` §3). Every `gh`
call passes `--repo "$REPO"`.

The PR number is `$ARGUMENTS` when given; otherwise resolve it from the
current branch:

```bash
gh pr view "$(git branch --show-current)" --repo "$REPO" --json number,url,state -q '[.number,.state,.url]|@tsv'
```

If no PR exists for the branch, say so and stop.

## Protocol

1. **Test-merge the branch against fresh `master`.** Run it in the PR
   branch's worktree, with `HEAD` at the commit you pushed:
   ```bash
   git fetch -q origin master
   git merge-tree --write-tree --name-only --no-messages origin/master HEAD   # git >= 2.38
   # exit 0 = clean; exit 1 = conflicts, listed after the tree id on line 1
   # older git: this count must be 0
   git merge-tree "$(git merge-base HEAD origin/master)" HEAD origin/master | grep -c '^+<<<<<<<'
   ```
   Zero conflicts is required. On a conflict, `git merge origin/master`
   into the branch (where both sides appended tests, keep both; where
   both added a migration with the same number, renumber this
   branch's), run `bash scripts/ci.sh` on the result, show the user the push, and
   once they approve push the merge commit. Then continue with step 2.

   GitHub's `mergeable` field is not evidence: it is computed lazily,
   can read `UNKNOWN`, and can lag behind `master`. Neither is
   `git diff origin/master...HEAD`, a merge-base diff that never tests
   mergeability.
2. **List the unresolved review threads.** Don't wait first:
   ```bash
   gh api graphql -f owner="${REPO%/*}" -f name="${REPO#*/}" -F number=<PR> -f query='
     query($owner:String!,$name:String!,$number:Int!){
       repository(owner:$owner,name:$name){ pullRequest(number:$number){
         reviewThreads(first:100){ nodes{
           id isResolved isOutdated path line
           comments(first:1){ nodes{ author{login} body } } } } } } }' \
     --jq '.data.repository.pullRequest.reviewThreads.nodes[] | select(.isResolved|not)
           | {id, path, line, outdated: .isOutdated, by: .comments.nodes[0].author.login, first: .comments.nodes[0].body}'
   ```
   Also read the review verdicts and the conversation comments, which
   are not threads and cannot be resolved:
   ```bash
   gh pr view <PR> --repo "$REPO" --json reviews,comments \
     --jq '{reviews: [.reviews[] | {by: .author.login, state, body}], comments: [.comments[] | {by: .author.login, body}]}'
   ```
3. If any threads are unresolved, or a review is `CHANGES_REQUESTED`,
   hand them off to
   [`respond-to-pr-comments`](./respond-to-pr-comments.md) to reply
   and resolve.
4. **Check the status checks** and report them to the user in a
   compact table:
   ```bash
   gh pr checks <PR> --repo "$REPO" --json name,state,bucket,link
   ```
   The `ci` check builds the solution and the test project; it does not
   run the tests yet (`AGENTS.md` §7), so a green `ci` says nothing
   about test results. Report the local `scripts/ci.sh` result next to
   it.
5. **Wait for running checks to finish, and only for those.** If any
   check's `bucket` is `pending`, wait for it with your tool's
   background wait if it has one, otherwise:
   ```bash
   gh pr checks <PR> --repo "$REPO" --watch --interval 60
   ```
   Then report the result. If a check failed, read its log before
   attempting any fix (see `golden-principles.md` §14 — never fix an
   unobserved failure):
   ```bash
   gh run view <RUN_ID> --repo "$REPO" --log-failed
   ```
   (`<RUN_ID>` is in the check's `link`.)

## Done when

- The pushed branch test-merges cleanly into a freshly fetched
  `origin/master` (step 1; after a conflict, the merge commit is
  pushed), **and**
- unresolved threads and change requests were handed off (or there
  were none), **and**
- no check is `pending`.

A required human review still outstanding is expected. Report it and
stop. Don't wait for it.
