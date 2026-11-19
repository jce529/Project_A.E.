# Phase 15: 로드 시점 및 로드 범위 정의 - Context

**Gathered:** 2026-09-10
**Status:** Ready for planning

<domain>
## Phase Boundary

세이브 데이터가 **언제** 로드되는지(트리거)와 로드가 **무엇까지** 복원하는지(범위)를 확정하고 구현한다.

저장 트리거(체크포인트 / 보스 격파 / 일시정지 메뉴 저장)와 슬롯 구조는 Phase 11·14에서 이미
확정됐으므로 이번 범위 밖이다. "이어하기" 진입 경로도 Phase 14 D-01에서 확정됐다
(항상 슬롯 선택 화면 경유) — 따라서 이번 페이즈가 새로 정하는 로드 시점은 **플레이어 사망**과
**로드 실패 폴백**이다.

</domain>

<decisions>
## Implementation Decisions

### 로드 시점 — 플레이어 사망
- **D-01:** 플레이어 HP가 0이 되면 **즉시 마지막 세이브를 자동 로드**한다. 게임오버 화면이나
  사용자 선택 단계를 두지 않는다. 기존 `SaveLoadManager.LoadGame()` 경로를 그대로 재사용하므로
  씬 재로드 + 스폰 복원 + 스탯 복원이 한 번에 처리되고, `GameStateManager`에 새 상태를
  추가하거나 씬마다 사망 UI를 배치할 필요가 없다.
- **D-02:** 세이브 파일이 아직 없는 상태(체크포인트를 한 번도 활성화하지 않음)에서 죽으면
  **현재 씬을 처음부터 다시 시작**한다. 메인메뉴로 내보내지 않는다 — 튜토리얼 초반에 죽어도
  게임이 이어지는 흐름을 유지하는 것이 목적.
- **D-03:** 낙하(`FallZone`)는 **현행 유지** — 데미지를 주고 `PlayerRespawn.RespawnPosition()`으로
  위치만 되돌리며 세이브를 읽지 않는다. 낙하는 '사망'이 아니라 페널티로 취급한다. 낙하 데미지로
  HP가 0이 되면 그때 D-01의 사망 로드 경로로 자연히 넘어간다. `FallZone.cs`는 무수정.

### 로드 실패 처리
- **D-04:** 로드가 어느 단계에서든 실패하면(세이브 파일 손상, `SceneName`이 Build Settings에서
  해석 불가, 씬 로드 후 `PlayerStats.Instance`가 null) **메인메뉴로 돌려보낸다**. 현재처럼
  `Debug.LogError` 후 조용히 `return`해서 사용자가 아무 반응도 못 받는 상태로 남기지 않는다.
- **D-05:** 로드 실패 시 **세이브 파일을 지우거나 덮어쓰지 않는다**. 수동 복구 여지를 남긴다.
  (Phase 14 D-05의 "확인 없는 즉시 덮어쓰기 경로는 만들지 않는다"와 같은 방향.)

### 로드 범위 — 보스 진행도
- **D-06:** 씬 로드 시 `BossProgress`를 읽어 **이미 격파된 보스는 등장시키지 않는다**. 현재
  `SaveOnBossDefeated()`는 기록만 하고 `IsBossDefeated()`를 읽는 곳이 0건이라 잡은 보스가
  로드 후 부활하는 상태다. 보스는 씬에 배치된 오브젝트이므로 "로드하지 않는다"는 실질적으로
  씬 로드 직후 해당 보스를 등장시키지 않는다는 의미다.
- **D-07:** 판정 주체는 **보스 쪽**이다. 로드 경로가 보스 목록을 알고 순회하는 구조가 아니라,
  각 보스가 자기 ID로 `SaveLoadManager.Instance.IsBossDefeated()`를 조회해 스스로 물러난다.
  Phase 11 D-01(매니저는 로직만 소유, 호출자가 직접 호출)의 방향을 그대로 따른다.

### 로드 범위 — 체력 불변식
- **D-08:** 로드 시 `0 < health <= maxHealth <= maxTotalHealth` 불변식을 **강제 보정**한다.
  저장값을 무조건 신뢰하지 않는다. 이미 역전된 값이 든 기존 세이브도 로드 시 정상화되고,
  손으로 고친 JSON도 방어된다.
- **D-09:** `Player.prefab`의 체력값을 **시작 `maxHealth` 100 / 성장 상한 `maxTotalHealth` 200**
  으로 확정한다. 실제 새 게임 진입 씬인 `Tutorial Map`이 이미 `maxHealth`를 100으로
  오버라이드하고 있어 그 값에 맞추는 것이고, `PlayerHealthUI`의 하트 생성(상한/5 = 40개)과
  활성화(시작 시 20개) 규칙도 일관되게 유지된다. 프리팹 기본값을 고치는 결과로 나머지 9개
  스테이지 씬의 실제 시작 체력이 400에서 100으로 낮아진다 — 의도된 변경이다.

