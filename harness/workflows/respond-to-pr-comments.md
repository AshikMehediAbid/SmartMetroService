---
name: respond-to-pr-comments
kind: workflow
description: >-
  After reading PR review threads from GitHub, respond to each thread
  and resolve addressed ones, then post one summary comment on the PR.
  Should always be used after fetching PR
  review comments.
arguments: pull request number (optional; defaults to the PR for the current branch)
---

You have just read review threads from a GitHub PR. For each
**unresolved** thread, do one of the following:

1. **If the comment has already been addressed** (the issue was fixed
   in the current branch):
   - Reply to the thread explaining what was done ("Fixed — <brief
     description>.").
   - Then **resolve the thread** (see below).

2. **If the comment will NOT be addressed** (intentional design
   choice, not applicable, or deferred):
   - Reply explaining why: `"Won't fix — <reason>"` or
     `"Acknowledged — <reason and plan>."` (link the follow-up ticket
     when one was filed).
   - Then resolve the thread — the reply makes the disposition clear.

3. **If the comment has not yet been addressed but should be**:
   - **Obvious fix** (a typo, a stale assertion your own change broke,
     a missing using, an async call without `await`, a broken markdown
     link): have a `fast`-tier `quick-fix` agent make the fix (several
     in parallel when there are several fixes with disjoint files),
     have a `standard`-tier `security-reviewer` and then a `high`-tier
     `final-reviewer` check it (`harness/README.md`, *Model tiers and
     the delivery pipeline*), show the user the push and the reply,
     and once they approve, push, then reply and resolve as in 1.
   - **A real tradeoff** (an API-contract change, a security, ownership,
     payment or auth choice, performance against readability): flag it to the
     user as still open and ask how to proceed. Do not resolve the
     thread.

## Steps

`REPO` is `AshikMehediAbid/SmartMetroService` (`AGENTS.md` §3);
`<PR>` is the pull request number (see
[`check-pr-comments`](./check-pr-comments.md) for how to resolve both).

### Step 1: List unresolved threads

```bash
gh api graphql -f owner="${REPO%/*}" -f name="${REPO#*/}" -F number=<PR> -f query='
  query($owner:String!,$name:String!,$number:Int!){
    repository(owner:$owner,name:$name){ pullRequest(number:$number){
      reviewThreads(first:100){ nodes{
        id isResolved isOutdated path line
        comments(first:50){ nodes{ author{login} body createdAt } } } } } } }' \
  --jq '.data.repository.pullRequest.reviewThreads.nodes[] | select(.isResolved|not)'
```

Conversation comments and review summary bodies
(`gh pr view <PR> --repo "$REPO" --json comments,reviews`) are not
threads and cannot be resolved; answer those with one PR comment that
addresses each by quoting it.

### Step 2: Check each thread against the branch

For each thread, read the referenced file at the branch head and
determine whether the issue described has been addressed. An
`isOutdated` thread points at lines that changed; check the current
code, not the old line.

### Step 3: Reply to the thread

```bash
gh api graphql -f thread=<THREAD_ID> -f body="Fixed — <what was done>." -f query='
  mutation($thread:ID!,$body:String!){
    addPullRequestReviewThreadReply(input:{pullRequestReviewThreadId:$thread, body:$body}){
      comment{ url } } }' --jq '.data.addPullRequestReviewThreadReply.comment.url'
```

### Step 4: Resolve the thread

```bash
gh api graphql -f thread=<THREAD_ID> -f query='
  mutation($thread:ID!){ resolveReviewThread(input:{threadId:$thread}){ thread{ isResolved } } }' \
  --jq '.data.resolveReviewThread.thread.isResolved'
```

Resolve addressed and explicitly declined threads. Leave threads that
are still open unresolved.

For a top-level comment or review summary (not a thread):

```bash
gh pr comment <PR> --repo "$REPO" --body-file <reply.md>
```

### Step 5: Post one PR summary comment

GitHub collapses resolved threads, so the replies alone are invisible to
a reviewer opening the PR. After the replies and resolves, post one
PR-level comment that says what happened. Draft it together with the
thread replies in the approval request of step 3, so one approval covers
both, and post it last:

```markdown
**Review follow-up**

| Comment | Disposition | Where |
|---|---|---|
| <one-line summary, linked to the thread> | Fixed / Won't fix / Acknowledged / Still open | <commit sha or follow-up issue #> |

**Checks:** <gate result and CI status at the pushed head>
**Next:** <what the reviewer should do, e.g. "re-review", "decide on the open item">
```

```bash
gh pr comment <PR> --repo "$REPO" --body-file <summary.md>
```

### Step 6: Summarise

Present the same table to the user, with each thread's disposition
(fixed / won't fix / acknowledged / still open) and the summary
comment's URL.

## Important

- Every push, reply and resolve is a GitHub write: show each command
  to the user and get approval before it runs (the repo's
  `AGENTS.md` §0).
- Reply once per thread — don't fork side-discussions.
- Keep replies concise (1-2 sentences).
- When a comment suggests a code change and the fix used a different
  approach, mention that in the reply.
- A reviewer's `CHANGES_REQUESTED` verdict is cleared by that reviewer
  re-reviewing, not by resolving threads; tell the user to re-request
  their review once the threads are answered.
