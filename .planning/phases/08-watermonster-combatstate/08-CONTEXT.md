# Phase 8: WaterMonster 보스를 CombatState 기반 패턴 판단 로직으로 마이그레이션 - Context

**Gathered:** 2026-07-27
**Status:** Ready for planning

<domain>
## Phase Boundary

`WaterMonsterCombatState.SelectAttackStrategy`의 "일반 공격 패턴 풀" 선택 로직(`WaterGeyser`/`WaterWavePush`/`WaterMeleeSwipe`/`WaterJumpLand`/`WaterRangedSpit`/페이즈별 프리즌 변형)을 `CombatState`(Phase 7에서 추가된 `PatternCandidate` + `SelectWeightedPattern`) 헬퍼 기반으로 교체한다. 이 페이즈는 또한 CombatState 공통 헬퍼에 새로운 범용 옵션(직전 1개 패턴 가중치 감쇠)을 추가한다 — WaterSpirit도 향후 선택적으로 쓸 수 있지만, 이번 페이즈에서 WaterSpirit 코드를 되돌리지는 않는다.

**범위 외:**
- 장판 스폰(Enrage 상태의 `CanSpawnZone`/`SpawnRandomZone`)과 텔레포트 상태 전환(`WaterTeleportState` 진입) 분기 — `SelectAttackStrategy` 최상단의 사전 가드(early return, `null` 반환 + 직접 `ChangeState`)로 그대로 유지한다. 이들은 "공격 패턴 후보"가 아니라 상태 전환 트리거이므로 후보 풀에 포함하지 않는다.
- WaterSpirit(`SpiritCombatState`)의 기존 완전배제 연속금지(Phase 7 D-05) 방식 변경 — 그대로 유지, 되돌리지 않는다.
- 그로기(Groggy) 시스템 — `WaterMonsterCombatState.ShouldTransitionToGroggy()`는 항상 `false`를 반환하며 이번 페이즈에서 변경하지 않는다.
- 애니메이션·시각 이펙트 연동 (v3.0+)

</domain>

<decisions>
## Implementation Decisions

### D-01: 연속 사용 처리 — 완전배제 대신 가중치 감쇠
- **D-01a:** WaterMonster에는 Phase 7(D-05, 완전배제)과 다른 방식을 새로 도입한다: 직전에 실행한 패턴은 완전히 제외되지 않고, 가중치만 감쇠된 채로 여전히 후보에 남는다. 확률은 낮아지지만 연속 실행이 아예 불가능하지는 않다.
- **D-01b:** 감쇠는 정확히 "직전 1개 패턴"에만 적용된다 (2턴 전 이상 패턴은 감쇠 없이 원래 가중치). 지수 누적 감쇠 아님.
- **D-01c:** 감쇠 비율은 0.5배(절반)로 고정한다.
- **D-01d:** 이 감쇠 메커니즘은 `CombatState` 공통 헬퍼에 범용 옵션으로 추가한다 (예: `SelectWeightedPattern`에 감쇠 파라미터 추가, 또는 별도 오버로드). WaterSpirit은 기존 완전배제(D-05) 방식을 그대로 유지하며 이번 페이즈에서 감쇠 방식으로 전환하지 않는다 — 두 메커니즘이 헬퍼 안에 공존한다.

### D-02: 패턴 기본 가중치
- **D-02a:** 각 패턴의 기본 가중치는 균등(예: 전부 1.0)하게 유지한다 — 기존 uniform pool-random과 동일한 체감을 목표로 한다. D-01의 감쇠만이 유일한 차등 요소이며, 이번 페이즈에서 패턴 간 임의 가중치 재조정(밸런싱)은 하지 않는다.

### D-03: 광폭화(Enrage)가 개별 패턴 쿨다운에도 적용
- **D-03a:** 현재 광폭화는 전체 판단 대기시간(`_decisionTimer`)만 0.5배 단축한다. 새로 도입되는 패턴별 개별 쿨다운(Phase 7의 `_patternReadyAt` 메커니즘)에도 동일한 배율(0.5배)이 적용되어야 한다 — 광폭화 중에는 모든 패턴이 쿨다운 측면에서도 더 자주 나올 수 있어야 한다.
- **D-03b:** 정확한 구현 지점(예: `CommitSelection` 호출 시 배율 전달, 또는 쿨다운 설정 후 별도 배율 적용)은 플래너 재량.

### D-04: WaterWavePush 특수 쿨다운 유지
- **D-04a:** `WaterWavePush`의 45초 특수 재사용 쿨다운(다른 패턴보다 훨씬 긴 값)은 이번 마이그레이션에서도 그대로 유지한다 — 밸런스를 바꾸지 않는다.
- **D-04b:** D-03에 따라 광폭화 중에는 이 45초도 동일하게 0.5배(약 22.5초)로 단축된다.

### D-05: 사전 가드(장판 스폰/텔레포트) 범위 경계
- **D-05a:** `CanSpawnZone`+`SpawnRandomZone`(광폭화 장판 스폰)과 `PuddleStackManager` 기반 텔레포트 전환 분기는 새 후보 풀 헬퍼 밖에서, `SelectAttackStrategy` 최상단의 조건부 early-return으로 그대로 유지한다. 코드 흐름·조건·쿨다운 값 전부 변경하지 않는다.

