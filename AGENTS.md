# Project A.E Codex Working Rules

## Phase bug tracking

Every bug discovered while discussing, implementing, reviewing, or live-testing a phase must be stored under that phase:

```text
.planning/phases/<phase-number>-<phase-name>/bugs/
```

Create the `bugs/` directory when the first bug for that phase is confirmed. Do not create empty bug directories because Git does not track them.

### Required files

- `bugs/README.md`: phase-local bug index and status summary.
- `bugs/BUG-<global-id>-<slug>.md`: one document per confirmed bug.

Keep bug IDs globally unique across the repository. Before assigning an ID, search both the legacy root `bug/` directory and every `.planning/phases/*/bugs/` directory.

### Bug document requirements

Record at least:

- severity, status, discovery date, discovery route, and affected scope;
- observable symptom and reproducible steps;
- expected and actual results;
- concrete evidence, including relevant paths and logs;
- root cause when known;
- proposed fix direction and completion criteria;
- related phase artifacts such as CONTEXT, PLAN, SUMMARY, UAT, or VERIFICATION;
- resolution date, fix commit, and verification evidence after the fix.

Use `확인됨 — 미해결`, `수정 중`, `수정됨 — 재검증 필요`, or `해결됨` as the normal status progression. Do not mark a bug resolved until its completion criteria have been verified.

### Workflow

1. On discovery, create or update the phase-local bug document immediately.
2. Add or update its row in the phase-local `bugs/README.md`.
3. Link the bug from the phase UAT or verification artifact that exposed it.
4. When fixing it, update the same bug document instead of creating a separate resolution note.
5. Record the exact fix commit and post-fix verification result.

The root `bug/` directory is a legacy location. Preserve existing documents there unless a migration task explicitly assigns them to phases. New phase-related bugs must use the phase-local structure.
