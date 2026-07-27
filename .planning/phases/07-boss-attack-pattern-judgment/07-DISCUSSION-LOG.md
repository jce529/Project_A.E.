# Phase 7: 보스 공격 패턴 판단 로직 리팩토링 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-07-27
**Phase:** 07-boss-attack-pattern-judgment
**Areas discussed:** 판단 로직 아키텍처, 순간이동 패턴 조건, 중복 슬롯, Stage2 연동, 적용 범위, 패턴 체인, 연속금지, 우선순위

---

## 판단 로직 아키텍처

| Option | Description | Selected |
|--------|-------------|----------|
| 범용 베이스 헬퍼 | CombatState에 재사용 가능한 우선순위 기반 조건 평가기 추가 | ✓ |
| 보스 전용 개별 구현 (TutorialBoss 방식) | SpiritCombatState/Stage2CombatState에 직접 CanUseX() 작성 | |

**User's choice:** 범용 베이스 헬퍼
**Notes:** 향후 다른 보스도 재사용 가능하도록 일반적으로 설계.

---

## 순간이동 패턴 발동 조건

| Option | Description | Selected |
|--------|-------------|----------|
| 쿨다운 + 연속금지만 적용 | Charge/FarProjectile은 거리 조건 없이 쿨다운+연속금지만으로 판단 | ✓ |
| 거리 조건 형식상 유지 | 순간이동해도 구조적 대칭을 위해 거리 임계치 유지 | |

**User's choice:** "기본적으로 쿨다운과 연속금지만 적용하고 보스에 따라서 별도로 적용"
**Notes:** 헬퍼는 패턴별로 거리 조건을 선택적으로 걸 수 있도록 설계해야 함 (보스마다 다르게 적용 가능).

---

## 중복 슬롯 (0번/4번 SpiritCharge)

| Option | Description | Selected |
|--------|-------------|----------|
| 하나의 패턴으로 통합 | 새 판단 로직에서는 SpiritCharge 하나만 후보로 등록 | |
| 그대로 두고 언급만 | 이번 페이즈에서 새 변종을 만들지 않음, 이슈로만 기록 | ✓ |

**User's choice:** "일단 그대로 두고 언급만"
**Notes:** 컨텍스트에 이슈로만 기록, 향후 별도 요청 시 새 변종 패턴 고려 가능.

---

## Stage2 헤비콤보 카운터 연동

| Option | Description | Selected |
|--------|-------------|----------|
| 카운터 로직 그대로 유지 | SelectAttackStrategy의 null 반환 계약이 동일하므로 호환 | ✓ |
| 카운터 기준 재설계 | 판단 빈도가 달라질 수 있으므로 임계값 재튜닝 | |

**User's choice:** 카운터 로직 그대로 유지
**Notes:** _patternsExecuted는 반환값 null 여부에만 의존하므로 헬퍼 교체와 독립적.

---

## 적용 범위 (WaterMonster 포함 여부)

| Option | Description | Selected |
|--------|-------------|----------|
| WaterSpirit만 적용 | 이번 페이즈는 WaterMonsterCombatState를 건드리지 않음 | ✓ (조건부) |
| WaterMonster도 함께 전환 | 일관성을 위해 이번 페이즈에서 함께 마이그레이션 | |

**User's choice:** "지금은 WaterSpirit만 적용하고 다음 페이즈에서 WaterMonster도 적용"
**Notes:** WaterMonster 마이그레이션은 Deferred Ideas로 기록, 향후 페이즈 후보.

---

## 패턴 체인 (Exhaustion → WakeRepel)

| Option | Description | Selected |
|--------|-------------|----------|
| 독립적으로 유지 | 둘 다 각자의 쿨다운/연속금지 조건만으로 독립 판단 | |
| Exhaustion 직후에만 WakeRepel 허용 | 이름이 암시하는 "취약해졌다가 급습" 관계를 실제 로직으로 구현 | ✓ (변형) |

**User's choice:** "일단 Exhausiton 직후는 무조건 WakeRepel을 사용하고 이후 일반패턴에 다시 넣기"
**Notes:** 제시된 두 옵션의 중간 — Exhaustion 직후에는 강제 실행(옵션 B의 강화형), 그 다음부터는 다시 독립 후보로 복귀(옵션 A와 동일하게 취급). 체인 강제 실행은 연속금지 규칙보다 우선.

---

## 연속 사용 금지 규칙

| Option | Description | Selected |
|--------|-------------|----------|
| 모든 패턴에 동일 적용 | TutorialBoss처럼 직전 패턴 재사용 금지를 5개 패턴 전체에 적용 | ✓ |
| 패턴별 재량 | 특정 패턴만 연속 허용/금지 개별 지정 | |

**User's choice:** 모든 패턴에 동일 적용

---

## 우선순위 결정 방식

| Option | Description | Selected |
|--------|-------------|----------|
| 근접 우선 고정 리스트 (Phase5 의도 복원) | 근접→중거리→원거리 순 고정 우선순위 | |
| 가중치 기반 랜덤 | 조건 만족 후보 중 가중치 랜덤 선택 | ✓ |

**User's choice:** 가중치 기반 랜덤
**Notes:** TutorialBoss의 고정 우선순위 대신, WaterMonsterCombatState의 풀 기반 랜덤 선택과 더 유사한 방식 채택.

---

## Claude's Discretion

- 범용 헬퍼의 정확한 API/클래스 설계
- 각 패턴의 쿨다운/가중치 구체 수치
- 근접/원거리 패턴의 거리 임계값 재사용 여부

## Deferred Ideas

- WaterMonsterCombatState를 동일 헬퍼로 마이그레이션 (다음 페이즈 후보)
- 중복 슬롯(0/4번)을 실제 구분되는 새 변종 패턴으로 발전시키는 것
