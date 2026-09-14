# Phase 16: 일정 간격 자동저장 - Context

**Gathered:** 2026-09-14
**Status:** Ready for planning

<domain>
## Phase Boundary

플레이 중 **일정 시간 간격으로 자동 저장**을 수행해, 체크포인트 상호작용 / 보스 격파 /
일시정지 메뉴 저장 사이에 생기는 "장시간 미저장 구간"을 없앤다.

기존 저장 트리거 3종(`SaveAtCheckpoint` / `SaveOnBossDefeated` / `SaveAnywhere`)과 슬롯 구조,
로드 경로, 세이브 스키마는 Phase 11·14·15에서 이미 확정됐고 **이번 페이즈가 바꾸지 않는다**.
이번 페이즈가 추가하는 것은 오직 "시간 기반 네 번째 저장 트리거" 하나와, 그 발생을 알리는
최소 UI다.

</domain>

<decisions>
## Implementation Decisions

### 저장 대상 슬롯/파일
- **D-01:** 자동저장은 **현재 슬롯(`CurrentSlot`)의 세이브 파일을 그대로 덮어쓴다**.
  별도 자동저장 파일(`save_N_auto.json`)이나 전용 자동저장 슬롯은 만들지 않는다.
  근거: 사망 로드(P15 D-01)와 이어하기 경로가 `CurrentSlot` 파일 하나만 읽으므로 로드 쪽을
  전혀 건드리지 않아도 되고, 슬롯 카드의 진행도 표시도 자동으로 최신이 된다.
  수동 저장은 `SaveSlotDialog`로 사용자에게 슬롯을 묻지만 **자동저장은 물을 수 없으므로,
  자동저장 시점의 슬롯은 항상 `CurrentSlot`이다** — 새 대화상자를 띄우지 않는다.
- **D-02:** 덮어쓰기 전 백업(`.bak`)을 남기지 않는다. 게임 안에서 백업을 되돌리는 UI가 없어
  파일만 늘고 쓸모가 없다. 자동저장의 안전성은 D-03(억제 조건)과 D-06(체력 미저장)이 담보한다.

### 저장 시점 조건 + 간격
- **D-03:** 자동저장은 `GameStateManager.CurrentState == GameState.Playing` 일 때만 실행한다.
  `Paused` / `Loading` / `Inventory` / `Puzzle` / `GameClear` 중에는 건너뛴다.
  기존 상태머신을 그대로 쓰므로 새 인프라가 필요 없고, 씬 로딩 중 반쪽짜리 상태를 저장하는
  사고를 막는다.
- **D-03b (가드 위치 — 절대 조건):** D-03과 D-03c의 검사는 **전부 타이머(호출자) 쪽에 둔다.**
  `SaveLoadManager.cs`와 `SaveAnywhere()`는 **한 줄도 수정하지 않는다.**
  이건 스타일 선호가 아니라 강제다: 일시정지 메뉴 저장은 `Paused` 상태에서 실행되므로
  (`PauseMenu.cs:62`, `SaveSlotDialog.cs:38`이 `SetState(Paused)` → 그 상태에서
  `GameSettingsPanel.cs:75`가 `SaveAnywhere()` 호출), `CurrentState == Playing` 게이트를
  `SaveAnywhere()` 안에 넣으면 **기존 수동 저장이 통째로 죽는다.**
  P11 D-01(매니저는 로직만 소유, 호출자가 직접 호출)과도 정확히 일치한다.
  → 이번 페이즈의 신규 코드는 **타이머 컴포넌트와 알림 UI 둘뿐**이며, 기존 저장 경로는 무수정이다.
- **D-03c (필수 가드 — D-03만으로는 부족):** `CurrentState == Playing` 검사에 더해
  **`PlayerStats.Instance != null` 을 반드시 함께 검사한다.**
  근거: `GameStateManager.HandleSceneLoaded()`(`GameStateManager.cs:79-87`)는 씬 종류를 가리지
  않고 **모든 씬 로드마다 `CurrentState`를 무조건 `Playing`으로 되돌린다.** 따라서 메인메뉴에서도
  `CurrentState == Playing`이며, 타이머가 `DontDestroyOnLoad` 객체에 살면 메인메뉴에서 자동저장이
  발동한다. 그러면 `SaveAnywhere()`가 `SceneName = "MainMenu"`를 현재 슬롯에 기록하고, 이후
  그 슬롯을 로드하면 MainMenu 씬을 게임 씬으로 로드 → `PlayerStats.Instance == null` →
  `AbortLoadToMainMenu()`로 떨어져 **슬롯 하나가 통째로 못 쓰게 된다.**
  이 가드는 선택이 아니라 필수이며, 어떤 저장 메서드를 호출하든 동일하게 필요하다.
