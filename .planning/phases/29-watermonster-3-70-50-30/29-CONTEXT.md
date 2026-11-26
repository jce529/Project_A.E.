# Phase 29: WaterMonster 3스테이지 보스 재구현 (기획 문서 기준) - Context

**Gathered:** 2026-11-05
**Status:** Ready for planning
**Source:** 사용자 제공 기획 문서 2건 + 사용자 결정 4건 (discuss-phase 대신 대화에서 확정, PRD 급행 경로에 준함)

<domain>
## Phase Boundary

`WaterMonster`(3스테이지 최종보스, `WaterMonsterController : BossController`)를 사용자의 기획 문서 `29-SOURCE-3스테이지-보스.md`(이하 "기획 문서")에 맞게 재구현한다. 애니메이션 클립은 아직 없으므로 **애니메이터에 들어갈 동작(패턴, 상태 전환)의 로직과 트리거 위주**로 만든다. 연출·이펙트·아트는 범위 밖이다.

기획 문서가 최우선이다. `29-SOURCE-제작가이드-new.md`(외형 피드백 + 일부 모션)는 보조이며 충돌하면 기획 문서가 이긴다.

**범위 밖:**
- 애니메이션 클립/애니메이터 컨트롤러 제작, 스프라이트, 이펙트, UV 스크롤 문신 셰이더, 화면 어두워짐/검은 비 연출
- 물 속성 회복, 보스 자가 HP 소모(문서에 없음 → 비활성화 대상일 뿐 삭제 아님)
- 다른 보스(WaterSpirit, TutorialBoss), `SpiritCombatState`/`Stage2CombatState` 동작 변경
- 실제 3스테이지 씬(`Assets/Scenes/3stage/...`) 확정 배치(검증은 `Assets/Scenes/TestScene.unity`에서; 실제 씬 배치는 사용자 결정)
</domain>

<decisions>
## Implementation Decisions (사용자 확정 — 잠금)

### 페이즈
- **D-01:** 임계치 매핑: HP 70% = 기획 문서 2페이즈, 50% = 3페이즈, 30% = 3페이즈 강화(수치만 상향). 1페이즈는 시작~70%.
- **D-02:** 후보 풀은 **누적**이다. 1페이즈: 물 솟아오르기, 점프 착지, 물 확산 폭발, 물 감옥. 2페이즈: 위 + 감옥→전체 공격 + 웅덩이 시스템(생성/이동/폭발). 3페이즈: 위 + 색깔 감옥 + 파도 밀기(엇박), 물 솟아오르기/점프 착지/물 확산 폭발은 "강화". 현재 코드의 "페이즈마다 프리즌 변형 하나로 교체" 구조는 폐기한다.
- **D-03:** 1페이즈·2페이즈는 패턴 **단일 사용**(앞 패턴이 끝나기 전 다음 패턴 금지), 3페이즈는 패턴 **동시 사용/겹침 허용**. 현재는 모든 페이즈에서 겹침 가능하고 광폭화 타이머가 매 프레임 x0.5 되는 문제가 있어(`WaterMonsterCombatState.Execute`) 이 규칙으로 대체한다.
- **D-04:** 문서에 없는 `WaterMeleeSwipe`, `WaterRangedSpit`, 광폭화 장판(SpeedUp/SlowDown) 스폰, 보스 자가 HP 소모는 후보/동작에서 제외한다. **코드 파일은 삭제하지 않는다.**

### 그로기
- **D-05:** 그로기는 페이즈 전환 **전에** 넣는다: HP 임계치(70%, 50%) 도달 → 그로기 상태 → (전환 연출 자리) → 페이즈 플래그 갱신. 30% 강화 진입에도 적용할지, 그로기 지속/종료 조건, 그로기 중 피해 배율은 재량(아래).

