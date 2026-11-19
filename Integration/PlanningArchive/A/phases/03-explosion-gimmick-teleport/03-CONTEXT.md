# Phase 3: 폭발 기믹 연계 및 보스 순간이동 - Context

**Gathered:** 2026-04-16
**Status:** Ready for planning

<domain>
## Phase Boundary

`PuddleStackManager.OnThresholdReached` 이벤트를 실제 연쇄 AoE 폭발로 구현하고, `WaterTeleportState`를 추가해 보스가 Indestructible 웅덩이를 매개체로 플레이어를 농락하는 포지셔닝 패턴을 사용한다.

**이번 페이즈 포함하지 않음:** 광폭화 모드 (Phase 4), 이속/감속 장판 (Phase 4).

</domain>

<decisions>
## Implementation Decisions

### 연쇄 폭발 시퀀스
- **D-01:** 폭발 방식은 **동시 전체 폭발** — 임계치 도달 시 맵의 모든 Indestructible 웅덩이에서 동시에 AoE가 발동. 순차 연쇄가 아닌 1프레임 동시 폭발.
- **D-02:** 폭발 전 **경고 딜레이 2초** — 임계치 도달 즉시 경고 이펙트(빛남/사운드) 발생 → 2초 후 실제 폭발 AoE 실행. 플레이어가 반응할 시간 확보.
- **D-03:** 경고 딜레이 수치(2초)는 인스펙터에서 튜닝 가능하게 노출.
- **D-04:** 폭발 AoE는 각 웅덩이 위치에서 반경 기반 OverlapCircleAll로 Player 레이어에만 대미지 (REQ-WM-X-01 준수).

### 폭발 후 웅덩이 처리
- **D-05:** 폭발 후 Indestructible 웅덩이 **전부 Pool Return** + `_indestructibleCount = 0` 리셋.
- **D-06:** 리셋 후 플레이어가 다시 웅덩이를 흡수하면 스택이 다시 쌓이는 사이클이 반복됨. 텐션-해소-텐션 루프.

### WaterTeleportState 동작 (농락형 포지셔닝)
- **D-07:** 텔레포트 타겟 선택은 **플레이어와의 거리에 반비례** — 보스가 플레이어를 농락하는 핵심 메커니즘:
  - **플레이어가 가까이** (근접 범위 내) → 가장 **먼** Indestructible 웅덩이로 이동 → 원거리 패턴 사용
  - **플레이어가 멀리** (원거리) → 가장 **가까운** Indestructible 웅덩이로 이동 → 근접 패턴 사용
- **D-08:** 거리 임계치(근접/원거리 구분 기준)는 기존 `CombatState`의 `dist ≤ 3.0f` 기준을 재사용.
- **D-09:** 텔레포트 연출: 출발지 **사라짐 VFX** → position 이동 → 목적지 **나타남 VFX**. 사이 딜레이 0.2~0.3초 (플래너가 기존 VFX 에셋 확인 후 결정).
- **D-10:** 도착 직후 **즉시 CombatState 복귀** — 텔레포트 자체가 포지셔닝 수단이며 도착 후 대기 없음.
- **D-11:** 텔레포트 패턴에도 **REQ-WM-02 HP 코스트 적용**.

### WaterTeleportState 전환 조건 (SelectAttackStrategy 통합)
- **D-12:** `WaterMonsterCombatState.SelectAttackStrategy` 내에서 기존 근접/원거리 패턴과 함께 텔레포트를 **공격 패턴 후보 중 하나**로 통합. 별도 State 진입 조건이 아닌 패턴 선택 로직에 포함.
- **D-13:** 텔레포트 선택 조건: `_indestructibleCount >= 2` 이고 텔레포트 쿨다운이 지났을 때. 조건 미충족 시 기존 패턴(근접/원거리) 사용.
- **D-14:** 텔레포트 쿨다운 수치는 인스펙터 튜닝으로 위임 (플래너가 밸런싱 결정).
- **D-15:** `WaterTeleportState`는 `IBossState` 구현 — `Enter`에서 타겟 웅덩이 선택 + HP 코스트 차감 + 연출 코루틴 시작. 완료 후 `CombatState` 복귀.