### 로드 범위 — 제외 항목
- **D-10:** `MapGimmickState`와 `Items`는 **스텁으로 유지**한다. 두 필드 모두 쓰기 훅 자체가
  없어(P11 D-03 / D-03b) 복원할 데이터가 존재하지 않는다. 저장 쪽이 먼저 생겨야 복원을
  논할 수 있으므로 이번 페이즈 범위 밖임을 명시한다.

### Claude's Discretion
- 사망을 감지해 로드를 거는 정확한 훅 지점. `PlayerStats`가 `HP.Die()`를 override할지,
  기존 `HP.OnDeath` 이벤트를 구독할지, `ManualDeath` 플래그를 켜서 `Destroy(gameObject)`를
  막을지는 계획 단계에서 정한다. 다만 **현재 상태(`ManualDeath=0`이라 플레이어가 파괴됨)를
  그대로 두면 안 된다**는 것은 확정이다.
- 격파된 보스를 "등장시키지 않는" 구체적 방법(`SetActive(false)` / `Destroy` / 스폰 차단)과,
  보스 주변 오브젝트(보스방 문, `BossHealthBarController` 등)를 함께 처리할지 여부.
- 불변식 보정의 정확한 보정 규칙(어느 값을 기준으로 어느 값을 끌어당길지)과 보정 발생 시
  로그를 남길지 여부.
- 메인메뉴 복귀에 사용할 씬 이름 상수의 위치.
- 세이브 없이 죽었을 때의 "현재 씬 재시작"이 `SceneManager.LoadScene(현재 씬)`인지
  `LoadSceneAsync`인지 — 기존 로드 경로와의 일관성을 보고 판단.

</decisions>

<specifics>
## Specific Ideas

- "씬을 로드할때 보스 프로그래스를 읽고서, Defeat된 보스는 로드하지 말자" — 격파 기록을
  읽는 시점이 씬 로드 시점이라는 점이 명시됐다 (D-06).
- "아예 세이브데이터가 없으면 씬을 다시시작" — 세이브 부재를 예외/에러가 아니라 정상 흐름으로
  처리한다 (D-02).
- 낙하는 '사망'과 다른 층위의 페널티라는 구분이 유지된다 (D-03).

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

이번 페이즈에는 별도 ADR/스펙 문서가 없다. 로드맵 항목과 아래 선행 CONTEXT·버그 문서가
스펙 역할을 겸한다.

### 로드맵 원문 (스펙 역할)
- `.planning/ROADMAP.md` (Phase 15 섹션) — 로드 시점(사망/체크포인트 부활/이어하기)과
  로드 범위(보스 진행도/맵 기믹/체력 불변식)라는 이번 페이즈의 두 축이 제목에 잠겨 있음.

### 선행 페이즈 결정 (반드시 준수)
- `.planning/phases/11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json/11-CONTEXT.md`
  — D-01(매니저가 로직 소유, 호출자가 직접 호출), D-05(좌표 복원은 `PlayerSpawner.targetSpawnPointName`
  경로 재사용), D-06(새 게임은 메모리만 리셋), D-03/D-03b(기믹·아이템 스텁).
- `.planning/phases/14-save-slot-expansion/14-CONTEXT.md` — D-01(이어하기는 항상 슬롯 선택
  화면 경유 → 이번 페이즈가 이어하기 시점을 다시 정하지 않는 근거), D-05(확인 없는 즉시
  덮어쓰기 경로 금지), D-06(슬롯별 독립 파일).

### 버그 문서 (이 페이즈가 해소할 항목)
- `bug/BUG-002-player-health-bounds-inverted.md` — 체력 역전 현황과 씬별 오버라이드 실측.
  D-08 / D-09가 이 문서를 닫는다.
