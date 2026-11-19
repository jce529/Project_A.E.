# Phase 2: 날씨 시스템 및 물 웅덩이 상호작용 - Context

**Gathered:** 2026-04-10
**Status:** Ready for planning

<domain>
## Phase Boundary

보스 HP가 70% 이하로 떨어질 때 맵 전체에 비가 내리기 시작하고, 랜덤 위치에 `WaterPuddle` 오브젝트가 주기적으로 스폰된다. 플레이어는 '물 가르기'(`WaveSlice`)로 Destructible 웅덩이를 파괴하거나, 신규 Interact 입력으로 웅덩이를 흡수해 플레이어 수분을 회복하고 해당 웅덩이를 Indestructible 상태로 전환한다. `PuddleStackManager`가 Indestructible 웅덩이 개수를 중앙에서 카운팅한다.

**이번 페이즈 포함하지 않음:** 연쇄 AoE 폭발 실행 (Phase 3 소관), 보스 순간이동 (Phase 3), 광폭화 (Phase 4).

</domain>

<decisions>
## Implementation Decisions

### Phase 2 진입 트리거
- **D-01:** 보스 HP ≤ 70% 도달 시 `WeatherController`를 활성화하여 비 시작. 임계치는 인스펙터에서 튜닝 가능하게 하되 기본값 70%.
- **D-02:** 페이즈 전환 전용 연출(화면 플래시, 보스 대사 등) 없음 — 비 ParticleSystem이 즉시 켜지는 것으로 Phase 2 진입을 알림.
- **D-03:** 트리거 구조: `WaterMonsterController` 가 `WaterMonsterStats.OnDamageTaken` 이벤트를 구독하고, HP 임계치 도달 여부를 체크해 `WeatherController.StartRain()` 호출. 한 번만 트리거 (bool 가드).

### WeatherController & 비 이펙트
- **D-04:** `WeatherController`는 보스 오브젝트와 분리된 독립 컴포넌트 (씬에 별도 GameObject로 배치). REQ-WM-P2-01 준수.
- **D-05:** 비 이펙트는 간단한 **Unity ParticleSystem** 기반 — 복잡한 셰이더/동적 이펙트 없음.
- **D-06:** 비가 내리는 맵 커버리지: 씬에 **BoxCollider2D (isTrigger)** 를 인스펙터에서 지정하여 범위 고정. WeatherController가 이 BoxCollider2D의 bounds를 읽어 ParticleSystem Shape 영역으로 사용.
- **D-07:** WeatherController는 비 시작/정지 두 가지 상태만 필요. `StartRain()` / `StopRain()` 메서드로 제어.

### WaterPuddle 스포너
- **D-08:** REQ-WM-P2-02 준수 — 비 시작 시 `PuddleSpawner`가 맵 랜덤 위치에 `WaterPuddle` 프리팹을 주기적으로 Instantiate. 스폰 간격/최대 개수는 인스펙터 튜닝 가능.
- **D-09:** 스폰 위치는 WeatherController의 BoxCollider2D bounds 내 랜덤 좌표.

### WaterPuddle 상태 구조
- **D-10:** `WaterPuddle` 컴포넌트는 `bool isDestructible` 필드로 상태 구분 (true = Destructible, false = Indestructible). 단순 bool — 2개 상태이므로 enum 불필요.
- **D-11:** Indestructible 전환 시 시각적 구분은 `SpriteRenderer.color` 변경 (예: 반투명한 다른 색) 또는 별도 Sprite 전환. 구체 색상/스프라이트는 플래너가 기존 에셋 확인 후 결정.
- **D-12:** WaterPuddle에 **"WaterPuddle" 태그** 부착. WaveSlice의 `OverlapCircleAll` 루프에서 `CompareTag("WaterPuddle")` 체크를 추가하여 감지.
- **D-13:** WaveSlice의 WaterPuddle 파괴 처리 순서:
  1. `CompareTag("WaterPuddle")` 로 웅덩이 감지
  2. `WaterPuddle.isDestructible` 확인 — false면 무시
  3. true면 `PuddlePool.Return(puddle)` (Disable) + 파괴 VFX/사운드

### Object Pool (WaterPuddle)
- **D-14:** WaterPuddle은 Destroy 대신 **Object Pool** 방식 사용 (Disable → 재사용). 많은 웅덩이가 스폰/파괴될 때 성능 최적화. `PuddlePool` 또는 `PuddleSpawner`에 풀 로직 내장.