### Claude's Discretion
다음 항목은 리서처/플래너가 코드 확인 후 결정:
- 경고 이펙트 구체 에셋 (빛남 파티클 or 웅덩이 색 변화 or 화면 shake)
- 폭발 AoE 반경 수치 (인스펙터 튜닝)
- 텔레포트 VFX 에셋 (기존 이펙트 재사용 가능 여부 확인)
- 텔레포트 쿨다운 수치 (밸런싱, 인스펙터 튜닝)
- 폭발 대미지 수치 (치명적, 튜닝 위임)
- 텔레포트 연출 딜레이 정확한 수치 (0.2~0.3초 범위 내 플래너 결정)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 결정사항
- `.planning/PROJECT.md` — 상속 기반 확장 원칙, 레이어 분리 원칙
- `.planning/REQUIREMENTS.md` — REQ-WM-P3-01, REQ-WM-P3-02, REQ-WM-02, REQ-WM-X-01
- `.planning/ROADMAP.md` §`Phase 3` — Goal, Success Criteria, 의존성

### Phase 1/2 결정사항 (이어받음)
- `.planning/phases/01-boss-core-mechanics/01-CONTEXT.md` — DamageInfo/DamageElement, HP 코스트 패턴 (D-11에 필요)
- `.planning/phases/02-weather-puddle-interaction/02-CONTEXT.md` — PuddleStackManager.OnThresholdReached 훅, WaterPuddle 상태 구조, Object Pool 방식

### Phase 2 구현 코드 (직접 연결 대상)
- `Assets/Enemy/WaterMonster/Script/Phase2/PuddleStackManager.cs` — OnThresholdReached 이벤트, _indestructibleCount, 임계치 설정
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs` — isDestructible, SetIndestructible(), OnReturnToPool()
- `Assets/Enemy/WaterMonster/Script/Phase2/PuddlePool.cs` — Pool Return 방식 확인
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — SelectAttackStrategy 오버라이드 (D-12 텔레포트 통합 대상)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — Phase 2 트리거 구조 참고

### 베이스 State 구조
- `Assets/Enemy/NewBoss/Script/States/IBossState.cs` — WaterTeleportState 구현 인터페이스
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — SelectAttackStrategy 패턴, dist ≤ 3.0f 기준

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `PuddleStackManager.OnThresholdReached` — Phase 3 폭발 트리거 진입점. 이미 구현됨, 구독만 추가하면 됨
- `WaterPuddle.OnReturnToPool()` — Indestructible 웅덩이 파괴 후 Pool Return 시 사용
- `PuddlePool.Instance.ActiveCount` — 현재 활성 웅덩이 수 조회 가능
- `WaterMonsterCombatState.SelectAttackStrategy` — 텔레포트 후보 추가 위치 (override 이미 존재)
- `IBossState` — WaterTeleportState가 구현할 인터페이스

### Established Patterns
- 공격 패턴은 `IAttackStrategy` 또는 `IBossState` 구현 — 텔레포트는 포지셔닝 특성상 `IBossState`가 더 적합 (코루틴/비동기 연출)
- HP 코스트: `WaterMonsterStats`에서 공격 시 자가 소모 처리 (Phase 1 패턴 재사용)
- Object Pool: `PuddlePool.Instance.Return(puddle)` 방식

### Integration Points
- `PuddleStackManager` → 폭발 구독 + 폭발 후 전체 웅덩이 Pool Return 트리거
- `WaterMonsterCombatState.SelectAttackStrategy` → 텔레포트 조건 추가
- `WaterMonsterController` → 텔레포트 쿨다운 타이머 관리 위치 (또는 WaterMonsterCombatState 내부)

</code_context>

<specifics>
## Specific Ideas

- 농락형 패턴의 핵심: 플레이어가 가까이 붙으면 멀어지고, 멀리 있으면 가까이 오는 **반응형 포지셔닝**. 텔레포트 후 즉시 거리에 맞는 패턴(근접/원거리)으로 이어지므로 단순 이동이 아닌 공격 흐름의 일부.
- 텔레포트 조건(Indestructible ≥ 2)이 폭발 임계치(기본 5)보다 낮아서 — 폭발 직전 단계에서 텔레포트 패턴이 등장하는 자연스러운 긴장감 흐름이 만들어짐.

</specifics>

<deferred>
## Deferred Ideas

- **텔레포트 후 추가 패턴 연계** (예: 도착 즉시 AoE 발사) — Phase 4 광폭화에서 고려
- **광폭화 모드** — Phase 4 소관
- **이속/감속 장판** — Phase 4 소관
- **폭발 횟수 제한 또는 강화** (예: n번째 폭발은 더 강함) — Phase 4 또는 밸런싱 단계

</deferred>

---

*Phase: 03-explosion-gimmick-teleport*
*Context gathered: 2026-04-16 via /gsd:discuss-phase*
