# Phase 14: 키바인딩 keybind.json 저장 + SaveLoadManager 위임 - Context

**Gathered:** 2026-08-20
**Status:** Ready for planning

<domain>
## Phase Boundary

현재 `InputHandler.cs`는 사용자 키 리바인딩을 `PlayerPrefs`에 저장한다
(`SaveBindingOverrides()` / `LoadBindingOverrides()`, `ControlsSettingsPanel.cs`의
리바인딩 완료·초기화 콜백에서 호출됨). 이 저장 매체를 `Application.persistentDataPath`의
`keybind.json` 파일로 교체하고, 그 파일 I/O를 이미 `save.json`을 관리 중인
`SaveLoadManager`(Phase 11)에 위임한다.

새 기능 추가가 아니라 기존 저장 매체(PlayerPrefs → 파일)와 저장 책임 소재
(InputHandler → SaveLoadManager)를 바꾸는 리팩토링 성격의 phase.

</domain>

<decisions>
## Implementation Decisions

### 위임 범위 (Delegation Boundary)

- **D-01:** SaveLoadManager에는 **파일 I/O만** 위임한다. `InputHandler`는 계속
  Unity Input System API(`inputActions.SaveBindingOverridesAsJson()` /
  `LoadBindingOverridesFromJson(json)`)로 바인딩을 JSON **문자열**로
  직렬화/역직렬화하는 책임을 유지한다. `SaveLoadManager`는 그 문자열을 받아
  실제 `File.WriteAllText`/`File.ReadAllText`만 수행하는 새 공개 메서드
  (예: `SaveKeybindings(string json)` / `string LoadKeybindings()`)를 추가한다.
  `InputHandler`가 `InputActionAsset`을 직접 다루는 구조는 그대로 유지되고,
  `SaveLoadManager`는 입력 시스템 세부사항을 알 필요가 없다.
- **D-02 (프로젝트 원칙, 이번 phase에서 확립):** "디스크에 JSON 파일을 쓰는 코드는
  프로젝트 전체에서 `SaveLoadManager.cs` 한 곳에만 존재한다"는 원칙을 keybind.json에도
  적용한다. 코드베이스 스캔 결과 현재 `File.WriteAllText`/`File.ReadAllText`/
  `JsonConvert.Serialize·Deserialize`를 직접 호출하는 곳은 `SaveLoadManager.cs`뿐이므로
  (`InputHandler`는 지금까지 PlayerPrefs만 썼음), 이 원칙을 적용해도 다른 파일을
  건드릴 필요는 없다. 이 phase가 그 원칙이 실제로 지켜지는 첫 사례가 된다. Phase 11의
  기존 D-01("manager owns save/load logic; callers call in, no event bus") 패턴과
  동일 계열 — `Checkpoint.cs`/보스 사망 지점이 `SaveLoadManager.Instance.SaveXxx()`를
  호출하듯, `InputHandler`도 동일하게 `SaveLoadManager.Instance.SaveKeybindings(json)`을
  호출하는 방식.

### 마이그레이션

- **D-03:** 기존 `PlayerPrefs`에 저장된 사용자 키 리바인딩은 **마이그레이션하지 않는다**.
  아직 출시 전/테스트 단계이므로 `keybind.json` 기준으로 새로 시작하고, `PlayerPrefs`
  키(`SAVE_KEY = "InputBindings"`)는 더 이상 읽지도 쓰지도 않는다.

### 파일 독립성

- **D-04:** `keybind.json`은 `save.json`(게임 진행 저장)과 **완전히 독립**적으로 취급한다.
  `SaveLoadManager.NewGame()`, 세이브 파일 삭제/리셋 등 어떤 게임 진행 관련 로직도
  `keybind.json`을 건드리지 않는다. 키 설정은 세이브 슬롯 유무·상태와 무관하게 항상
  유지되는 "설정 파일"로 취급한다.

### Claude's Discretion
- `SaveKeybindings`/`LoadKeybindings` 등 정확한 메서드 시그니처, 파일 없음/손상 시
  에러 처리 방식(기존 `LoadGame()`의 try/catch 패턴 참고), `keybind.json` 경로를
  노출하는 `static string KeybindPath` 프로퍼티 추가 여부는 계획 단계에서 Claude가 정한다.
