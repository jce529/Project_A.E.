# Phase 11: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 - Context

**Gathered:** 2026-08-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Newtonsoft.Json 기반 싱글톤 `SaveLoadManager`(DontDestroyOnLoad)를 신규 구현한다. 플레이 중에는 메모리 캐시만 사용하고 파일 I/O는 발생하지 않으며, 저장은 체크포인트 상호작용 시점과 보스 격파 시점에만 트리거된다. 로드는 이어하기(게임 시작) 또는 체크포인트 부활 시점에 발생한다. 데이터 모델은 씬+좌표, 플레이어 스탯 하위클래스, 보스 진행도 Dictionary, 맵 기믹 상태 Dictionary, 아이템 목록으로 확장 가능하게 구성한다. 씬 전환은 비동기(LoadSceneAsync)로 수행하고, 로드 완료 후 좌표를 이동시킨다. 저장 파일은 `Application.persistentDataPath`에 `.json`으로 기록한다.

이번 페이즈는 매니저(백엔드 로직)만 다룬다. 메인 메뉴 "이어하기" 버튼 등 UI 연동은 범위 밖이다.

</domain>

<decisions>
## Implementation Decisions

### 저장 트리거 통합
- **D-01:** `SaveLoadManager`는 저장/로드 핵심 로직만 소유한다. 별도의 이벤트 버스/브로드캐스트 레이어를 신설하지 않고, `Checkpoint.cs`의 체크포인트 활성화 로직과 각 보스의 `OnDeath` 이벤트 핸들러에서 `SaveLoadManager.Instance.Save()`를 직접 호출하는 방식으로 최소 변경 통합한다.

### 슬롯 구조
- **D-02:** 단일 세이브 슬롯만 지원한다 (`Application.persistentDataPath`에 파일 1개, 예: `save.json`). 다중 슬롯 UI/구조는 범위 밖.

### 데이터 스키마 범위
- **D-03:** 보스 진행도 Dictionary와 맵 기믹 상태 Dictionary는 이번 페이즈에서 최소 스텁만 구현한다. 프로젝트에 현재 지속 상태를 가지는 맵 기믹이 거의 없으므로, 딕셔너리 구조(예: `Dictionary<string, bool>`)만 존재하면 되고 실제로 채워 넣는 항목 수는 최소화한다. 스키마 확장은 후속 페이즈 몫.
- **D-03b:** 아이템 목록도 동일하게 빈 리스트/스텁으로 구현한다 — 프로젝트에 아이템/인벤토리 시스템 자체가 아직 없음.
- **D-03c:** 플레이어 스탯 하위클래스에 담을 정확한 필드 구성은 Claude 재량. `PlayerStats : HP`(`Assets/Player/Script/PlayerStats.cs`)에 실제 존재하는 `health`/`maxHealth`/`maxTotalHealth` 필드가 출발점이며, 연구/계획 단계에서 재확인한다.

