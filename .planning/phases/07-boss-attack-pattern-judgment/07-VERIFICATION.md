---
phase: 07-boss-attack-pattern-judgment
verified: 2026-10-26T00:00:00Z
status: human_needed
score: 0/6 runtime checks verified (code complete, Play-mode checks not run)
human_verification:
  - test: "D-03 랜덤성"
    expected: "30초 이상 교전 시 패턴 순서가 고정 라운드로빈이 아니며, 재시작 후 두 번째 시퀀스가 첫 번째와 다르다"
  - test: "D-05 연속 사용 금지"
    expected: "`패턴 선택:` 로그에서 같은 패턴명이 연속 두 줄로 나오지 않는다"
  - test: "D-04 강제 체인"
    expected: "`패턴 선택: SpiritExhaustion` 직후 `체인 발동: SpiritExhaustion → SpiritWakeRepel (강제)` → `새로운 공격 시작: SpiritWakeRepel`, 이후 일반 `패턴 선택:` 복귀"
  - test: "후보 고갈 스팸 없음"
    expected: "`사용 가능한 패턴 후보 없음` 로그가 프레임마다 쏟아지지 않고 보스가 몇 초간 멈추지 않는다"
  - test: "D-07 Stage 2 사이클"
    expected: "HP 50% 이하 → `일반 패턴 실행 #1/3~#3/3` → 헤비콤보 → 분신 삭제 + 그로기, 분신 2 + 진짜 보스 1 = 총 3개 동시 존재"
  - test: "D-08b 물괴물 보스 회귀 없음"
    expected: "Phase 8 완료 후 `WaterMonsterCombatState`가 변경 전과 동일하게 동작한다"
---

# Phase 7: 보스 공격 패턴 판단 로직 리팩토링 Verification Report

**Phase Goal:** 정령 보스가 고정 순서 라운드로빈이 아니라 거리(선택)·쿨다운·연속사용금지 조건을 통과한 후보 중 가중치 랜덤으로 패턴을 고르고, SpiritExhaustion 직후에는 반드시 SpiritWakeRepel 이 이어진다. 판단 로직은 CombatState 범용 헬퍼로 구현한다.
**Status:** human_needed
**Re-verification:** No. 코드는 07-01 에서 완료됐지만, 남은 플랜 07-02 는 Unity Play 모드에서 사람이 직접 확인하는 체크포인트(`autonomous: false`)라서 에이전트 세션에서는 실행할 수 없다. 이 문서의 어떤 항목도 PASS 로 추정해 적지 않았다.

## Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | 같은 패턴을 연속으로 실행하지 않는다 (D-04 강제 체인 제외) | NOT VERIFIED | Play 모드 로그 필요 (D-05) |
| 2 | SpiritExhaustion 직후 항상 SpiritWakeRepel 이 실행된다 | NOT VERIFIED | Play 모드 로그 필요 (D-04) |
| 3 | 패턴 순서가 매 전투마다 동일하지 않다 | NOT VERIFIED | Play 모드 로그 필요 (D-03) |
| 4 | Stage 2 사이클이 기존과 동일하게 동작한다 | NOT VERIFIED | Play 모드 필요 (D-07) |
| 5 | 후보 고갈 로그가 스팸되지 않는다 | NOT VERIFIED | Play 모드 필요 |
| 6 | 물괴물 보스 동작에 회귀가 없다 | NOT VERIFIED | Phase 8 마이그레이션 이후에 확인 (D-08b) |

**Score:** 0/6. 코드 구현은 07-01-SUMMARY.md 에 기록되어 있다.

## Why It Cannot Be Run By An Agent

- 정적 grep 으로는 랜덤 선택·강제 체인·Stage 2 사이클을 증명할 수 없다. 이 프로젝트에는 Unity Test Framework 가 없고 `.planning/config.json` 의 `nyquist_validation` 이 false 이다.
- 에이전트 worktree 는 에디터가 열어 둔 프로젝트가 아니어서 Play 모드를 돌리거나 Console 로그를 볼 수 없다.

## Human Verification Required

체크리스트 원본은 `Assets/Enemy/WaterSpirit/Check.md` (8개 항목, 전부 미체크). 절차는 `07-02-PLAN.md` Task 1 의 how-to-verify 를 따른다.

1. 사전 준비: Unity 컴파일 에러 0건, 물의 정령 보스 씬 열기, Console Collapse 끄기, 검색창에 `SpiritCombatState` 입력.
2. 위 6개 항목을 수행하고 `패턴 선택` / `체인 발동` 로그 원문(최소 8줄)을 붙여넣는다.
3. 결과를 `07-02-SUMMARY.md` 에 `D-03 / D-05 / D-04 / D-07: PASS|FAIL` 로 기록한다. 보고되지 않은 항목은 `NOT VERIFIED` 로 둔다.
4. FAIL 이 있으면 `/gsd:plan-phase 7 --gaps` 로 갭 클로저 계획을 만든다.

**진행 시점:** 사용자 결정에 따라 Phase 8(WaterMonster 마이그레이션) 완료 후 WaterSpirit / TutorialBoss / WaterMonster 3종을 한 번에 일괄 검증한다. 요청 전에는 진행하지 않는다.

## Related Checklists

| 보스 | 체크리스트 | 상태 |
|---|---|---|
| WaterSpirit | `Assets/Enemy/WaterSpirit/Check.md` | 전부 미체크 (보류) |
| TutorialBoss | `Assets/Enemy/Tutorial/TutorialBoss/Check.md` | 2026-09-29 실측 완료, 전 항목 PASS (쿨다운은 간접 확인). 이 파일은 현재 uncommitted 변경 상태 |
| WaterMonster | `.planning/phases/08-watermonster-combatstate/Check.md` | Phase 8 에서 진행 |

## Gaps Summary

코드 결함은 발견되지 않았다. 남은 것은 Play 모드 확인뿐이며 07-02 가 닫힐 때까지 Phase 7 은 부분 완료(plan 2개 중 summary 1개) 상태로 남는다.