- **D-04:** 간격은 **3분**. 사망 시 즉시 로드(P15 D-01)이므로 최악의 경우 손실이 3분치로
  제한되고, `SaveData` JSON이 작아 디스크 쓰기 부담이 사실상 없다.
- **D-05:** 수동 저장(체크포인트 / 보스 격파 / 일시정지 메뉴 저장)이 발생하면 **자동저장
  타이머를 리셋한다**. 방금 저장한 직후 또 쓰는 낭비를 막는다.
- **D-05b:** '전투 중' / '보스전 중' 억제는 두지 않는다. 프로젝트에 전역 전투 판정 플래그가
  없어 새로 만들어야 하는데, D-06 때문에 보스전 중 자동저장이 남기는 해가 없다.

### 자동저장이 기록하는 내용
- **D-06 (선행 결정 확인 — 이번 페이즈가 새로 정하는 것 아님):** `SaveData`에는 **현재 체력이
  아예 없다**. `PlayerStatsSaveData`는 `MaxHealth` / `MaxTotalHealth` 2개뿐이고,
  `SaveData.cs:21` 주석대로 "현재 체력은 휘발성, 로드는 항상 풀피 부활"이 Phase 15 정책이다.
  따라서 자동저장이 저체력 상태를 박아 사망 루프를 만드는 시나리오는 **구조적으로 불가능**하며,
  체력 하한 보정이나 저체력 시 저장 스킵 같은 장치를 만들지 않는다.
- **D-07:** 자동저장은 **`SaveAnywhere()`의 씬/스폰지점 로직을 그대로 재사용한다** — 현재 씬을
  `SceneName`에 기록하고, 마지막 체크포인트 씬과 다르면 `SpawnPointName`을 비워 씬 기본 시작
  위치로 부활시킨다. 새 좌표 저장 경로를 만들지 않는다 (P11 D-05: raw XY 금지).
- **D-07b (호출 형태 확정):** 자동저장 타이머는 **`SaveLoadManager.Instance.SaveAnywhere()` 를
  그대로 호출한다.** `SaveAuto()` 같은 전용 래퍼를 신설하지 않는다. `SaveSlotDialog`는
  호출자 쪽(`GameSettingsPanel.cs:75`)에 있지 `SaveAnywhere()` 안에 없으므로, 직접 호출해도
  대화상자가 뜨지 않는다(D-01 준수). 힐도 없다.
  D-05의 타이머 리셋은 3개 트리거가 전부 거쳐 가는 `Save()` 한 곳에 걸면 자동으로 커버된다.
- **D-07c (`Save()` 직접 호출 기각 — 검토 후 배제):** 자동저장이 `Save()`를 그대로 부르는 안을
  검토했으나 **완전한 no-op임이 확인되어 배제한다.** `Save()`는 `SceneName`/`SpawnPointName`을
  갱신하지 않고, 나머지 필드도 3분 주기로 달라질 것이 없다:
  `MaxHealth`/`MaxTotalHealth`는 유일한 변경 경로인 `PlayerStats.AddHealth()`의 **호출자가 0건**
  이라 값이 변하지 않고, `BossProgress`는 `SaveOnBossDefeated()`가 격파 즉시 이미 저장하며,
  `MapGimmickState`/`Items`는 쓰기 훅이 없는 스텁이다. 즉 직전과 동일한 JSON을 다시 쓰는 것에
  불과해 "미저장 구간 제거"라는 페이즈 목표를 전혀 달성하지 못한다.
  **자동저장이 의미를 가지려면 `SceneName` 갱신이 필수이고, 그걸 하는 기존 메서드는
  `SaveAnywhere()` 하나뿐이다.**
- **D-08:** D-07의 결과로, 체크포인트를 밟지 않은 채 다음 씬으로 넘어가 자동저장이 걸리면
  **사망 부활 지점이 마지막 체크포인트가 아니라 그 씬의 기본 시작 위치가 된다**. 이것을
  의도된 동작으로 수용한다 — 씬을 되돌리지 않으므로 진행은 앞으로만 간다.
