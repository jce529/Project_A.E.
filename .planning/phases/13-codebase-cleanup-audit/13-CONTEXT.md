# Phase 13: 프로젝트 폴더를 돌면서 의미 없는 코드나, 주석, 리펙토링이 필요한 코드 살펴보는 페이즈 - Context

**Gathered:** 2026-08-19
**Status:** Ready for planning

<domain>
## Phase Boundary

`Assets/` 아래 전체 C# 스크립트를 대상으로, 죽은 코드/TODO 잔재/중복 로직/과도하게 복잡한 함수를
찾아 **보고서**로 정리한다. 실제 삭제/리팩토링은 이 phase의 1단계 산출물(보고서)이 아니라, 사용자가
항목별로 승인한 뒤에만 진행하는 2단계 작업이다.

이 phase는 새 기능을 추가하지 않는다 — 순수 조사/정리 phase다.

</domain>

<decisions>
## Implementation Decisions

### 산출물 형태 (Output Mode)
- **D-01:** 1단계로 문제 목록 보고서를 먼저 작성한다 (파일경로/줄번호/사유/카테고리 포함).
  2단계로 사용자가 항목별로 승인(삭제/수정 진행 vs 보류)한 것만 실제로 코드에 반영한다.
- **D-02:** CLAUDE.md 3번 원칙("기존에 있던 데드 코드는 언급만 하고 삭제하지 마세요")은 이
  phase의 목적 자체와 충돌하므로, 이 phase에 한해 **사용자 승인을 받은 항목에 한해** 예외로
  삭제/리팩토링을 허용한다. 승인 없이 임의로 삭제하지 않는다.

### 대상 범위 (Scope)
- **D-03:** `Assets/` 아래 전체 C# 스크립트를 스캔 대상으로 한다 (특정 폴더로 좁히지 않음).
- **D-04:** CP949 인코딩 파일(`Checkpoint.cs`, `WoodBossController.cs` 등 — Phase 9~12에서 이미
  식별된 인코딩 훼손 위험 파일)은 보고서에서 별도로 표시한다. 이 파일들은 수정 시 표준 Read/Edit
  툴 왕복이 비-ASCII 바이트를 훼손시킬 위험이 있으므로(과거 Phase 9~11에서 반복 발견된 패턴),
  실제 수정 단계에서 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜 적용이 필요하다.

### 안전 기준 (Safety / Risk Tiering)
- **D-05:** 이미 Play 모드로 검증된 보스 상태머신 코드(WaterMonsterController/Stats/CombatState,
  SpiritController/Stats/CombatState, TutorialBoss, WoodBoss 등)도 보고서 대상에 **포함**한다.
- **D-06:** 단, 이 파일들에서 발견된 항목은 보고서에 "회귀 위험 높음 — 신중 검토 필요" 태그를
  별도로 붙인다. 이런 항목은 사용자가 개별적으로 더 신중하게 판단할 수 있도록 다른 항목과
  구분해서 제시한다.

### 판단 기준 (What Counts as "Meaningless Code")
보고서는 아래 4개 카테고리로 분류한다:
- **D-07 — 죽은 코드**: 호출/참조가 프로젝트 전체에서 0건인 함수/클래스/필드, 주석 처리된 코드
  블록, 사용되지 않는 using/변수. STATE.md에 이미 "Portal.cs / GameManager.NextSpawnPointName /
  WoodBossStatSystem.cs 는 고아 코드" 로 명시된 선례가 있음 — 이런 기존에 알려진 항목도 보고서에
  재확인 차원으로 포함한다.
- **D-08 — TODO/FIXME 잔재 및 임시 디버그 코드**: 해결되지 않은 TODO/FIXME 주석, `Debug.Log`
  난무, 임시로 주석처리된 테스트 코드.
- **D-09 — 중복 로직 (DRY 위반)**: 여러 보스 스크립트에 반복되는 비슷한 패턴. 단, 이미
  `BossController`/`BossStatsSystem` 기반 클래스 수준 공유 원칙(PROJECT.md Key Architectural
  Decisions)과 충돌하지 않는 선에서만 — 즉 "왜 공유 안 됐는지" 이유가 있는 의도된 중복(예: Phase
  8/9/10/11에서 반복적으로 발견된 "다른 보스에 영향 주지 않기 위한 의도적 비공유")은 문제로
  보고하지 않는다.
- **D-10 — 과도하게 긴/복잡한 함수 (가독성)**: 주관적 판단이 많이 개입되므로 "권장" 수준으로만
  표시. 실제 수정은 사용자 승인 필수.

### Claude's Discretion
- 각 항목의 구체적 우선순위 정렬(예: 심각도/파일별 그룹핑 방식)은 Claude 재량.
- 보고서 형식(표/목록 등 세부 레이아웃)은 Claude 재량.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 규칙
- `.claude/CLAUDE.md` — 특히 3번 "정밀한 변경" 원칙 (기존 데드 코드는 언급만, 삭제 금지 — 이
  phase에서는 D-02에 따라 사용자 승인 항목에 한해 예외)
- `.planning/PROJECT.md` — Key Architectural Decisions (보스는 BossController/BossStatsSystem
  상속, 코드 공유는 기반 클래스 수준으로 제한 — 이 원칙과 충돌하지 않는 중복만 D-09로 보고)

### 인코딩 위험 (과거 Phase에서 반복 발견된 패턴)
- Phase 9 Plan 1 (`09-01-PLAN.md`), Phase 10 Plan 1 (`10-01-PLAN.md`), Phase 11 Plan 3
  (`11-03-PLAN.md`) — CP949 인코딩 파일 편집 시 표준 Read/Edit 툴이 비-ASCII 바이트를 훼손시킬
  수 있음. `.planning/STATE.md` Key Decisions 섹션에 상세 기록.

### 검증 상태 참고
- `.planning/VERIFICATION-STATUS.md` — 다른 phase들의 Play 모드 검증 진행 상태 (이 phase와는
  별개 트랙이지만, D-05/D-06의 "이미 검증된 보스 로직" 판단 근거)

No external specs beyond the above — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Known prior findings (already flagged, not yet acted on)
- `Assets/SaveSystem/Check.md` 107~108행: `Portal.cs` / `GameManager.NextSpawnPointName` /
  `WoodBossStatSystem.cs` 가 고아 코드로 이미 식별돼 있으나 Phase 11 범위에서 건드리지 않았음.
  이 phase의 보고서에 재확인 차원으로 포함.
- `Assets/SaveSystem/Check.md` 107행: `Assets/Scenes/InGame.unity` 가 Build Settings에 등록돼
  있으나 파일이 존재하지 않는 스테일 엔트리 — `MainMenuUI.OnClickStart()` 가 이 씬을 로드하려다
  실패함. 코드 문제라기보다 씬/빌드 설정 문제이지만 보고서에 언급.

### Established Patterns
- 보스는 `BossController`/`BossStatsSystem` 상속 + State 패턴(`IdleState`, `CombatState`,
  `CounterState`, `GroggyState` 등) — 이 구조 자체를 리팩토링 대상으로 삼지 않는다 (아키텍처
  변경은 이 phase 범위 밖).
- CP949 인코딩 파일이 다수 존재 — 정리 작업 시 인코딩 보존이 최우선 제약.

### Integration Points
- 이 phase는 별도 런타임 기능을 추가하지 않으므로 통합 지점이 없다. 산출물은 문서(보고서) +
  사용자 승인 후의 선택적 코드 수정.

</code_context>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches within the 4 categories (D-07~D-10).

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 13-codebase-cleanup-audit*
*Context gathered: 2026-08-19*
