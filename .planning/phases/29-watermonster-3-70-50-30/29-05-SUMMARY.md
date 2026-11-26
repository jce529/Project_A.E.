---
phase: 29-watermonster-3-70-50-30
plan: 05
subsystem: WaterMonster combat selection
requirements-completed: [D-01, D-02, D-03, D-04]
files:
  modified:
    - Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs
deviation: "없음(계획 그대로). 실행자 Codex 사용 한도로 Claude 폴백."
verify:
  task1: T1_OK
  task2: T2_OK
  combatstate_family_unchanged: "git diff --exit-code (CombatState.cs, SpiritCombatState.cs, Stage2CombatState.cs) = 0"
unity_compile:
  refresh: AssetDatabase.Refresh
  isCompiling: false
  scriptCompilationFailed: false
  note: "Assembly-CSharp.dll 이 편집 시각 이후 재생성됨. Editor.log 의 error CS 3건은 이전(plan 04 도중) 컴파일 기록으로 최신 컴파일 이전 라인."
---

# 29-05 SUMMARY — WaterMonsterCombatState 페이즈 구조 연결

## 변경 내용
- **Task 1 (D-02, D-04):** 장판 스폰 early-return 과 `PuddleStackManager` 기반 구 텔레포트 가드 제거. `SelectAttackStrategy` 앞에 `HasPendingPhaseTransition` → null, 3페이즈 `ActivePatternCount >= Phase3MaxConcurrentPatterns` → null 가드 추가. `BuildCandidates` 는 매 판단 재구성하며 누적 구성:
  - 공통: `WaterGeyser`, `WaterBurst`, `WaterPrisonAttack`, `WaterJumpLand`(maxDistance: MeleeRange)
  - `CurrentPhase >= 2`: `WaterPrisonMapAoe`, 웅덩이 시스템 활성 시 `WaterPuddleSpawn`(CanSpawnMore) / `WaterPuddleExplode`(숙성 웅덩이 있음, 폭발 중 아님) / `WaterPuddleMove`(CanTeleport && TrySelectMove)
  - `CurrentPhase >= 3`: `WaterColorPrison`, `WaterWavePush`(cooldownOverride: WaveAttackCooldown 45f)
  - `IsPatternRunning` 이면(3페이즈 겹침) `WaterJumpLand`, `WaterPuddleMove` 제외(OD-7)
  - `WaterMeleeSwipe`, `WaterRangedSpit` 후보 제거(파일 유지). `new PatternCandidate(` 10개.
- **Task 2 (D-01, D-03):** `_enrageCooldownMultiplier`/`_isEnraged`/`SetEnraged` 제거, Enter 에서 `_wmc` 캐시. `AcquireHistory` override 로 `wmc.PatternHistory` 반환(WavePush 45초 잠금·직전 감쇠가 재진입 사이 유지). `GetPatternCooldownMultiplier` 는 `_wmc.PatternCooldownMultiplier`. `Execute` 는 페이즈 전환 대기 시 return, `CurrentPhase < 3 && IsPatternRunning` 이면 return(단일 사용), 이후 base.Execute.

## `_selectedThisFrame` 처리 방식
베이스 `CombatState.Execute` 는 `SelectAttackStrategy` 가 non-null 을 반환한 직후 같은 프레임 안에서 `_decisionTimer = attack.Cooldown` 을 대입하고 반환한다. 그래서 override `Execute` 에서 `base.Execute` 호출 전에 `_selectedThisFrame = false` 로 초기화하고, `SelectAttackStrategy` override 가 non-null 을 반환하는 유일한 경로(SelectWeightedPattern 성공)에서 `_selectedThisFrame = true` 를 세운다. `base.Execute` 가 돌아온 뒤 플래그가 참일 때만 `_decisionTimer *= GetPatternCooldownMultiplier();` 를 1회 적용하므로, 곱셈 대상은 항상 방금 대입된 `attack.Cooldown` 이고 대기·공격 종료 대기·거리 이탈(Chase/Idle 전환) 프레임에는 곱하지 않는다. 이로써 옛 코드의 매 프레임 x0.5 수렴 버그가 사라진다. 패턴별 쿨다운(`ReadyAt`)에는 베이스 `CommitSelection` 이 같은 배율을 이미 곱한다.

## 검증
- Task 1 automated: `T1_OK`
- Task 2 automated: `T2_OK`
- Unity: `compiling=false, failed=false`
- CombatState / SpiritCombatState / Stage2CombatState 무변경
- 파일 인코딩 UTF-8(BOM 없음), CRLF 유지
