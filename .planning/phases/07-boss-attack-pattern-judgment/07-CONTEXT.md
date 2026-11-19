# Phase 7: 보스 공격 패턴 판단 로직 리팩토링 - Context

**Gathered:** 2026-07-27
**Status:** Ready for planning

<domain>
## Phase Boundary

`CombatState`(`Assets/Enemy/NewBoss/Script/States/CombatState.cs`)에 재사용 가능한 범용 패턴 판단 헬퍼(우선순위/조건부 후보 평가)를 추가하고, WaterSpirit 보스의 `SpiritCombatState`(Stage 1)와 `Stage2CombatState`(Stage 2)가 이를 사용하도록 리팩토링한다. 현재 `SpiritCombatState.SelectAttackStrategy`는 조건 없는 고정 순서 라운드로빈(`_pattern[_patternIndex++ % length]`)이며, 이는 Phase 5 CONTEXT.md(D-03a)가 원래 의도했던 거리 기반 우선순위(근접→튕겨내기, 중거리→돌진, 원거리→투사체)와 어긋난 상태다. TutorialBoss(`Assets/Enemy/Tutorial/TutorialBoss/`)의 `SelectPattern()` 스타일(거리/쿨다운/연속금지 조건부 `CanUseX()` 판단)을 참고해 이 판단 로직을 되살린다.

**범위 외:**
- WaterMonsterCombatState 리팩토링 — 자체 거리/쿨다운/랜덤풀 판단 로직을 이미 보유. 이번 페이즈는 손대지 않는다 (다음 페이즈에서 다룰 후보로 별도 기록).
- 애니메이션·시각 이펙트 연동 (v3.0+)
- 패턴 수치(쿨다운/가중치) 밸런싱 최종값 확정 — 플래너/구현 재량

</domain>

<decisions>
## Implementation Decisions

### D-01: 판단 로직 아키텍처
- **D-01a:** `CombatState`에 범용 우선순위 기반 패턴 후보 평가 헬퍼를 추가한다 (예: 거리 범위, 쿨다운, 연속사용금지, 가중치를 가진 "패턴 후보" 목록을 선언하면 판단이 이루어지는 구조).
- **D-01b:** `SpiritCombatState`는 이 헬퍼 위에 자신의 패턴 후보 목록만 선언하는 얇은 데이터 레이어가 된다.
- **D-01c:** 목적: 향후 WaterMonster를 포함한 다른 보스도 같은 헬퍼를 재사용할 수 있도록 일반적으로 설계한다 (단, WaterMonster 자체를 이번에 마이그레이션하지는 않음 — D-05 참고).

### D-02: 순간이동형 패턴(SpiritCharge, SpiritFarProjectile)의 발동 조건
- **D-02a:** `SpiritCharge`, `SpiritFarProjectile`은 거리 조건 없이 **쿨다운 + 직전 패턴과 다름(연속금지)** 만으로 판단한다 — 공격 전에 스스로 순간이동하므로 거리 조건이 무의미하기 때문.
- **D-02b:** 헬퍼는 패턴별로 거리 조건을 선택적(optional)으로 걸 수 있게 설계한다 — 순간이동형은 거리 조건을 생략하고, 근접(WakeRepel→Repel)·원거리(기본 투사체류) 패턴에는 거리 조건을 건다.
- **D-02c:** 이 "보스에 따라 조건을 다르게 적용"하는 유연성은 헬퍼 설계 자체(D-01)의 요구사항이다.

### D-03: 패턴 우선순위 — 가중치 기반 랜덤
- **D-03a:** TutorialBoss처럼 고정 우선순위 리스트가 아니라, **현재 조건을 만족하는(쿨다운 통과 + 연속금지 통과 + 거리 조건 있으면 통과) 후보들 중 가중치 기반 랜덤**으로 하나를 선택한다.
- **D-03b:** 가중치 구체값은 플래너/구현 재량. WaterMonsterCombatState의 풀 기반 랜덤 선택 방식을 참고 가능.