- `InputHandler.Awake()`가 `SaveLoadManager.Instance`를 호출하는 시점에 인스턴스가
  이미 존재하는지(SaveLoadManager는 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`로
  부트스트랩되므로 씬의 어떤 `Awake()`보다도 먼저 생성됨)는 기존 코드 구조상 문제없음 —
  연구/계획 단계에서 재확인만 하면 됨.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 키 리바인딩 (현재 구현)
- `Assets/Player/Script/InputHandler.cs` — 싱글톤, `SaveBindingOverrides()`/`LoadBindingOverrides()`가
  PlayerPrefs로 저장하는 현재 구현. **주의: 이 파일은 CP949(비-UTF-8) 인코딩** —
  13-AUDIT-REPORT.md D-04에서 발견된 46개 CP949 파일 중 하나. 표준 Read/Edit 툴 조합은
  파일 전체의 비-ASCII 바이트를 훼손시키므로(Phase 11 Plan 3에서 동일 문제 발생),
  수정 시 `git show HEAD:<path>`로 원본 바이트를 추출해 순수 바이트 단위로 편집할 것.
- `Assets/Player/Script/Menu/ControlsSettingsPanel.cs` — 리바인딩 UI, `FinishRebind()`/
  `ResetAllBindings()`에서 `InputHandler.Instance?.SaveBindingOverrides()` 호출.

### 저장 시스템 (위임 대상)
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — Phase 11 산출물. `save.json` 단일
  슬롯 관리, `SavePath` static 프로퍼티, `Save()`/`LoadGame()` 파일 I/O 패턴을
  keybind.json에도 동일하게 적용할 것.
- `Assets/SaveSystem/Script/SaveData.cs` — 기존 세이브 스키마 (keybind는 이 스키마에
  포함하지 않음 — 별도 파일).
- `Assets/SaveSystem/Check.md` — Phase 11 Play 모드 검증 체크리스트 컨벤션. 이 phase도
  같은 문서 컨벤션으로 체크리스트를 추가하는 것을 고려.

### 관련 STATE.md 항목
- `.planning/STATE.md` "Accumulated Context" 중 Phase 11 관련 결정들 (D-01: manager owns
  save/load logic, callers call in / CP949 인코딩 사고 사례 2건 / 2026-08-20 정책:
  "GSD phase 완료 시 개발용 추적 Debug.Log는 기본적으로 전량 제거").

[추가 스펙/ADR 문서 없음 — 이번 phase는 REQUIREMENTS.md v2.0 마일스톤 범위 밖의
독립 유지보수성 phase로, 위 두 기존 시스템(InputHandler, SaveLoadManager) 코드 자체가
유일한 근거 문서.]

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SaveLoadManager.Instance` 싱글톤 + `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`
  부트스트랩 패턴 — 새 파일 I/O 메서드를 이 클래스에 추가하기만 하면 되고, 별도
  매니저를 새로 만들 필요 없음.
- `Application.persistentDataPath` 기반 경로 패턴 (`SavePath` static 프로퍼티) —
  `KeybindPath`도 동일 패턴으로 만들 수 있음.
- Unity Input System의 `InputActionAsset.SaveBindingOverridesAsJson()` /
  `LoadBindingOverridesFromJson(string)` — 이미 JSON 문자열 변환을 제공하므로
  `InputHandler`가 직접 JSON 파싱 코드를 짤 필요 없음 (Newtonsoft.Json 불필요,
  Unity Input System 자체 직렬화 사용).

### 확립된 패턴
- 파일 I/O는 오직 `SaveLoadManager.cs`에서만 발생 (`File.WriteAllText`/
  `File.ReadAllText`) — 코드베이스 전체 스캔으로 확인됨, 이번 phase가 이 원칙을
  두 번째 파일(keybind.json)에 처음 적용하는 사례.
- "매니저가 저장/로드 로직을 소유하고, 호출자는 그냥 불러서 씀" (Phase 11 D-01) —
  `Checkpoint.cs`/보스 사망 지점이 `SaveLoadManager.Instance.SaveAtCheckpoint()`/
  `SaveOnBossDefeated()`를 부르는 것과 동일하게, `InputHandler`도
  `SaveLoadManager.Instance.SaveKeybindings(json)`을 호출하는 방식으로 통일.

### Integration Points
- `InputHandler.Awake()` — 기존 `LoadBindingOverrides()` 호출 위치, `SaveLoadManager`
  경유 로드로 교체될 지점.
- `InputHandler.SaveBindingOverrides()` — 기존 PlayerPrefs 저장 로직, `SaveLoadManager`
  경유 저장으로 교체될 지점. `ControlsSettingsPanel.FinishRebind()`/`ResetAllBindings()`
  두 호출부는 코드 변경 없이 그대로 동작해야 함 (내부 구현만 바뀜).

</code_context>

<specifics>
## Specific Ideas

- 사용자가 명시한 원칙: "JSON 형태로 파일을 저장하는 행위 자체는 SaveLoadManager만 할 수
  있도록" — 이는 project-wide 아키텍처 원칙으로 채택되었고(D-02), 앞으로 새로운 저장
  대상이 생겨도 이 원칙을 따라야 한다는 선례가 됨.

</specifics>

<deferred>
## Deferred Ideas

None — 논의가 phase 범위 내에서 유지됨.

</deferred>

---

*Phase: 14-keybinding-keybind-json-saveloadmanager*
*Context gathered: 2026-08-20*