### UI 범위
- **D-04:** 이번 페이즈는 `SaveLoadManager`의 공개 API(예: `SaveGame()`, `LoadGame()`, `HasSaveFile()`, `NewGame()`)만 구현한다. `MainMenuUI.cs`에 "이어하기" 버튼을 추가/연결하는 작업, 세이브 파일 유무에 따른 UI 분기는 범위 밖 — 후속 페이즈에서 처리.
- 이 범위 결정에 따라 비동기 씬 로드 중 별도 로딩 화면 UI도 이번 페이즈에서 만들지 않는다 (Claude's Discretion 항목 참고).

### 좌표 복원 / 씬 전환 통합
- **D-05:** 로드 후 좌표 복원은 기존에 실제로 동작 중인 `PlayerSpawner.targetSpawnPointName`(static) → `PlayerSpawner.ApplySpawn()` 경로를 재사용한다. `Portal.cs` → `GameManager.Instance.NextSpawnPointName` 경로는 아무 데서도 읽히지 않는 기존 고아 코드이며, 이번 페이즈에서 되살리거나 수정하지 않는다 (기존 코드 존중 원칙 — 언급만, 삭제/수정 금지).

### 새 게임 처리
- **D-06:** 단일 슬롯 구조에서 "새 게임" 시작 시 기존 세이브 파일을 즉시 덮어쓰지 않는다. 메모리 상 데이터만 기본값으로 리셋하고, 실제 파일 덮어쓰기는 다음 `Save()` 트리거(체크포인트 상호작용 또는 보스 격파) 시점에만 발생한다.

### Claude's Discretion
- 정확한 `PlayerStats` 저장 필드 구성 (D-03c) — 연구/계획 단계에서 재확인.
- 비동기 씬 로드 중 로딩 화면 UI 유무 — UI가 범위 밖(D-04)이므로 로딩 화면 없이 코루틴/async 흐름만 구현하는 것이 기본 방향.
- 보스 진행도/맵 기믹 Dictionary의 정확한 키 네이밍 규칙(보스 ID, 기믹 ID 문자열 등).
- Newtonsoft.Json 직렬화 세부 설정(들여쓰기, null 처리, 타입 핸들링 등).

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

이번 페이즈에는 별도 ADR/스펙 문서가 없다. 로드맵 항목 자체가 상세 스펙 역할을 겸한다.

### 로드맵 원문 (스펙 역할)
- `.planning/ROADMAP.md` (Phase 11 섹션, "### Phase 11: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 ...") — Newtonsoft.Json, DontDestroyOnLoad, 메모리 캐싱, 로드/저장 시점, 데이터 클래스 구조, 비동기 씬 로드, persistentDataPath 등 핵심 제약이 전부 이 한 줄 설명에 잠겨 있음.

No external specs — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/map/script/GameManager.cs` — 기존 `DontDestroyOnLoad` 싱글톤 패턴(Awake null-check + Destroy else) 참고용. `NextSpawnPointName` 프로퍼티는 아무 데서도 읽히지 않는 고아 코드 — 재사용하지 말 것.
- `Assets/map/script/PlayerSpawner.cs` — static `targetSpawnPointName` + `ApplySpawn()`이 실제로 동작하는 유일한 좌표 복원 경로 (D-05).
- `Assets/map/script/Checkpoint.cs` — S키 체크포인트 활성화 로직. 여기에 `SaveLoadManager.Instance.Save()` 직접 호출을 추가 (D-01).
- `Assets/Enemy/*/*StatsSystem.cs` (`BossStatsSystem`, `SpiritStats`, `WoodBossStatSystem` 등)의 `OnDeath` 이벤트 — 보스 격파 자동저장 훅 포인트 (D-01).
- `Assets/Player/Script/PlayerStats.cs` (`PlayerStats : HP`) — 플레이어 스탯 하위클래스 데이터 소스 (`health`, `maxHealth`, `maxTotalHealth`).
- `com.unity.nuget.newtonsoft-json` (버전 3.2.1, `packages-lock.json`에 기록) — 이미 `Library/PackageCache`에 존재하는 간접 의존성. `manifest.json`에 직접 등록되어 있지 않음 — 다른 패키지가 제거되면 함께 사라질 잠재 위험. 프로젝트에 `.asmdef` 파일이 하나도 없어 기본 어셈블리에서 바로 `using Newtonsoft.Json;` 사용 가능.

### 이중화된 기존 씬 전환 시스템 (주의)
- `Portal.cs` → `GameManager.Instance.NextSpawnPointName` (고아, 아무 데서도 안 읽힘)
- `SignpostPortal.cs` → `PlayerSpawner.targetSpawnPointName` (실사용, D-05가 채택한 경로)

### Established Patterns
- 싱글톤 패턴: `public static X Instance`, `Awake()`에서 null 체크 후 `DontDestroyOnLoad(gameObject)`, 아니면 `Destroy(gameObject)` (GameManager, GameStateManager와 동일 형태 유지 권장).
- 모든 기존 씬 전환은 동기 `SceneManager.LoadScene` — 이번 페이즈가 프로젝트 최초의 `LoadSceneAsync` 도입이 된다.

### Integration Points
- `Checkpoint.cs` (S키 핸들러) → `SaveLoadManager.Instance.Save()`
- 각 보스 컨트롤러의 `OnDeath` 핸들러 → `SaveLoadManager.Instance.Save()`
- `PlayerSpawner.ApplySpawn()` → 로드 흐름의 좌표 복원 대상
- `MainMenuUI.cs` `OnClickStart()` — 이번 페이즈에서 건드리지 않음 (UI 범위 밖, D-04)

</code_context>

<specifics>
## Specific Ideas

- "저장기능은 매니저만 보유, 나머지는 전부 호출로" — Checkpoint/Boss가 매니저를 직접 호출하는 구조. 매니저가 Checkpoint/Boss 이벤트를 구독하는 옵저버 구조가 아님 (D-01).
- "기존 파일 유지, 새 게임은 메모리만 초기화" — 새 게임 시작이 파일을 즉시 덮어쓰지 않음 (D-06).

</specifics>

<deferred>
## Deferred Ideas

- 메인 메뉴 "이어하기" 버튼 UI 연동, 세이브 파일 존재 여부에 따른 버튼 활성화/비활성화 — 후속 페이즈.
- 다중 세이브 슬롯 지원 — 필요 시 후속 페이즈.
- `GameManager.NextSpawnPointName` 고아 코드 정리 — 이번 페이즈 범위 아님, 언급만 (건드리지 않음).

### Reviewed Todos (not folded)
None — no matching todos found for this phase.

</deferred>

---

*Phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json*
*Context gathered: 2026-08-09*