### D-06: 페이즈별(Phase1/2/3) 프리즌 변형과 거리 기반 근접/원거리 분기
- **D-06a:** 근접(`dist <= 3.0`) → `WaterMeleeSwipe`/`WaterJumpLand`, 원거리 → `WaterRangedSpit` 분기는 헬퍼의 `MinDistance`/`MaxDistance` 조건부 후보로 재구성한다.
- **D-06b:** 페이즈별 프리즌 패턴(`WaterPrisonAttack`/`WaterPrisonMapAoe`/`WaterColorPrison`)은 현재 페이즈(`wmc.IsPhase2`/`IsPhase3`)에 해당하는 것 하나만 후보에 오르도록 유지한다 (상호 배타적, 기존 동작과 동일).
- **D-06c:** WaterSpirit(`SpiritCombatState`)처럼 `Enter()`에서 후보 목록을 한 번만 구성할 수 없다 — WaterMonster는 전투 세션 도중 페이즈가 바뀌므로(HP 임계치 통과), 후보 목록(혹은 최소한 페이즈 의존 후보)은 페이즈 변화를 반영해 최신 상태로 판단되어야 한다. 정확한 갱신 시점/방식(매 판단마다 재구성 vs 페이즈 전환 콜백에서 갱신)은 플래너 재량.

### Claude's Discretion
- D-01d 감쇠 메커니즘의 정확한 API 형태 (`PatternCandidate` 확장 vs `SelectWeightedPattern` 오버로드 vs 별도 메서드)
- D-06c 페이즈 변화에 따른 후보 목록 재구성 시점/방식
- D-03b 광폭화 배율을 개별 쿨다운에 적용하는 정확한 구현 지점
- `WaterGeyser` 등 나머지 패턴의 정확한 가중치 수치(균등 유지 원칙 하에서의 구체값)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 리팩토링 대상 (수정 필요 파일)
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — 현재 `SelectAttackStrategy`의 pool-random 로직 → 헬퍼 기반 후보 목록으로 교체 대상. `Enter`/`Execute`의 광폭화 배율 훅(`_enrageCooldownMultiplier`)도 D-03 확장 대상.
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — Phase 7에서 추가된 `PatternCandidate`/`SelectWeightedPattern`/`ForceSelectPattern`/`CommitSelection`. D-01d 가중치 감쇠 옵션을 여기에 추가.

### 변경하지 않는 연동 지점 (참고용, 손대지 않음)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — `CanSpawnZone`/`SpawnRandomZone`/`RecordZoneTime`(장판), `CanTeleport`/`RecordTeleportTime`(텔레포트), `IsPhase2`/`IsPhase3`/`IsEnraged` 프로퍼티. D-05/D-06 판단에 그대로 사용.
- `Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs` — 텔레포트 상태, `CombatState`를 상속하지 않으므로 헬퍼와 무관.

### 참고 구현 (Phase 7에서 이미 헬퍼를 사용 중인 예시)
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` — `PatternCandidate` 선언 + `SelectWeightedPattern` 사용 예시 (단, `Enter()`에서 후보 목록을 한 번만 구성하는 방식은 WaterMonster에 그대로 적용 불가 — D-06c 참고).

### 프로젝트 문서
- `.planning/phases/07-boss-attack-pattern-judgment/07-CONTEXT.md` — D-01(헬퍼 설계 목적: "향후 WaterMonster를 포함한 다른 보스도 재사용"), D-08b(이번 페이즈로 명시적으로 이월된 항목)
- `.planning/phases/07-boss-attack-pattern-judgment/07-01-SUMMARY.md` — 헬퍼의 기존 설계 결정(타입 키 기반 쿨다운/연속금지, `Time.time` 절대 시각 비교 방식)
- `Assets/Enemy/WaterSpirit/Check.md`, `Assets/Enemy/Tutorial/TutorialBoss/Check.md` — Phase 7에서 보류된 Play 모드 검증 체크리스트. 이번 페이즈 완료 후 WaterMonster용 Check.md와 함께 3개 보스를 일괄 검증할 예정 (사용자 결정, Phase 7 세션에서 합의됨).

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CombatState._decisionTimer` — 전체 판단 대기 타이머, 광폭화 배율이 이미 이 값에 곱해지는 기존 훅 (`WaterMonsterCombatState.Execute`)
- `CombatState._patternReadyAt` (Phase 7 신규) — 패턴별 쿨다운 만료 시각 딕셔너리. D-03 광폭화 배율 확장 지점.
- `IAttackStrategy.Cooldown` — 각 패턴 자체 쿨다운 값 노출 중

### Established Patterns
- `WaterMonsterController.Update()`가 `CombatState` 타입을 `WaterMonsterCombatState`로 가로채는 상태 인터셉트 패턴 — Phase 7과 동일 구조, 변경 없음
- `SelectAttackStrategy`가 `null`을 반환하면 다음 프레임 재시도 — 장판 스폰/텔레포트 분기가 이 계약을 이미 활용 중 (그대로 유지)

### Integration Points
- `WaterMonsterCombatState.Execute()`의 `_decisionTimer *= _enrageCooldownMultiplier` 직후 지점 — D-03 개별 쿨다운 배율 확장이 자연스럽게 연결되는 곳
- `WaterMonsterController.IsPhase2`/`IsPhase3`는 HP 임계치 통과 시 1회성 `true`로 전환되는 프로퍼티 — D-06c 후보 갱신 시점 판단에 사용

</code_context>

<specifics>
## Specific Ideas

- "최근에 사용한 패턴일수록 가중치를 줄이는 방식으로 패턴을 다양하게 하고 싶다" — 사용자가 직접 제안한 메커니즘. 완전배제가 아니라 확률 감쇠로 다양성을 유도하는 것이 핵심 의도.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 08-watermonster-combatstate*
*Context gathered: 2026-07-27*