- `bug/BUG-003-watermonster-not-wired.md` — "Phase 15 이관" 섹션. D-06 / D-07이 이 문서의
  보스 진행도 복원 부분만 닫는다. **보스 프리팹 제작과 씬 배치는 이 페이즈 범위가 아니다.**

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/SaveSystem/Script/SaveLoadManager.cs:302` `LoadGame()` — 로드 진입점. D-01의 사망
  로드가 그대로 재사용할 대상. 내부적으로 `LoadSceneAndRestoreRoutine()` → `LoadSceneAsync` →
  `ApplyPlayerStatsFromSave()` 순으로 진행한다.
- `Assets/SaveSystem/Script/SaveLoadManager.cs:178` `IsBossDefeated(string)` — 이미 존재하지만
  **호출자가 0건**. D-06/D-07이 이 메서드의 첫 소비자를 만든다.
- `Assets/Player/Script/PlayerStats.cs:70` `RestoreStats()` — D-08 불변식 보정이 들어갈 지점.
  `health`/`maxHealth`가 `protected`, `maxTotalHealth`가 `private`이라 외부에서 손댈 통로가
  여기뿐이다.
- `Assets/map/script/PlayerSpawner.cs` — static `targetSpawnPointName` + `ApplySpawn()`.
  P11 D-05가 채택한 유일한 좌표 복원 경로.

### 현재 상태의 문제점 (실측)
- `PlayerStats`는 `HP.Die()`를 override하지 않고, `Player.prefab`의 `ManualDeath: 0`을
  **어떤 씬도 오버라이드하지 않는다** → 사망 시 `HP.Die()`가 `Destroy(gameObject)`를 실행해
  플레이어 오브젝트가 사라진다. `HP.OnDeath` 구독자도 0건.
- `LoadGame()`의 실제 호출자는 `Assets/Script/SlotSelectPanel.cs:106`(→ `LoadSlot`) 하나뿐이다.
  `SaveLoadManager.cs:300` 주석이 주장하는 "체크포인트 부활" 경로는 배선돼 있지 않다.
- `SaveOnBossDefeated()` 호출부는 코드 주석이 말하는 4곳이 아니라 **3곳**이다:
  `TutorialBossController.cs:304`, `WaterMonsterStats.cs:77`, `SpiritStats.cs:67`.
  `Assets/Enemy/Boss` 계열(우드보스)은 호출하지 않는다. `WaterMonsterStats`의 호출부는 보스가
  어떤 씬에도 배치돼 있지 않아 현재 죽은 코드다 (BUG-003).
- `GameStateManager.GameState`에는 `Playing/Paused/Inventory/Loading/GameClear/Puzzle`만 있고
  사망 상태가 없다. D-01이 게임오버 UI를 만들지 않기로 한 이유 중 하나.

### Established Patterns
- 싱글톤: `public static X Instance` + `Awake()` null 체크 + `DontDestroyOnLoad`
  (`SaveLoadManager`, `GameManager`, `GameStateManager` 동일 형태).
- 씬 전환: 프로젝트 대부분은 동기 `SceneManager.LoadScene`. 비동기 `LoadSceneAsync`는
  `SaveLoadManager`의 로드 경로에만 존재한다.
- `PlayerStats.TakeDamage()`가 `CameraController.Instance.Shake()`를 null 가드 없이 호출하는
  선례가 있다 (Phase 12 주석 참고) — 싱글톤 접근에 대한 프로젝트 관행.

### Integration Points
- 사망 감지 지점 → `LoadGame()` 호출 (D-01)
- 각 보스의 초기화 지점 → `IsBossDefeated()` 조회 후 자기 비활성화 (D-07)
- `PlayerStats.RestoreStats()` → 불변식 보정 (D-08)
- `Assets/Player.prefab` `health`/`maxHealth`/`maxTotalHealth` 직렬화 값 (D-09)
- `SaveLoadManager.LoadGame()`의 4개 실패 분기 → 메인메뉴 복귀 (D-04)

</code_context>

<deferred>
## Deferred Ideas

- **일정 간격 자동저장** — 사용자가 "세이브가 없어서 로드할 게 없는 상황" 자체를 줄이자는
  취지로 제안. 저장 시점을 늘리는 새 기능이라 로드 범위인 이번 페이즈 밖이다.
  **Phase 16으로 분리해 로드맵에 추가하기로 결정.**
- **WaterMonster 보스 프리팹 제작 및 씬 배치** — BUG-003의 본체. 프리팹 참조 7개와 씬 소속
  `BoxCollider2D _battleAreaBounds` 배선이 필요해 규모가 크다. 별도 페이즈.
- **게임오버 화면 / 사망 연출** — D-01이 즉시 로드를 택하면서 보류. 나중에 연출을 넣고 싶으면
  `GameStateManager`에 사망 상태를 추가하는 형태가 될 것이다.
- **맵 기믹 상태 저장 훅** — D-10에서 스텁 유지로 결정. 저장 쪽이 먼저 생겨야 복원이 가능하다.

### Reviewed Todos (not folded)
None — `todo match-phase 15` 결과 매칭되는 todo 없음.

</deferred>

---

*Phase: 15-load-timing-and-load-scope*
*Context gathered: 2026-09-10*
