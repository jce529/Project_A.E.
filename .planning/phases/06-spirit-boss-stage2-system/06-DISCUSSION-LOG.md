# Phase 6: 스테이지 전환 및 스테이지 2 은신·분신 시스템 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-30
**Phase:** 06-spirit-boss-stage2-system
**Areas discussed:** 스테이지 2 진입 구조, 은신 텔레포트 목적지, 분신 소멸 조건, 스테이지 2 패턴 선택 로직

---

## 스테이지 2 진입 구조

### HP 50% 체크 위치

| Option | Description | Selected |
|--------|-------------|----------|
| SpiritStats.TakeDamage 내부에서 | TakeDamage override에서 _currentHealth <= MaxHealth * 0.5f 관단 후 SpiritController.OnStage2Trigger() 콜백. 데미지 파이프라인에 직접 연결되어 신뢰성 높음. | ✓ |
| OnDamageTaken 이벤트 구독 (SpiritController에서) | SpiritController.Start()에서 Stats.OnDamageTaken += 구독, 공법에서 HP% 체크. BossStatsSystem 패턴과 일치. | |

**User's choice:** SpiritStats.TakeDamage 내부에서 (추천)

### 스테이지 2 상태 관리

| Option | Description | Selected |
|--------|-------------|----------|
| Stage2CombatState 신규 클래스 | SpiritCombatState를 상속한 Stage2CombatState 신규 작성. SelectAttackStrategy에 은신/분신 패턴 추가. HP 50% 도달 시 ChangeState(new Stage2CombatState())로 교체. 스테이지 1 로직과 완전 분리. | ✓ |
| SpiritCombatState 내 _isStage2 플래그 | 동일 클래스에 _isStage2 bool 추가, SelectAttackStrategy에 if(_isStage2) 분기 포함. 클래스가 단순하지만 S1/S2 로직이 합쳐 복잡해짐. | |

**User's choice:** Stage2CombatState 신규 클래스 (추천)

---

## 은신 텔레포트 목적지

### 텔레포트 위치

| Option | Description | Selected |
|--------|-------------|----------|
| 랜덤 위치 | 아레나 내 랜덤 좌표. SpiritController에 TeleportBounds Rect/Vector2 범위 [SerializeField] 노출. | |
| 고정 웨이포인트 배열 | SpiritController에 Transform[] TeleportPoints [SerializeField] 노출, 현위치 제외 랜덤 선택. | |
| 플레이어 반대편 | 플레이어 위치에서 X 방향 반대로 고정 거리만큼 이동. | |
| 플레이어 중심 고리형 범위 (커스텀) | 플레이어를 중심으로 원형 고리형 구간을 지정, 해당 고리의 랜덤한 위치로 텔레포트 | ✓ |

**User's choice:** 플레이어를 기준으로 원형의 고리형 구간을 지정. 해당 고리의 랜덤한 위치로 텔레포트 (사용자 직접 입력)

### 고리 범위 Inspector 노출 방식

| Option | Description | Selected |
|--------|-------------|----------|
| MinTeleportRadius / MaxTeleportRadius | 예: Min=4f, Max=8f. Random.insideUnitCircle 샘플링으로 [Min, Max] 범위 내 좌표 생성. Inspector에서 쉽게 조정 가능. | ✓ |
| TeleportRingCenter / TeleportRingRadius 단일 반경 | 플레이어가 아닌 고정 중심 기준으로 단일 반경 원주 입력. | |

**User's choice:** MinTeleportRadius / MaxTeleportRadius (추천)

### 은신 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 대기 후 텔레포트 | 콜라이더 off → StealthDuration 초 대기 → 다른 위치로 텔레포트 → 콜라이더 on 재등장. 요구사항 S2-02에 정확히 대응. | ✓ |
| 즉시 텔레포트 | 콜라이더 off → 1프레임 이내 텔레포트 → 콜라이더 on. 은신 대기 없이 순간이동 자체가 은신 역할. | |

**User's choice:** 대기 후 텔레포트 (추천)

---

## 분신 소멸 조건

### 분신 소멸 시점

| Option | Description | Selected |
|--------|-------------|----------|
| 보스 사망 시 | 진짜 보스 HP 0 시 SpiritStats.Die()에서 전체 분신 Destroy. 전투 전내 내내 활성화. | |
| 고정 시간 후 자동 소멸 | CloneDuration [SerializeField] 후 Destroy. 유효 시간 연출 효과. | |
| 다음 분신 패턴 발동 시 교체 | 신규 패턴 시 기존 분신 Destroy 후 새로 스폰. 항상 3개 유지. | |
| 사이클 기반 (커스텀) | 분신소환 → 은신 텔레포트 → 돌진 → 그로기 전환 시 분신 삭제 → 반복 | ✓ |