- **D-09:** 보스전 도중에도 자동저장을 억제하지 않는다. 로드는 `LoadSceneAsync`로 씬을 통째
  재생성하므로 보스/몬스터 HP는 어차피 전부 초기화되고, 격파 기록이 있는 보스만 등장하지
  않는다(P15 D-06/D-07, 이미 3개 보스에 배선됨). 즉 보스전 중 사망은 "플레이어 풀피 + 보스
  풀피 재대결"이며 자동저장이 여기에 영향을 주지 않는다.

### 사용자 피드백 / 설정 노출
- **D-10:** 자동저장이 발생하면 **화면 구석에 짧은 텍스트를 1~2초 띄우고 페이드아웃**한다
  (예: "자동 저장됨"). 조용히 저장하면 플레이어가 진행이 보존됐는지 알 길이 없다.
  프로젝트에 범용 토스트/알림 UI가 **없으므로 이번 페이즈에서 새로 만들어야 한다**.
  `HealPopup` / `HealPopupSpawner`는 전투용 월드 스페이스 팝업이라 재사용 대상이 아니다.
- **D-11:** `setting.json`에 자동저장 on/off나 간격 설정을 **노출하지 않는다**. 자동저장은
  항상 켜진 고정 동작이다. `SettingsData` / `GameSettingsPanel`은 이번 페이즈에서 건드리지
  않는다.

### Claude's Discretion
- 자동저장 타이머를 소유할 위치 — `SaveLoadManager` 내부 `Update()`/코루틴으로 둘지, 별도
  경량 컴포넌트로 분리할지. P11 D-01("매니저가 로직 소유, 호출자가 직접 호출")과의 일관성을
  보고 계획 단계에서 정한다.
- 씬 전환 직후 타이머 처리 — 이어서 카운트할지, 새 씬에서 다시 셀지.
  (`PlayerStats.Instance` 없는 씬의 방어는 더 이상 재량이 아니다 — D-03c로 확정됐다.)
- D-05의 타이머 리셋을 `Save()` 내부에 걸지, 각 트리거 호출부에 개별로 걸지.
  전자가 3개 트리거를 한 번에 커버하지만 매니저가 타이머를 알게 된다.
- 알림 텍스트의 정확한 문구·폰트·화면 위치, 페이드 시간, 그리고 이 UI를 씬 10개에 각각 배치할지
  영속 캔버스(`PersistentManagers` 계열)에 한 번만 올릴지. 후자가 유력하나 실측 후 판단.
- 타이머 구현 방식(`Time.time` 누적 vs `WaitForSeconds` 코루틴)과 `Time.timeScale = 0`
  (일시정지) 구간 취급 — D-03이 이미 Paused를 배제하므로 어느 쪽이든 결과는 같아야 한다.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

이번 페이즈에는 별도 ADR/스펙 문서가 없다. 로드맵 항목과 아래 선행 CONTEXT가 스펙 역할을 겸한다.

### 로드맵 원문 (스펙 역할)
- `.planning/ROADMAP.md` (Phase 16 섹션, 438행) — "일정 간격 자동저장 - 플레이 중 주기적으로
  현재 진행 상황을 자동 저장해 세이브 부재/장시간 미저장 구간을 없앤다"

### 선행 페이즈 결정 (반드시 준수 — 이번 페이즈가 뒤집지 않음)
- `.planning/phases/11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json/11-CONTEXT.md`
  — D-01(매니저는 로직만 소유, 호출자가 직접 호출), D-05(좌표 복원은 `PlayerSpawner.targetSpawnPointName`
  경로만, raw XY 금지), D-03/D-03b(`MapGimmickState`·`Items` 스텁 유지).
- `.planning/phases/14-save-slot-expansion/14-CONTEXT.md` — D-06(슬롯별 독립 파일).
  D-04/D-05(확인 없는 덮어쓰기 금지)는 **사용자가 슬롯을 지우는 경우**에 대한 규칙이며,
  같은 슬롯 안에서 진행 상황을 갱신하는 자동저장(D-01)에는 적용되지 않는다.
- `.planning/phases/15-load-timing-and-load-scope/15-CONTEXT.md` — D-01(사망 시 즉시 자동 로드,
  게임오버 화면 없음) ← 자동저장 내용이 곧 부활 상태가 되는 근거. D-06/D-07(격파 보스 미등장,
  판정 주체는 보스 쪽). Deferred Ideas에 "일정 간격 자동저장 → Phase 16으로 분리"로 이 페이즈가
  예고되어 있다.