### 웅덩이 시스템
- **D-06:** 웅덩이 시스템 전체를 이번 페이즈에서 한 번에 구현한다: 2페이즈 진입 시 랜덤 위치 생성, 보스 종료 시까지 유지, 보스 이동(웅덩이 진입 → 다른 웅덩이로 이동), 웅덩이별 일정 시간 후 폭발, 플레이어 물 흡수(마시면 웅덩이 제거 불가), 제거(문서: 물가르기). 기존 `WaterPuddle`, `PuddleSpawner`, `PuddleStackManager`, `PuddleExplosionController`, `WaterTeleportState`, `PlayerAbsorb`를 문서 방식에 맞게 재사용·수정한다.

### 패턴 사양 (기획 문서 "프로그래머용 설계")
- **D-07 물 솟아오르기:** 랜덤, 전조 0.7초 → 생성 → 타격, 원형 판정. (현 `WaterGeyser` 유지)
- **D-08 점프 착지:** 플레이어 근접 시, 수축 0.5초 → 점프 → 공중 정지 0.4초 → 낙하 → 충격파(광역). (현 `WaterJumpLand` 유지)
- **D-09 물 확산 폭발(신규):** 랜덤, 흡수 0.6초 → 정지 0.4초 → 폭발(광역).
- **D-10 심연의 물 감옥:** 랜덤, 몸 주위에 물 구체 생성(응축) → 2회 발사 → 피격 시 감옥(이동 불가·공격 불가·지속 도트딜, 탈출은 연타 게이지). 1페이즈에서는 위험 요소로만(전체 공격 없음).
- **D-11 감옥→전체 공격:** 2페이즈 이상, 랜덤. 감옥과 똑같이 구체 생성·발사 → 2.5초 후 전체 공격. 감옥 내부는 안전, 외부 전체 피해. 애니메이터 트리거는 감옥과 **분리**(현재 `Attack_Prison` 공유).
- **D-12 색깔 감옥:** 3페이즈, 랜덤. 코어 색 변경(빨강/파랑) → 랜덤 위치에 빨강/파랑 감옥 생성 → 색 일치 생존, 불일치 피해. (현 `WaterColorPrison` 로직 유지, 정리 코드 견고화는 재량)
- **D-13 파도 밀기(엇박):** 좌측 생성 → 이동 → 딜레이 → 우측 생성, 타이밍과 속도가 다르다. (현 `WaterWavePush` 구조와 동일, 문서 순서 "좌측 먼저" 반영)

### Claude's Discretion (플래너가 근거와 함께 결정)
- **애니메이터 트리거 이름 정리:** 신규(물 확산 폭발, 감옥→전체 분리, 웅덩이 생성/이동/폭발, 그로기, 페이즈 전환)는 기존 `Attack_*` 규칙을 따르는 이름으로. 아직 클립이 없으므로 코드가 트리거 파라미터 부재로 오류를 내지 않게 하는 방법(예: 파라미터 존재 확인 후 SetTrigger)을 포함한다. **애니메이터 컨트롤러 자체는 이번에 수정하지 않는다**(클립 없음, 아트 작업 전).
- 3페이즈 "강화"와 30% 강화의 구체 수치(범위/피해/전조 단축 등) — 문서에 값이 없으므로 보수적 기본값을 정하고 Inspector 노출.
- 심연의 물 감옥의 "구체 생성(응축)" 구현 방식과 발사 방향(문서: 프로그래머 설계는 2회 발사, 아트 문서는 "사방 직선 발사" — 해석 확인 필요; 현 코드는 플레이어 조준 2발).
- 그로기의 지속 시간/종료 조건/피해 배율, 30% 강화 진입 시 그로기 적용 여부.
- 웅덩이 제거 수단: 물가르기 스킬(`WaveSlice.cs`)이 커밋 a9c427f에서 삭제돼 현재 경로가 없다. 대체 입력/히트박스 설계 필요 — 사용자 확인이 필요하면 플래너가 질문 항목으로 남긴다.
- 웅덩이 "보스 이동"의 진입 조건과 현재 `WaterTeleportState`(파괴 불가 웅덩이 ≥2 + 쿨다운 8초 → 즉시 순간이동) 재구성 방식.
- 각 패턴의 쿨다운/가중치.
- 페이즈별 누적 후보 구성 시점(매 판단 재구성 — Phase 8 D-06c 방식 유지 권장).
</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기획 (최우선)
- `.planning/phases/29-watermonster-3-70-50-30/29-SOURCE-3스테이지-보스.md` — 페이즈 구조, 패턴 1~8 프로그래머 설계, 패턴 1~10 아트 애니메이션 목록
- `.planning/phases/29-watermonster-3-70-50-30/29-SOURCE-제작가이드-new.md` — 외형/모션 보조 문서(그로기, 피격 경직, 2페이즈 전환, 웅덩이 이동, 사망 모션 설명)
- 갭 분석 원문(저장소 밖): `C:\Users\chang\AppData\Roaming\Claude\side-session-notes\local_b8e3dcfc-602c-474f-aebf-11f0b4e2594b\watermonster_stage3_doc_gap_and_decisions.md`