**User's choice:** "분신소환 은신 텔레포트 돌진 후 그로기하면서 분신 삭제 이후 다시 반복" (사용자 직접 입력)

**Notes:** 사이클 구조 확인:
1. 분신 생성 (S2 진입 또는 그로기 해제 시)
2. S1 거리기반 패턴 최대 2회
3. 전체 동시 은신+돌진 (진짜+분신 모두)
4. 그로기 전환 + 분신 Destroy
5. 그로기 해제 → 1번 반복

### 분신 스폰 방식

| Option | Description | Selected |
|--------|-------------|----------|
| Instantiate (Prefab) | SpiritController에 DummyPrefab [SerializeField] 노출, Instantiate로 생성. Unity 표준 방식. | ✓ |
| Object Pool | 순환 재사용. 스폰 빈도가 낮아 Instantiate가 효율적. | |

**User's choice:** Instantiate (Prefab) (추천)

---

## 스테이지 2 패턴 선택 로직

### 사이클 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 고정 순환: 분신→은신→돌진→회복 | 매 사이클마다: Clone spawn → Stealth+teleport → Charge → 짧은 회복 동안 분신 Destroy → 반복. Stage2CombatState가 순서를 코루틴으로 관리. | |
| S1과 동일: 거리 기반 선택 + 유혹 스테이지 2 패턴 (커스텀) | S1과 동일한데 s2돌입과 동시 혹은 그로기에서 풀리면서 분신 삽입, 이후 돌진 이전까지는 분신과 각각의 패턴을 최대 2번 수행. 이후 분신과 다같이 은신돌진 이후 그로기상태로 전환하면서 분신 삭제 | ✓ |

**User's choice:** S1과 동일한데 s2돌입과 동시 혹은 그로기에서 풀리면서 분신 삽입, 이후 돌진 이전까지는 분신과 각각의 패턴을 최대 2번 수행. 이후 분신과 다같이 은신돌진 이후 그로기상태로 전환하면서 분신 삭제 (사용자 직접 입력)

### 헤비콤보 동기화

| Option | Description | Selected |
|--------|-------------|----------|
| 진짜가 분신들에 명령 전달 | Stage2CombatState가 분신 SpiritController 리스트를 보유하고, 순환 3단계에서 TriggerHeavyCombo() 메서드를 호출하여 분신들도 동일 순간 은신+돌진 실행. | ✓ |
| 모든 엔티티 독립 실행 (AI입력 기반) | 분신도 SpiritCombatState가 있어 독립적으로 동일 패턴을 실행. 동시성은 우연히 맞춰질 수도 있고 어긋날 수도 있음. | |

**User's choice:** 진짜가 분신들에 명령 전달 (추천)

### 패턴 카운터 구조

| Option | Description | Selected |
|--------|-------------|----------|
| Stage2CombatState가 카운터 정수 보유 | Stage2CombatState 내 _attacksThisCycle int 추적. 패턴 1회 실행될 때마다 +1, 2회 도달 시 순환 3단계(은신+돌진) 트리거. | |
| 직접 정하지 말고 Claude 재량 | 카운터 구조는 플래너 재량으로 결정하고 대신 최대 회수만 지정(2회)하고 싶음. | ✓ |

**User's choice:** 직접 정하지 말고 Claude 재량

### 스테이지 2 진입 시 실행

| Option | Description | Selected |
|--------|-------------|----------|
| 분신 즉시 스폰 | Stage2로 ChangeState 되는 순간 Instantiate DummyPrefab 2개. | ✓ |
| 일시적 무적 (대기 없이) | HP 50% 신호 후 즈음없이 바로 Stage2CombatState 시작. | ✓ |

**User's choice:** 분신 즉시 스폰 + 일시적 무적 (대기 없이) 동시 선택

---

## Claude's Discretion

- 패턴 카운터 구조 (일반 단계 최대 N회 추적 방식)
- 그로기 해제 후 Stage2CombatState 재진입 시 구체적 인터셉트 구현
- 헤비콤보 단계 진입 조건 (카운터 >= 2 트리거 타이밍)
- 분신 스폰 정확한 위치 (보스 주변 반경 등)
- 순간 무적의 구체적 구현 방식

## Deferred Ideas

- 애니메이션·이펙트 — v3.0+
- 은신 시각 효과 (투명화 등) — v3.0+
- 스테이지 전환 연출 — v3.0+
- 분신과 진짜 보스 시각 구별 — v3.0+
