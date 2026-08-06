---
phase: 08-watermonster-combatstate
plan: 01
subsystem: boss-ai
tags: [combatstate, pattern-selection, cooldown, state-machine]

# Dependency graph
requires:
  - phase: 07-boss-attack-pattern-judgment
    provides: CombatState 범용 패턴 후보 평가 헬퍼 (PatternCandidate, SelectWeightedPattern, ForceSelectPattern)
provides:
  - "PatternCandidate.CooldownOverride (strategy.Cooldown 과 분리된 재사용 잠금 시간)"
  - "CombatState.GetPatternCooldownMultiplier() virtual 훅 (광폭화 배율용)"
  - "SelectWeightedPattern 의 lastUsedWeightMultiplier 파라미터 (완전배제/가중치 감쇠 선택)"
affects: [08-02-watermonster-combatstate, 08-03-watermonster-combatstate]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "옵션 파라미터 기본값 = 기존 동작 유지 (하위 호환 확장)"

key-files:
  created: []
  modified:
    - Assets/Enemy/NewBoss/Script/States/CombatState.cs

key-decisions:
  - "별도 오버로드 대신 SelectWeightedPattern 에 기본값 있는 3번째 파라미터(lastUsedWeightMultiplier = 0f) 추가 — 기존 2-인자 호출부가 한 글자도 안 바뀜"
  - "룰렛 누적합을 candidate.Weight 대신 eligibleWeights 리스트(실효 가중치)로 병행 저장 — 감쇠가 룰렛 확률에 실제로 반영되도록"

patterns-established:
  - "PatternCandidate 확장 시 기본값은 항상 '기존 동작 그대로'가 되도록 설계 (CooldownOverride=null, lastUsedWeightMultiplier=0f, GetPatternCooldownMultiplier()=>1f)"

requirements-completed: [D-01d, D-03b, D-04a]

# Metrics
duration: 15min
completed: 2026-07-29
---

# Phase 08 Plan 01: CombatState 쿨다운 오버라이드 + 가중치 감쇠 확장 Summary

**`CombatState.cs`에 패턴별 쿨다운 오버라이드, 광폭화 배율 가상 훅, 직전 패턴 가중치 감쇠 모드를 추가해 WaterMonster D-03/D-04 요구사항의 재사용 기반을 마련했다 (SpiritCombatState.cs 변경 0 라인).**

## Performance

- **Duration:** 15 min
- **Started:** 2026-07-29T12:14:00Z (approx.)
- **Completed:** 2026-07-29T12:29:21Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- `PatternCandidate`에 `CooldownOverride` 필드 및 5번째 생성자 파라미터 추가 (`IAttackStrategy.Cooldown`과 분리된 재사용 잠금 시간)
- `protected virtual float GetPatternCooldownMultiplier() => 1f` 훅 추가 — 파생 클래스가 쿨다운에 배율(예: 광폭화 단축)을 적용 가능
- `CommitSelection`이 `cooldownOverride ?? strategy.Cooldown`에 배율을 곱해 `_patternReadyAt`에 기록하도록 교체
- `SelectWeightedPattern`에 `lastUsedWeightMultiplier`(기본 `0f`) 파라미터 추가 — `0f`는 기존 완전배제, `0f` 초과는 직전 패턴을 배제하지 않고 가중치만 감쇠
- 룰렛 누적합 계산을 실효 가중치(`eligibleWeights`) 기반으로 교체하여 감쇠가 실제 확률에 반영되도록 수정
- 스테일 주석(`WaterMonsterCombatState._lastWaveTime` 참조) 정리

## Task Commits

Each task was committed atomically:

1. **Task 1: PatternCandidate 쿨다운 오버라이드 + 광폭화 배율 가상 훅 추가** - `a7ea54d` (feat)
2. **Task 2: SelectWeightedPattern 에 직전 패턴 가중치 감쇠 모드 추가** - `9d00ba3` (feat)

_Note: No TDD tasks in this plan (pure C# structural extension, no test framework configured for Unity scripts in this repo)._

## Files Created/Modified
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` - PatternCandidate.CooldownOverride 필드, GetPatternCooldownMultiplier 가상 훅, CommitSelection 오버라이드/배율 반영, SelectWeightedPattern 가중치 감쇠 모드

## Decisions Made
- 별도 오버로드가 아닌 기본값 있는 파라미터 추가 방식을 택해 `SpiritCombatState.cs`의 기존 2-인자/4-인자 호출부를 전혀 건드리지 않음 (Phase 7 D-05a 완전배제 동작 그대로 유지)
- 감쇠 모드에서 룰렛 확률 계산 버그를 피하기 위해 실효 가중치를 별도 리스트로 병행 저장

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- **작업 환경 이슈 (계획 외):** 이 실행자 워크트리(`agent-af976905c6dfa294b`)가 `주창은` 브랜치보다 여러 커밋 뒤처져 있었고, Phase 08 계획 파일(`08-01/02/03-PLAN.md`, `08-CONTEXT.md`, `08-RESEARCH.md`, `08-DISCUSSION-LOG.md`)이 메인 워크트리에 커밋되지 않은 상태로만 존재했다. `HEAD`가 `주창은`의 조상(ancestor)임을 확인한 뒤 `git merge 주창은 --ff-only`로 안전하게 패스트포워드하고(충돌 없음, 로컬 `.claude/settings.local.json` 수정분만 폐기), 메인 워크트리에서 Phase 08 계획 문서를 파일 복사로 가져와 실행을 진행했다. 코드 변경에는 영향 없음 — 순수 환경 동기화 조치.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `CombatState.cs`의 확장점(쿨다운 오버라이드/배율 훅/가중치 감쇠) 준비 완료 — Plan 08-02(WaterMonsterCombatState)가 이를 바로 사용 가능
- Unity 컴파일 검증은 이 실행 환경에서 에디터를 열 수 없어 수행하지 못함 — Plan 08-03의 Play 모드 체크리스트 단계로 이월 (계획에 명시된 대로)
- `SpiritCombatState.cs` / `Stage2CombatState.cs` / `WaterMonsterCombatState.cs` 변경 0 라인 확인됨 (git diff --exit-code 통과)

---
*Phase: 08-watermonster-combatstate*
*Completed: 2026-07-29*

## Self-Check: PASSED

- FOUND: Assets/Enemy/NewBoss/Script/States/CombatState.cs
- FOUND: .planning/phases/08-watermonster-combatstate/08-01-SUMMARY.md
- FOUND commit: a7ea54d
- FOUND commit: 9d00ba3
