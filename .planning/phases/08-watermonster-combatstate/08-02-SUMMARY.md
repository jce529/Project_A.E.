---
phase: 08-watermonster-combatstate
plan: 02
subsystem: boss-ai
tags: [combatstate, pattern-selection, cooldown, weighted-random, state-machine]

# Dependency graph
requires:
  - phase: 08-watermonster-combatstate (Plan 01)
    provides: "CombatState.PatternCandidate.CooldownOverride, GetPatternCooldownMultiplier() virtual hook, SelectWeightedPattern lastUsedWeightMultiplier parameter"
provides:
  - "WaterMonsterCombatState as a thin PatternCandidate declaration layer (8 candidates) replacing manual pool-random"
  - "Assets/Enemy/WaterMonster/Check.md Play-mode verification checklist (input for Plan 08-03)"
affects: [08-03-watermonster-combatstate]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "PatternCandidate 목록을 SelectAttackStrategy 매 호출마다 재구성 (Enter() 캐싱 금지) — 세션 도중 상태(페이즈)가 바뀌는 보스의 표준 패턴"

key-files:
  created:
    - Assets/Enemy/WaterMonster/Check.md
  modified:
    - Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs

key-decisions:
  - "BuildCandidates() 를 Enter() 가 아니라 SelectAttackStrategy 매 호출마다 재구성 — WaterMonster 는 전투 도중 IsPhase2/IsPhase3 가 바뀌므로 SpiritCombatState 의 Enter()-1회-캐싱 패턴을 그대로 복사하면 페이즈 전환 후에도 이전 프리즌 패턴이 남는 회귀가 생긴다 (D-06c)"
  - "WaterWavePush 의 45초 특수 잠금은 PatternCandidate 의 cooldownOverride 로 전달 — strategy.Cooldown(3f) 에 의존하면 잠금이 조용히 3초로 줄어드는 회귀가 된다 (D-04a)"
  - "RESEARCH Pitfall 6 을 그대로 수용: 나머지 7개 패턴도 헬퍼 채택에 따라 자신의 Cooldown 값만큼 자동 자가 쿨다운을 갖게 됨 — Phase 7 SpiritCombatState 채택 때와 동일한 선례이며 스코프 크립 아님"

patterns-established:
  - "페이즈 의존 후보(BuildCandidates)는 Enter() 캐싱이 아니라 판단 시점마다 재구성 — 세션 도중 상태가 바뀌는 보스에 재사용 가능"

requirements-completed: [D-01a, D-01b, D-01c, D-02a, D-03a, D-04a, D-04b, D-05a, D-06a, D-06b, D-06c]

# Metrics
duration: 20min
completed: 2026-07-29
---

# Phase 08 Plan 02: WaterMonsterCombatState 후보 선언 데이터 레이어 전환 Summary

**`WaterMonsterCombatState`의 수작업 `List<IAttackStrategy>` + `Random.Range` 풀-랜덤을 8개 `PatternCandidate` 선언 + `CombatState.SelectWeightedPattern(dist, candidates, 0.5f)` 로 교체하고, WaterWavePush 45초 잠금·광폭화 0.5배 배율·페이즈별 프리즌 상호배타를 헬퍼 경로로 이전했다.**

## Performance

- **Duration:** 20 min (환경 동기화 포함)
- **Started:** 2026-07-29T12:54:31Z
- **Completed:** 2026-07-29T12:56:43Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- `WaterMonsterCombatState.SelectAttackStrategy`의 수작업 `pool` + `Random.Range(0, pool.Count)`를 `BuildCandidates()` (8개 `PatternCandidate`) + `SelectWeightedPattern(dist, candidates, LastUsedWeightDecay)`로 교체
- `WaterWavePush`는 `cooldownOverride: WaveAttackCooldown`(45f)로 선언해 기존 45초 특수 재사용 잠금을 `_patternReadyAt` 경로에서도 유지
- `GetPatternCooldownMultiplier()`를 override해 `_isEnraged`일 때 `_enrageCooldownMultiplier`(0.5f)를 반환 — 광폭화 중 모든 패턴의 개별 쿨다운(WavePush 포함, 45s→22.5s)이 함께 단축됨
- 근접(`dist <= 3.0`)/원거리(`dist >= 3.0`) 분기를 `maxDistance`/`minDistance` 조건부 후보로, 페이즈별 프리즌 변형 3종(`WaterPrisonAttack`/`WaterPrisonMapAoe`/`WaterColorPrison`) 상호배타를 `wmc.IsPhase2`/`IsPhase3` 실시간 체크로 재구성
- 장판 스폰(`CanSpawnZone`/`SpawnRandomZone`)과 텔레포트(`PuddleStackManager`+`CanTeleport`) 사전 가드는 `BuildCandidates` 호출보다 위의 early-return으로 원형 보존
- 고아 필드 `_lastWaveTime`과 수작업 45초 게이트/기록 로직 제거 (`_patternReadyAt`이 대체) — `WaveAttackCooldown` 상수 자체는 `cooldownOverride` 값으로 계속 사용되므로 보존
- `Assets/Enemy/WaterMonster/Check.md` 신규 작성 — 8종 패턴 표 + D-01~D-06 Play 모드 검증 항목 9개 (WaterSpirit 완전배제 회귀 항목 포함)

