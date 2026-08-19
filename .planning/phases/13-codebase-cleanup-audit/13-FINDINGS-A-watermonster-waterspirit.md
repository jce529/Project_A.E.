# Phase 13 Audit — Findings A: WaterMonster / WaterSpirit

**Scope:** `Assets/Enemy/WaterMonster/**/*.cs`, `Assets/Enemy/WaterSpirit/**/*.cs`
**Files scanned:** 38
**Risk tier:** 전 범위 회귀 위험 높음 (D-05/D-06 — Play 모드 검증된 보스 상태머신)
**Generated:** 2026-08-19

## D-07 — 죽은 코드

### D-07 일반 항목

(없음)

### D-07 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| A-D07-01 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 8 | `ChargeRange` | 문서화된 의도적 유지 (STATE.md Key Decision) — 삭제 권장 아님 |
| A-D07-02 | Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs | 12 | `_unusedField1` | 프로젝트 전체 참조 0건, 씬/프리팹 참조 없음 |
| A-D07-03 | Assets/Enemy/WaterMonster/Script/Phase2/PuddlePool.cs | 45 | `// _pool.Clear();` | 주석 처리된 코드 블록 |
| A-D07-04 | Assets/Enemy/WaterMonster/Script/Phase2/PuddleSpawner.cs | 15 | `_unusedSpawnRate` | 프로젝트 전체 참조 0건, 씬/프리팹 참조 없음 |
| A-D07-05 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 120 | `using System.Collections.Generic;` | 사용되지 않는 using 구문 |
| A-D07-06 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 88 | `_tempVar` | 프로젝트 전체 참조 0건, 씬/프리팹 참조 없음 |

## D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

### D-08 일반 항목

(없음)

### D-08 회귀 위험 높음 — 신중 검토 필요

(없음)

## D-10 — 과도하게 긴/복잡한 함수 (권장)

### D-10 일반 항목

(없음)

### D-10 회귀 위험 높음 — 신중 검토 필요

(없음)

## D-09 후보 관찰 (raw — Plan 05에서 교차 검증)

(없음)

## 스캔 커버리지

(없음)
