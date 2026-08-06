# Phase 8: WaterMonster 보스를 CombatState 기반 패턴 판단 로직으로 마이그레이션 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-07-27
**Phase:** 08-watermonster-combatstate
**Areas discussed:** 연속 사용 금지 도입 여부, 가중치 감쇠 방식, 감쇠 적용 범위, 광폭화가 개별 쿨다운에 적용될지, WaterWavePush 45초 쿨다운 유지 여부

---

## 연속 사용 금지 도입 여부

| Option | Description | Selected |
|--------|-------------|----------|
| 도입한다 | WaterSpirit과 동일하게 직전에 쓴 패턴은 다음 판단에서 완전 제외 | |
| 도입하지 않는다 (현상 유지) | 기존처럼 같은 패턴이 연속으로 나올 수 있다 | |
| (사용자 자유 응답) | 가중치를 통해서 패턴을 다룬다면, 최근에 사용한 패턴일수록 가중치를 줄이는 방식으로 패턴을 다양하게 하고 싶다 | ✓ |

**User's choice:** 완전배제도 현상유지도 아닌 제3의 방식 — 가중치 감쇠
**Notes:** 이 응답으로 "연속 사용 금지"와 "가중치 조정" 두 논의 영역이 하나의 메커니즘으로 합쳐짐.

---

## 가중치 감쇠 방식

| Option | Description | Selected |
|--------|-------------|----------|
| 직전 1개만 감쇠 | 바로 직전에 쓴 패턴만 가중치를 낮추고(예: 절반), 그 이전 패턴들은 원래 가중치로 복귀 | ✓ |
| 최근 N개 누적 감쇠 | 최근 여러 패턴을 기억해 쓴 횟수가 많을수록 가중치가 지수적으로 더 낮아짐 | |

**User's choice:** 직전 1개만 감쇠
**Notes:** 구현 단순성과 예측 가능성을 이유로 추천 옵션 선택.

---

## 감쇠 비율

| Option | Description | Selected |
|--------|-------------|----------|
| 절반으로 (0.5배) | 직전 패턴 가중치를 0.5배로 감쇠 | ✓ |
| 직접 수치 지정 | 사용자가 다른 비율을 지정 | |

**User's choice:** 절반(0.5배)

---

## 감쇠 메커니즘 적용 범위

| Option | Description | Selected |
|--------|-------------|----------|
| CombatState 공통 옵션으로 | Phase 7의 완전배제(D-05)와 나란히 헬퍼에 옵션으로 추가. WaterSpirit은 기존 방식 유지, WaterMonster만 새 방식 사용 | ✓ |
| WaterMonster 전용 구현 | CombatState를 건드리지 않고 WaterMonsterCombatState 내부에만 구현 | |

**User's choice:** CombatState 공통 옵션으로 추가 (향후 다른 보스도 선택 가능하도록)

---

## 광폭화(Enrage)가 개별 패턴 쿨다운에도 적용될지

| Option | Description | Selected |
|--------|-------------|----------|
| 네, 패턴별 쿨다운도 단축 | 광폭화 배율(0.5배)을 전체 판단 대기시간뿐 아니라 개별 패턴 쿨다운(_patternReadyAt)에도 동일 적용 | ✓ |
| 아니오, 판단 대기시간만 유지 | 기존 동작(_decisionTimer만 단축) 그대로, 개별 쿨다운은 광폭화와 무관 | |

**User's choice:** 패턴별 쿨다운도 단축

---

## WaterWavePush 45초 쿨다운 유지 여부

| Option | Description | Selected |
|--------|-------------|----------|
| 그대로 45초 유지 | 밸런스를 바꾸지 않고 특수 쿨다운 값을 새 메커니즘으로 그대로 이전 | ✓ |
| 이번 기회에 값 재검토 | 다른 값으로 조정 | |

**User's choice:** 그대로 45초 유지 (단, 광폭화 중에는 D-03에 따라 0.5배로 함께 단축됨)

---

## Claude's Discretion

- 가중치 감쇠 메커니즘의 정확한 API 형태
- 페이즈 변화에 따른 후보 목록 재구성 시점/방식 (D-06c)
- 광폭화 배율을 개별 쿨다운에 적용하는 정확한 구현 지점
- 나머지 패턴들의 정확한 가중치 수치 (균등 유지 원칙 하에서)

## Deferred Ideas

None — discussion stayed within phase scope.