## Task Commits

Each task was committed atomically:

1. **Task 1: WaterMonsterCombatState 를 PatternCandidate 후보 선언 데이터 레이어로 교체** - `07fdb4b` (feat)
2. **Task 2: WaterMonster Play 모드 검증 체크리스트 작성** - `cb8fdf2` (docs)

_Note: No TDD tasks in this plan (pure C# structural refactor + markdown checklist, no automated test framework configured for Unity boss AI scripts in this repo)._

## Files Created/Modified
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` - 8개 `PatternCandidate` 선언 데이터 레이어로 교체, `GetPatternCooldownMultiplier()` override 추가, 수작업 pool-random/`_lastWaveTime` 제거
- `Assets/Enemy/WaterMonster/Check.md` (신규) - 8종 패턴 가중치/거리조건/재사용잠금 표 + D-01~D-06 Play 모드 검증 항목 9개

## Decisions Made
- `BuildCandidates()`를 `Enter()`가 아니라 `SelectAttackStrategy` 매 호출마다 재구성 — 마이그레이션 이전 코드도 매 호출마다 `pool`을 새로 만들었으므로 GC 할당 프로파일 동일, 페이즈 전환 시 즉시 반영 보장 (D-06c)
- RESEARCH Pitfall 6에서 예견된 대로, WaveAttackCooldown 외 7개 패턴도 이제 자신의 `IAttackStrategy.Cooldown` 값만큼 자가 쿨다운을 갖게 됨 — Phase 7 SpiritCombatState 선례와 동일한 헬퍼 채택의 자연스러운 결과이며 D-02a("균등 가중치 유지")가 요구하지 않는 자가 쿨다운 제거는 하지 않음

## Deviations from Plan

None - plan executed exactly as written (코드 내용은 계획에 명시된 전체 파일 교체 그대로).

## Issues Encountered
- **작업 환경 이슈 (계획 외, Plan 08-01과 동일한 근본 원인):** 이 워크트리(`agent-a45793365ce31ef76`)가 Plan 08-01이 완료·병합된 `주창은` 브랜치보다 여러 커밋 뒤처져 있었고, Phase 08 계획 문서(`08-01/02/03-PLAN.md`, `08-CONTEXT.md`, `08-RESEARCH.md`, `08-DISCUSSION-LOG.md`, `08-01-SUMMARY.md`)가 메인 워크트리에만 커밋되지 않은 상태로 존재했다. 조치: (1) 로컬 `.claude/settings.local.json` 수정분만 stash로 대피, (2) `git merge 주창은` 실행 — 순수 fast-forward로 충돌 없이 병합됨(`CombatState.cs`의 Plan 08-01 확장 포함), (3) stash pop 시 `settings.local.json`에서만 충돌 발생 → 두 버전의 합집합으로 수동 해결, (4) 메인 워크트리의 Phase 08 계획 문서를 읽어 동일 내용으로 이 워크트리에 재작성, (5) `ROADMAP.md`/`STATE.md`에 Phase 8 섹션·진행 상태를 반영해 `gsd-tools init` 이 Phase 08을 정상 인식하도록 동기화. 별도 커밋(`15b3f0b`)으로 분리 기록. 코드 변경에는 영향 없음 — 순수 환경 동기화 조치.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `WaterMonsterCombatState.cs`가 8개 `PatternCandidate`만 선언하는 데이터 레이어로 완성되어, Plan 08-03의 정적 회귀 검사(변경 파일 2개 확인, 사전 가드 위치, 45초 잠금 보존 등)를 그대로 통과할 준비가 되었다
- `Assets/Enemy/WaterMonster/Check.md`가 작성되어 Plan 08-03의 3종 보스 일괄 Play 모드 검증 체크포인트 입력으로 사용 가능
- Unity 컴파일 검증은 이 실행 환경에서 에디터를 열 수 없어 수행하지 못함 — Plan 08-03의 Play 모드 체크리스트 단계로 이월 (계획에 명시된 대로)
- `CombatState.cs` / `SpiritCombatState.cs` / `Stage2CombatState.cs` / `WaterMonsterController.cs` / `WaterTeleportState.cs` 변경 0 라인 확인됨 (`git diff --exit-code` 통과)

---
*Phase: 08-watermonster-combatstate*
*Completed: 2026-07-29*

## Self-Check: PASSED

- FOUND: Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs
- FOUND: Assets/Enemy/WaterMonster/Check.md
- FOUND commit: 07fdb4b
- FOUND commit: cb8fdf2