### D-04: 패턴 체인 — Exhaustion → WakeRepel 강제 연결
- **D-04a:** `SpiritExhaustion`(지침/정지/취약) 실행 직후에는 판단 로직을 거치지 않고 **무조건 `SpiritWakeRepel`을 다음 패턴으로 강제 실행**한다.
- **D-04b:** WakeRepel 실행이 끝나면 이후에는 다시 일반 판단 풀(가중치 랜덤 후보 중 하나)로 복귀한다 — WakeRepel은 체인 밖에서도 독립 후보로 계속 존재한다.
- **D-04c:** 체인 트리거 조건은 "직전 실행 패턴 == SpiritExhaustion"이며, 헬퍼의 일반 후보 평가보다 우선 처리된다.

### D-05: 연속 사용 금지 규칙
- **D-05a:** TutorialBoss와 동일하게, **직전에 실행한 패턴과 같은 패턴은 이번 판단에서 후보 제외**한다. 5개 패턴(Charge/Exhaustion/WakeRepel/FarProjectile + 중복 슬롯) 모두 동일 규칙 적용.
- **D-05b:** D-04 체인(Exhaustion→WakeRepel 강제)은 연속금지 규칙보다 우선한다 — 체인 강제 실행에는 연속금지가 적용되지 않는다.

### D-06: 회전 배열 0번/4번 중복 슬롯 (`SpiritCharge` 중복)
- **D-06a:** 이번 페이즈에서는 손대지 않는다. 새 판단 로직에서는 `SpiritCharge`를 하나의 후보로만 등록한다 (중복 제거는 자연스럽게 발생 — 라운드로빈 배열 자체가 사라지므로).
- **D-06b:** 과거 코드의 "5. 순간이동 + 돌진" 주석은 실제로는 구현되지 않은 미완성 의도였다는 점만 기록. 새 패턴을 만들지 않는다.

### D-07: Stage 2 헤비콤보 카운터 연동
- **D-07a:** `Stage2CombatState`의 "일반 패턴 3회 실행 후 헤비콤보 전환" 카운터 로직은 그대로 유지한다.
- **D-07b:** `SelectAttackStrategy`가 조건 불충족으로 `null`을 반환할 수 있다는 기존 계약은 동일하게 유지되므로, 새 판단 로직 도입 후에도 `Stage2CombatState`의 카운터(`_patternsExecuted`, `if (strategy != null) _patternsExecuted++`)는 수정 없이 호환된다.

### D-08: 적용 범위
- **D-08a:** 이번 페이즈는 WaterSpirit 보스(`SpiritCombatState`, `Stage2CombatState`)에만 적용한다.
- **D-08b:** `WaterMonsterCombatState`는 건드리지 않는다 — 다음 페이즈(백로그)에서 동일 헬퍼로 마이그레이션할지 별도 검토.

### Claude's Discretion
- 범용 헬퍼의 정확한 API 형태 (클래스/구조체 설계, 후보 등록 방식) — 플래너 재량
- 각 패턴의 쿨다운/가중치 수치 — 기존 `IAttackStrategy.Cooldown` 값 유지 또는 조정 가능 (플래너 재량)
- 근접(WakeRepel)·원거리(기본 투사체) 패턴의 거리 임계값 — 기존 `RepelRange`/`ChargeRange`/`ProjectileRange` Inspector 값 재사용 여부는 플래너 재량

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 리팩토링 대상 (수정 필요 파일)
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — 범용 판단 헬퍼 추가 대상 (§100-105 현재 기본 `SelectAttackStrategy` 구현부)
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` — 현재 고정 라운드로빈(`_pattern[]`, `_patternIndex`) → 헬퍼 기반 후보 목록으로 교체
- `Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs` — `SelectAttackStrategy` override, `_patternsExecuted` 카운터 (수정 없이 호환 확인 필요)

### 참고 구현 (판단 로직 스타일 레퍼런스)
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` — `SelectPattern()`, `CanUseTentacleStab()`/`CanUseGroundTentacle()`, 쿨다운 타이머, `LastUsedPattern` 연속금지 가드
- `Assets/Enemy/Tutorial/TutorialBoss/State/TutorialAttackState.cs` — 판단은 Idle에서만, Attack 진입 후 패턴 끝까지 실행하는 구조
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — 거리 조건 + 쿨다운(`_lastWaveTime`) + 풀 기반 랜덤 선택 예시 (D-03 가중치 랜덤 설계 참고용)