### 이 페이즈가 읽어야 할 소스 파일
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — `Save()`(138), `SaveAtCheckpoint()`(148),
  `SaveAnywhere()`(164), `CapturePlayerStats()`, `LoadGame()`(351), `CurrentSlot`/`SavePath`(117).
- `Assets/SaveSystem/Script/SaveData.cs` — 스키마 전문. 21행 주석이 "현재 체력 미저장" 정책의
  근거(D-06).
- `Assets/Player/Script/GameStateManager.cs` — `GameState` enum(7~15), `CurrentState`(19),
  `OnGameStateChange`(20). D-03의 게이트.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/SaveSystem/Script/SaveLoadManager.cs:164` `SaveAnywhere()` — D-07이 재사용할 로직 전부가
  여기 있다. 현재 씬 기록 + 씬 불일치 시 `SpawnPointName` 비우기 + `Save()` 호출.
- `Assets/SaveSystem/Script/SaveLoadManager.cs:117` `SavePath` (→ `GetSavePath(CurrentSlot)`) —
  D-01의 자동저장 대상 경로. 추가 작업 없이 그대로 맞는다.
- `Assets/Player/Script/GameStateManager.cs:19` `CurrentState` + `:20` `OnGameStateChange` —
  D-03 게이트. 폴링(매 틱 확인)과 이벤트 구독 둘 다 가능.

### ⚠️ 함정 (계획 단계에서 반드시 반영)
- `GameStateManager.HandleSceneLoaded()` (`GameStateManager.cs:79-87`)는 **씬 종류를 가리지 않고
  모든 씬 로드마다 `CurrentState`를 무조건 `Playing`으로 되돌린다.** 메인메뉴도 예외가 아니다.
  → D-03의 `Playing` 게이트만으로는 메인메뉴 자동저장을 막지 못하며, 막지 않으면 슬롯이
  `SceneName = "MainMenu"`로 오염돼 못 쓰게 된다. D-03c의 `PlayerStats.Instance` 가드가 필수인 이유.
- `PlayerStats.AddHealth()` (`PlayerStats.cs:52`) — `maxHealth`를 올리는 **유일한** 경로인데
  **호출자가 0건**이다. 즉 현재 프로젝트에서 `MaxHealth`는 플레이 중 절대 변하지 않는다.
  D-07c(`Save()` 직접 호출 기각)의 근거.
- `Save()`는 `SceneName`/`SpawnPointName`을 갱신하지 않는다 — `CapturePlayerStats()` 후
  `_data`를 있는 그대로 직렬화할 뿐이다. 자동저장에 그대로 쓰면 아무것도 새로 보존되지 않는다.
- **일시정지 메뉴 저장은 `CurrentState == Paused` 상태에서 실행된다.** `PauseMenu.cs:62`와
  `SaveSlotDialog.cs:38`이 각각 `SetState(GameState.Paused)`를 호출하고, 그 상태에서
  `GameSettingsPanel.cs:75`가 `SaveAnywhere()`를 부른다.
  → D-03의 `Playing` 게이트를 `SaveAnywhere()` 내부에 넣으면 **기존 수동 저장이 전부 막힌다.**
  가드는 반드시 타이머(호출자)에만 둘 것 (D-03b).

### 현재 상태의 사실 (실측)
- 저장 트리거는 현재 3종뿐이고 전부 명시적 사용자 행동에서만 발생한다:
  `Checkpoint.cs:10`(S키) → `SaveSlotDialog.Open(... SaveAtCheckpoint ...)`,
  `GameSettingsPanel.cs:75`(일시정지 메뉴) → `SaveSlotDialog.Open(... SaveAnywhere ...)`,
  각 보스 사망 지점 → `SaveOnBossDefeated`. **시간 기반 트리거는 0건** — 이번 페이즈가 최초.
- 수동 저장 2종은 최근 quick task `260913-git`으로 `SaveSlotDialog`를 경유하게 바뀌었다.
  자동저장은 이 대화상자를 절대 거치면 안 된다(D-01).
- `SaveAtCheckpoint()`는 살아있는 플레이어를 `ResetHealthToMax()`로 **풀회복시킨다**.
  자동저장이 이 메서드를 재사용하면 3분마다 무료 힐이 되므로 **절대 재사용 금지** — D-07이
  `SaveAnywhere` 쪽을 고른 이유 중 하나.
- `LoadGame()` → `LoadSceneAndRestoreRoutine()` → `LoadSceneAsync(SceneName)` → 씬 전체 재생성 →
  `RestoreStats(MaxHealth, MaxTotalHealth)`. 씬에 배치된 보스·몬스터 HP를 담는 필드는 스키마에
  없으므로 로드 시 **모든 적이 초기 상태로 리셋**된다 (D-09의 근거).
- `IsBossDefeated()` 소비자는 3곳으로 이미 배선돼 있다: `TutorialBossController.cs:139`,
  `WaterMonsterStats.cs:28`, `SpiritStats.cs:15`.
- **범용 토스트/알림 UI가 없다.** `Assets/Script/Combat/HealPopup.cs` /
  `HealPopupSpawner.cs`가 유일한 팝업 계열인데 전투용 월드 스페이스라 D-10에 쓸 수 없다.
- `SettingsData`에 `ScreenShake` / `TutorialHint` bool 선례가 있고 `GameSettingsPanel.cs:15-16`에
  대응 Toggle이 있다 — D-11에 따라 **이번엔 쓰지 않는다**. (설정 노출을 나중에 되살릴 때의 경로)

### Established Patterns
- 싱글톤: `public static X Instance` + `Awake()` null 체크 + `DontDestroyOnLoad`
  (`SaveLoadManager`, `GameManager`, `GameStateManager` 동일 형태).
- 디스크 쓰기는 명시적 트리거에서만, 플레이 중에는 메모리 캐시만 사용 (P11) —
  **D-04가 이 관행에 시간 기반 트리거를 처음으로 추가한다.** 계획 단계에서 이 변화를 명시할 것.
- 코루틴 기반 (`IEnumerator` / `StartCoroutine`). 프로젝트에 `async`/`Task` 사용처 없음.

### Integration Points
- 자동저장 타이머 → `SaveAnywhere()` 동등 로직 호출 (D-07)
- `GameStateManager.CurrentState` → 타이머의 실행 게이트 (D-03)
- 기존 저장 트리거 3종 → 타이머 리셋 신호 (D-05)
- 신규 알림 UI (아직 없음) → 자동저장 성공 시 표시 (D-10)

</code_context>

<specifics>
## Specific Ideas

- "자동저장은 슬롯을 물어볼 수 없다" — 수동 저장 2종이 `SaveSlotDialog`를 거치도록 바뀐 직후라,
  자동저장만 `CurrentSlot`에 조용히 쓴다는 비대칭이 이 페이즈의 핵심 구조다 (D-01).
- 논의 중 "저체력 자동저장 → 사망 루프" 위험을 검토했으나, `SaveData`에 현재 체력 필드 자체가
  없어 **구조적으로 발생 불가**임을 코드에서 확인하고 관련 방어 장치를 전부 뺐다 (D-06).
- 사망 후 보스/몬스터 체력이 유지되는지 확인한 결과 **씬 재로드로 전부 리셋**됨 —
  이 사실이 "보스전 중 자동저장 억제 불필요"(D-09)의 직접 근거가 됐다.
- 진행이 보존됐다는 신호는 필요하지만 설정으로 끄고 켜는 것까지는 필요 없다는 판단
  (D-10 채택 + D-11 기각).

</specifics>

<deferred>
## Deferred Ideas

- **자동저장 on/off 및 간격을 설정 화면에 노출** — D-11에서 기각. 필요해지면 `SettingsData`에
  bool 하나 추가 + `GameSettingsPanel` Toggle 추가로 끝나는 작은 작업이다.
- **자동저장 별도 파일 / 전용 슬롯** — D-01에서 기각. 수동 세이브를 절대 보호해야 할 이유가
  생기면 재검토.
- **'전투 중' 전역 판정 플래그** — D-05b에서 불필요로 결론. 다른 기능(예: 전투 중 회복 금지,
  전투 BGM 전환)이 요구하면 그때 별도로 도입할 것.
- **범용 토스트/알림 시스템** — D-10은 자동저장 알림 하나만 만든다. 다른 알림(아이템 획득,
  퀘스트 등)까지 받는 범용 시스템으로 일반화하는 것은 이번 범위 밖.
- **맵 기믹 상태 저장 훅** — P15 D-10에서 스텁 유지로 결정, 여전히 유효.

### Reviewed Todos (not folded)
None — `todo match-phase 16` 결과 매칭되는 todo 0건.

</deferred>

---

*Phase: 16-interval-autosave*
*Context gathered: 2026-09-14*