### 기존 구현 (수정/재사용 대상)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — 페이즈 플래그(IsPhase2/IsPhase3/IsEnraged), `SpawnRandomZone`, 텔레포트 쿨다운
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — 피해 규칙(Normal/WaveSlash만 피해, 그 외 회복), `SpendHpCost`, 광폭화 tick
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — 패턴 후보 선언(Phase 8), 광폭화 타이머 x0.5 매 프레임 문제
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — `PatternCandidate`, `SelectWeightedPattern`, `PatternHistory`, `GetPatternCooldownMultiplier`. `SpiritCombatState`/`Stage2CombatState`가 상속하므로 기본 동작 불변이어야 한다.
- `Assets/Enemy/WaterMonster/Script/States/Attacks/` — WaterGeyser, WaterJumpLand, WaterPrisonAttack, WaterPrisonMapAoe, WaterColorPrison(+Zone), WaterWavePush(+Projectile), WaterMeleeSwipe, WaterRangedSpit
- `Assets/Enemy/WaterMonster/Script/Phase2/`, `Phase3/WaterTeleportState.cs`, `Phase3/PuddleExplosionController.cs`, `Phase4/` — 웅덩이/폭발/텔레포트/장판
- `Assets/Player/Script/PrisonDebuff.cs` — 감옥 상태(점프 15회 연타 해제, 시간 제한 없음, 도트 3/초)
- `Assets/Player/Script/PlayerAttackDamager.cs`, `Assets/Enemy/Script/EnemyHitBox.cs` — 보스 피격 경로
- `Assets/Enemy/WaterMonster/WaterMonster.prefab` — 직렬화 값(아래 "검증된 사실")

### 선행 단계 문서
- `.planning/phases/08-watermonster-combatstate/08-CONTEXT.md`, `08-03-SUMMARY.md` — Phase 8 결정(D-01~D-06), 3종 보스 Play 검증 결과, 남은 이슈
- `.planning/phases/01-boss-core-mechanics/01-CONTEXT.md` ~ `04-enrage-zone-system/04-CONTEXT.md` — 물괴물 원 설계 결정
- `docs/ssot/systems/boss-combat.md` — 시스템 SSOT
- `Assets/Enemy/WaterMonster/Check.md`, `Assets/Enemy/WaterSpirit/Check.md` — 회귀 검증 체크리스트(Spirit 완전배제 회귀 항목 포함)
</canonical_refs>

<code_context>
## 검증된 현재 상태 (2026-11-05, 08-03 세션이 코드·프리팹·Play로 확인)