### 관련 공격 전략 (기존 IAttackStrategy 구현)
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs` — 자가 순간이동 후 돌진, Cooldown 3.0f
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritExhaustion.cs` — 정지/취약, Cooldown 2.5f
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritWakeRepel.cs` — 0.4초 대기 후 SpiritRepel 실행, Cooldown 1.5f
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritFarProjectile.cs` — 자가 순간이동(2×MaxTeleportRadius) 후 SpiritProjectileAttack, Cooldown 2.5f
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritRepel.cs` — RepelRange 근접 판정, WakeRepel이 내부 호출

### 프로젝트 문서
- `.planning/phases/05-spirit-boss-stage1-patterns/05-CONTEXT.md` D-03a — 원래 의도했던 거리 기반 우선순위 (근접→튕겨내기/중거리→돌진/원거리→투사체), 실제 구현과 어긋난 부분
- `.planning/phases/06-spirit-boss-stage2-system/06-CONTEXT.md` D-10a — Stage2가 부모 `SelectAttackStrategy`를 재사용하는 현재 구조
- `.planning/PROJECT.md` — 상태 패턴 재사용 원칙, 상태 인터셉트 패턴

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CombatState._decisionTimer` — 공격 종료 후 쿨다운 대기 메커니즘, 헬퍼 설계 시 그대로 활용 가능
- `IAttackStrategy.Cooldown` — 각 패턴이 이미 자기 쿨다운 값을 노출 중 (헬퍼가 재사용 가능)
- `SpiritController.RepelRange` / `ChargeRange` — 거리 조건에 쓸 기존 Inspector 필드

### Established Patterns
- 상태 인터셉트: `SpiritController.Update()`가 `CombatState` 타입 체크로 `SpiritCombatState`/`Stage2CombatState` 교체 — 이번 페이즈에서 변경 없음
- `IAttackStrategy` 구현체는 전부 `boss.StartCoroutine(...)` 기반 — 판단 헬퍼는 실행이 아닌 "어떤 전략을 선택할지"만 책임진다

### Integration Points
- `Stage2CombatState.SelectAttackStrategy`가 `base.SelectAttackStrategy(boss, dist)`를 호출하는 지점이 새 헬퍼의 진입점이 된다
- `_patternsExecuted` 카운터는 `SelectAttackStrategy`의 반환값(`null` 여부)에만 의존하므로 헬퍼 교체와 독립적

</code_context>

<specifics>
## Specific Ideas

- 라운드로빈 배열이 통째로 사라지므로 0번/4번 `SpiritCharge` 중복도 자연히 하나로 정리된다 (D-06)
- Exhaustion→WakeRepel 체인은 "취약해졌다가 급습" 느낌을 실제 로직으로 구현하려는 의도

</specifics>

<deferred>
## Deferred Ideas

- **WaterMonsterCombatState를 동일 범용 헬퍼로 마이그레이션** — 사용자가 명시적으로 "다음 페이즈에서 WaterMonster도 적용"이라고 언급. 향후 페이즈 후보로 기록.
- 회전 배열 0/4번 중복 슬롯을 실제로 구분되는 새 변종 패턴("진짜 순간이동+돌진")으로 만드는 것 — 이번엔 보류, 필요 시 별도 요청.

</deferred>

---

*Phase: 07-boss-attack-pattern-judgment*
*Context gathered: 2026-07-27*