### 흡수 상호작용 (Absorb)
- **D-15:** InputHandler에 **신규 `OnInteractEvent` (Action)** 추가. `.inputactions` 파일에 Player 맵 내 "Interact" 액션 신규 추가. 키 바인딩은 플래너가 현재 .inputactions 파일을 확인하고 미사용 키로 설정 (권장: E 키).
- **D-16:** 흡수 범위 판정: `WaterPuddle`에 **CircleCollider2D (isTrigger)** 부착. 플레이어가 Trigger 범위에 진입하면 `WaterPuddle.playerInRange = true` 설정. 플레이어가 OnInteractEvent 발생 시 `playerInRange` 가 true인 웅덩이를 흡수.
- **D-17:** 흡수 실행 결과 (REQ-WM-P2-04):
  1. 플레이어 수분(Water 자원) 회복 — `WaterController.AddWater(amount)` 호출
  2. 해당 WaterPuddle → `isDestructible = false` (Indestructible 전환)
  3. `PuddleStackManager.RegisterIndestructible(puddle)` 호출

### PuddleStackManager
- **D-18:** `PuddleStackManager` 싱글턴 또는 씬 GameObject — Indestructible 웅덩이 개수를 `int _indestructibleCount` 로 중앙 카운팅.
- **D-19:** 임계치 도달 시 `OnThresholdReached` 이벤트 발화. 실제 연쇄 AoE 폭발 구현은 Phase 3 소관 — Phase 2에서는 이벤트 훅까지만.
- **D-20:** Destructible 웅덩이가 파괴될 때는 카운트 영향 없음 (Indestructible만 카운팅).

### Claude's Discretion
다음 항목은 리서처/플래너가 코드와 기존 에셋을 보고 결정:
- WaterPuddle Indestructible 시각 구분 구체 색상/스프라이트 (기존 에셋 확인 후)
- Interact 키 바인딩 구체 키 값 (.inputactions 미사용 키 확인 후)
- PuddlePool 구현 위치 (PuddleSpawner 내장 vs 별도 PuddlePool MonoBehaviour)
- WaterPuddle CircleCollider2D 흡수 반경 수치 (인스펙터 튜닝으로 위임)
- WaveSlice 파괴 VFX 구체 에셋 (기존 이펙트 프리팹 재사용 or 신규)

### Folded Todos
없음

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 결정사항
- `.planning/PROJECT.md` — 상속 기반 확장 원칙, 레이어 분리 원칙
- `.planning/REQUIREMENTS.md` — REQ-WM-P2-01~05, REQ-WM-X-01 (Layer Damage)
- `.planning/ROADMAP.md` §`Phase 2` — Goal, Success Criteria, 의존성

### Phase 1 결정사항 (이어받음)
- `.planning/phases/01-boss-core-mechanics/01-CONTEXT.md` — DamageInfo/DamageElement, WaterMonsterController/Stats 구조, 속성 태그 시스템

### 베이스 코드 (상속/재사용 대상)
- `Assets/Enemy/NewBoss/Script/BossController.cs` — State 전환, 이벤트 핸들러 구조
- `Assets/Enemy/NewBoss/Script/BossStatsSystem.cs` — OnDamageTaken 이벤트 (Phase 2 진입 트리거에 활용)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — D-03 진입 트리거 추가 대상
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — HP 임계치 체크 대상

### 플레이어 스킬 & 입력
- `Assets/Player/Script/SkillScript/WaveSlice.cs` — D-12/D-13 WaterPuddle 감지 로직 추가 대상
- `Assets/Player/Script/InputHandler.cs` — D-15 OnInteractEvent 추가 대상
- `Assets/Player/Script/UI/WaterUIController.cs` — 흡수 시 수분 회복 연동 확인

</canonical_refs>

<specifics>
## Specific Ideas

- Phase 2 진입은 단발성 (once) — WaterMonsterController에 `bool _phase2Triggered` 가드 필요.
- InputHandler의 신규 Interact 액션은 다른 상호작용(문 열기, 아이템 줍기 등)에도 범용으로 활용될 수 있어 `OnInteractEvent` 로 일반적 이름 부여.
- BoxCollider2D 범위 지정 방식은 맵 크기가 바뀌어도 인스펙터만 수정하면 되어 WeatherController와 PuddleSpawner 모두 동일한 BoxCollider2D를 공유하면 편리.

</specifics>

<deferred>
## Deferred Ideas

- **연쇄 AoE 폭발 구현** — Phase 3 소관. PuddleStackManager.OnThresholdReached 이벤트 훅만 Phase 2에서 준비.
- **보스 순간이동 패턴** — Phase 3 소관.
- **광폭화 모드** — Phase 4 소관.
- **WaterPuddle 흡수 애니메이션/사운드** — Claude's Discretion (플래너 단계).
- **비 사운드 (Rain ambience)** — Claude's Discretion (플래너 단계).
- **임계치 도달 시 경고 UI** (웅덩이 개수 표시) — Phase 3 연계 시 고려.

</deferred>

---

*Phase: 02-weather-puddle-interaction*
*Context gathered: 2026-04-10 via /gsd:discuss-phase*