- **배치:** 보스 프리팹/`WeatherController`/`PuddleSpawner`/`PuddleStackManager`/`PuddleExplosionController`를 참조하는 씬·프리팹이 없다. 프리팹 `_weatherController`, `_speedUpZonePrefab`, `_slowDownZonePrefab`이 null, 장판 프리팹 파일 없음. `WaterPuddle.prefab`만 `Resources`에 있다.
- **애니메이터:** `WaterMonster` 컨트롤러 상태는 Idle/Attack_Melee/Attack_Ranged뿐(클립 없음), 파라미터는 Attack_Melee/Attack_Ranged 트리거 둘뿐. 나머지 공격이 호출하는 `Attack_Geyser/Jump/Wave/Prison/ColorPrison`은 존재하지 않는 파라미터라 `SetTrigger` 경고. 이 때문에 `CombatState`는 공격이 거의 즉시 끝난 것으로 보고 코루틴만 계속 돈다.
- **피해/회복:** `WaterMonsterStats`는 Normal/WaveSlash만 피해, 그 외 타입은 회복. `WaveSlice.cs`/`FlashSlice.cs` 삭제(a9c427f)로 WaveSlash/Other를 만드는 코드가 없다(`SetOverrideType` 호출자 없음). 기획 문서에는 회복 규칙이 없다.
- **BattleArea:** 프리팹에서 보스 자식(30x12 트리거). `PlayerAttackDamager`가 `GetComponentInParent<BossStatsSystem>`로 보스를 때려, 영역 안 공격이 보스 피격이 된다.
- **텔레포트:** 파괴 불가 웅덩이 개수만 ≥2이고 활성 대상이 없으면 `WaterTeleportState`가 매 프레임 진입/복귀하며 HP 5%를 소모한다.
- **광폭화 타이머:** `WaterMonsterCombatState.Execute`가 매 프레임 `_decisionTimer *= 0.5` — 패턴 간격이 사실상 0이 된다(Phase 8 이전부터 존재).
- **PrisonDebuff:** 점프 15회로만 해제. **ColorPrison:** 정리 코드에 try/finally가 없고 코루틴이 보스 MonoBehaviour에서 돌아 보스 비활성화/예외 시 존이 잔존.
- **측정:** 비광폭화 패턴 간격 약 3~5초, WavePush 45초 잠금은 `CombatState` 재진입 시 초기화(`PatternHistory` 기본 = Enter마다 새 이력).
- **테스트 씬:** `Assets/Scenes/TestScene.unity`(Main Camera, 바닥/벽, Player 프리팹, CameraController stub). stub이 없으면 `PlayerStats.TakeDamage`가 `CameraController.Instance.Shake()`에서 NRE를 던져 공격 코루틴이 중단된다. 보스 검증은 여기서, `1 stage` 씬으로 복구한다.
- **Unity CLI:** 에디터는 `unity command eval`로 구동·리플렉션 검증(메모리 참조). 에디터는 프로젝트당 하나.
</code_context>

<specifics>
## Specific Ideas
- 기획 문서 설계 의도: "이 보스는 회피가 아니라 판단을 요구한다." 1페이즈는 감옥 학습(위험 요소), 2페이즈는 감옥=생존 수단 + 웅덩이=위치 교란, 3페이즈는 패턴 겹침과 타이밍.
- 아트용 애니메이션 목록(패턴 1~10)이 곧 애니메이터 상태/트리거 목록이다. 코드 트리거는 이 목록과 1:1이 되게 한다.
</specifics>

<deferred>
## Deferred Ideas
- 애니메이션 클립/컨트롤러 제작, 문신 UV 셰이더, 코어 빛 제어, 피격 경직·사망 모션과 연출(화면 어두워짐, 검은 비), 하늘 어두워짐
- 보스 사망 후 처리 연출, 실제 3스테이지 씬 배치와 카메라/트리거 구성
- 물 속성 회복 규칙 재정의(문서에 없음)
</deferred>

---

*Phase: 29-watermonster-3-70-50-30*
*Context gathered: 2026-11-05 via 사용자 기획 문서 + 대화 결정*
