# Phase 4: 광폭화 및 장판 시스템 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-16
**Phase:** 04-enrage-zone-system
**Areas discussed:** 광폭화 State 구조, 장판 생성 구조, HP 코스트 가속 방식

---

## 광폭화 State 구조

| Option | Description | Selected |
|--------|-------------|----------|
| WaterMonsterCombatState 내 플래그 | _isEnraged bool 플래그 추가. SelectAttackStrategy에서 쿨다운/가중치 변경. 코드 양 최소, Phase 3 핸드쉐이크 유지. | ✓ |
| 별도 EnrageCombatState 신규 | WaterMonsterCombatState를 상속하는 EnrageCombatState 신규 생성. 광폭화 시 Controller가 State 교체. | |

**User's choice:** WaterMonsterCombatState 내 플래그
**Notes:** 기존 State 구조를 유지하는 최소 변경 방식 선택.

---

## 광폭화 쿨다운 배율

| Option | Description | Selected |
|--------|-------------|----------|
| 0.5x (절반) | Phase shift가 명확하게 느껴지면서 포스 시프트가 명확히 느껴짐. 밸런싱 용이. | |
| 0.3x (매우 빠름) | 진짜 난스러운 마무리. 트위치하기 어날 수 있음. | |
| 인스펙터 튜닝 위임 | cooldownMultiplier 필드로 노출. Phase 1~3 쿨다운 위임 패턴과 동일. | ✓ |

**User's choice:** 인스펙터 튜닝 위임

---

## 광폭화 AI 패턴 전략

| Option | Description | Selected |
|--------|-------------|----------|
| 장판 생성을 SelectAttackStrategy 후보에 포함 | 쿨다운이 지나면 장판 생성을 텔레포트처럼 공격 후보에 추가. Phase 3 패턴과 동일한 구조, 코드 일관성 유지. | ✓ |
| 별도 코루틴 주기 생성 | Controller나 별도 Spawner에서 독립적으로 장판을 주기적으로 생성. 공격 패턴과 병렬로 진행. | |
| 풀리스트 선택 (5~6종 후보 풀) | 장판생성/텔레포트/근접/원거리를 다 후보로 넣고 랜덤 난사. 가장 복잡하지만 힘드스한 최종 페이즈. | |

**User's choice:** 장판 생성을 SelectAttackStrategy 후보에 포함

---

## 장판 컴포넌트 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 단일 ZoneEffect 컴포넌트 | SpeedMultiplier float 필드 하나로 선속(>1) / 감속(<1)을 통합. SpeedUpZone과 SlowDownZone은 프리팹 변형으로만 구분. | |
| 두 개 전용 코드 | SpeedUpZone.cs와 SlowDownZone.cs 따로 작성. 명시적이지만 코드 추가. | |
| 기존 Zone 오브젝트 Inspector 참조 | 다른 팀원이 이미 만든 Zone 오브젝트(프리팹)를 Inspector에서 직접 참조할 수 있게. 신규 Zone 스크립트 작성 불필요. | ✓ |

**User's choice:** 기존 Zone 오브젝트를 Inspector에서 참조
**Notes:** "이미 다른 사람이 만든 zone이 있으니까 인스펙터에서 원하는 오브젝트로 추가할 수 있게 만들어줘"

---

## 장판 생성 주체

| Option | Description | Selected |
|--------|-------------|----------|
| WaterMonsterController 직접 | Controller에 ZoneSpawnRadius 등 필드 추가 후 Instantiate. Phase 3의 텔레포트처럼 Controller가 상태를 직접 관리하는 패턴과 동일. | ✓ |
| 별도 ZoneSpawner 컴포넌트 | Phase 2 PuddleSpawner처럼 별도 컴포넌트. 분리가 명확하지만 컴포넌트 수 증가. | |

**User's choice:** WaterMonsterController 직접

---

## 장판 Object Pool

| Option | Description | Selected |
|--------|-------------|----------|
| 일정 시간 후 자동 비활성화 | 장판 지속 시간(Inspector 튜닝)이 지나면 자동으로 비활성화. 신규 장판이 생길 때 Instantiate/Destroy 없이 재활용. | ✓ |
| Instantiate / Destroy 방식 | 장판 생성 시 Instantiate, 후 Destroy. 지속가 사라지면 GC spike 우려 있지만 벗 수가 많지 않으면 문제 없음. | |

**User's choice:** 일정 시간 후 자동 비활성화

---

## HP 코스트 가속 방식

| Option | Description | Selected |
|--------|-------------|----------|
| 코스트 배율 증가 | enrageHpCostMultiplier 필드 추가. 공격 시이미 SpendHpCost(amount * multiplier)로 간단히 확장. 코드 변경 최소. | |
| 별도 주기적 tick 소모 | 광폭화 중 매 X초마다 HP를 tick으로 마모. 공격 코스트도 유지. 진장감 자체는 높지만 복잡도 증가. | ✓ |

**User's choice:** 별도 주기적 tick 소모
**Notes:** 보스가 공격을 아끼더라도 시간이 지나면 HP가 줄어 자연스러운 타임 리밋 역할.

---

## HP tick 소모 위치

| Option | Description | Selected |
|--------|-------------|----------|
| WaterMonsterStats.Update | WaterMonsterStats에 _isEnraged bool + enrageTickRate/Amount 필드 추가. Update에서 tick 안전하게 SpendHpCost 호출. Stats가 자신의 HP 소모를 자체 관리. | ✓ |
| WaterMonsterController 코루틴 | Controller에서 코루틴으로 tick 호출. 컨트롤 플로우가 명시적이지만 Stats vs Controller 역할 경계가 애매. | |

**User's choice:** WaterMonsterStats.Update

---

## 광폭화 진입 임계치 M%

| Option | Description | Selected |
|--------|-------------|----------|
| 30% | 일반적인 마무리 페이즈 진입 기준. Phase 2가 70%에서 시작되니 일관성 있음. | |
| 50% | 더 일시적으로 광폭화가 등장. Phase 2 구간이 짧아질 수 있음. | |
| 인스펙터 튜닝 위임 | _enrageHpThreshold 필드로 노출. Phase 2의 _phase2HpThreshold 패턴과 동일. | ✓ |

**User's choice:** 인스펙터 튜닝 위임

---

## Claude's Discretion

- 경고 광폭화 쿨타임 배율 기본값
- 장판 생성 위치 랜덤 범위 계산 방식
- 기존 Zone 프리팹 이속 적용 인터페이스 확인
- 장판 개수 상한선
- `SetEnraged` 호출 시 `WaterMonsterCombatState` 참조 방법

## Deferred Ideas

- 광폭화에서 텔레포트 후 즉시 AoE 발사 연계
- 폭발 강화 (n번째 폭발이 더 강함)
- SpeedUp/SlowDown Zone의 시각적 구분 강화
