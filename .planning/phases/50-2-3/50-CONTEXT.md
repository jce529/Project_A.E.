# Phase 50: 세이브 슬롯 확장 - Context

**Gathered:** 2026-08-27
**Status:** Ready for planning

> **번호 안내:** 이 Phase는 임시로 50번을 쓰고 있다. 다른 기기(로컬에 없는 별도 작업 환경)의
> 로드맵이 이미 Phase 13까지 완료돼 있고 그보다 더 진행됐을 가능성도 있어서, 충돌을 피하려고
> 큰 번호를 임시로 예약해둔 것이다. 두 기기 로드맵을 동기화한 뒤 정확한 다음 번호로
> 재번호(폴더명 `.planning/phases/50-2-3/` 포함)해야 한다. 아래 결정 사항 자체는 번호와 무관하게
> 유효하다.

<domain>
## Phase Boundary

Phase 11에서 구현한 단일 슬롯 `SaveLoadManager`(`save.json` 1개)를 3슬롯 구조로 확장한다.
기존 슬롯 1개 + 신규 슬롯 2개, 슬롯마다 완전히 독립된 `SaveData`(씬/좌표, 플레이어 스탯,
보스 진행도, 맵 기믹, 아이템)를 갖는다. 메인 메뉴의 "이어하기"/"새시작" 흐름이 슬롯 개념을
포함하도록 바뀌고, 슬롯 목록에 슬롯별 진행 상태를 보여주는 UI가 새로 필요하다.

이번 페이즈는 슬롯 구조(데이터 분리, 파일 구조, 매니저 API)와 슬롯 선택/진행도 UI를 다룬다.
Phase 11이 이미 구현한 저장 트리거 통합(체크포인트/보스 격파 시 `Save()` 호출), 좌표 복원
경로(`PlayerSpawner`), 직렬화 방식(Newtonsoft.Json)은 그대로 재사용하고 바꾸지 않는다.

</domain>

<decisions>
## Implementation Decisions

### 슬롯 선택 UI 흐름
- **D-01:** "이어하기" 버튼은 항상 슬롯 선택 화면으로 이동한다. 사용자가 슬롯을 직접 골라야
  로드가 시작된다 — 자동으로 아무 슬롯이나 이어서 로드하지 않는다.
- **D-02:** "새시작" 버튼은 빈 슬롯이 하나라도 있으면 슬롯 선택 화면을 거치지 않고 자동으로
  그 빈 슬롯을 골라 바로 새 게임을 시작한다.
- **D-03:** 3개 슬롯이 전부 데이터로 차 있으면 "새시작"도 슬롯 선택 화면으로 보내서, 사용자가
  덮어쓸 슬롯을 직접 고르게 한다.

### 빈 슬롯 / 덮어쓰기 처리
- **D-04:** 슬롯 선택 화면에서 이미 데이터가 있는 슬롯을 "새로 시작"용으로 선택하면(D-03 경로,
  또는 슬롯 화면에서 직접 재시작을 고르는 모든 경로), 덮어쓰기 확인 다이얼로그가 반드시 뜬다
  ("이 슬롯을 덮어쓰고 새 게임을 시작하시겠습니까?"). 확인 없이 즉시 지우지 않는다.
- **D-05:** 확인 없는 즉시 덮어쓰기 경로는 만들지 않는다 — 진행 중인 세이브를 실수로 날리는
  사고를 막는 것이 목적.

### 파일 구조
- **D-06:** 슬롯마다 별도 JSON 파일로 저장한다 (배열을 담은 파일 1개 방식은 채택하지 않음).
  한 슬롯 저장이 다른 슬롯에 영향을 주지 않고, 파일 손상 시 그 슬롯 하나만 영향받는다.
- **D-07 (Claude's Discretion으로 넘어감):** 정확한 파일명 규칙과 기존 `save.json` 마이그레이션
  방식은 사용자가 "파일 구조"를 별도 파일 방식으로 결정했을 뿐, 세부 네이밍까지 확정하지
  않았다. 연구/계획 단계에서 다음을 고려해 정할 것:
  - 기존 플레이어가 이미 갖고 있는 `save.json`(Phase 11 산출물)을 슬롯 0으로 인식시켜야
    데이터 유실이 없다 — 파일을 지우거나 이름을 강제로 바꾸는 마이그레이션은 피할 것.
  - 후보: 기존 `save.json`을 슬롯 0 파일로 그대로 유지 + 슬롯 1/2만 `save_1.json`/`save_2.json`
    신규 생성, 또는 최초 실행 시 `save.json` → `save_0.json` 자동 마이그레이션. 어느 쪽이든
    "기존 진행 중인 세이브가 사라지면 안 된다"가 절대 기준.

### Claude's Discretion
- **슬롯 카드에 표시할 진행도 내용:** 사용자가 이번 논의에서 다루지 않기로 함 (스킵 선택).
  기본 방향: 새 필드를 추가하지 말고 `SaveData`에 이미 있는 값만 사용 — 씬 이름(`SceneName`)과
  격파한 보스 수(`BossProgress.Count`, 예: "격파 보스 2/4") 정도로 시작. 플레이타임/마지막 저장
  시각처럼 `SaveData`에 없는 필드를 새로 추가할지는 연구/계획 단계에서 필요성이 명확해지면
  그때 판단 — 이번 결정에서 요구된 것은 아님.
- 정확한 슬롯 파일명 규칙 및 기존 `save.json` 마이그레이션 방식 (D-07 참고).
- 슬롯 선택 화면의 정확한 레이아웃/비주얼 디자인 (카드 3개를 어떻게 배치할지 등) — 기능
  요구사항(D-01~D-05)만 결정됐고 UI 디테일은 범위 밖.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

이번 페이즈에는 별도 ADR/스펙 문서가 없다. 로드맵 항목 + 이 CONTEXT.md가 스펙 역할을 겸한다.

### 로드맵 원문
- `.planning/ROADMAP.md` (Phase 50 섹션 — 임시 번호 안내 포함)

### 선행 Phase 결정 (그대로 유지해야 하는 부분)
- `.planning/phases/11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json/11-CONTEXT.md`
  — D-01(저장 트리거 직접 호출), D-05(PlayerSpawner 좌표 복원 경로), D-06(NewGame 메모리만 리셋)은
  슬롯이 여러 개가 되어도 그대로 적용된다. D-02(단일 슬롯)만 이번 페이즈가 뒤집는 대상이고,
  Deferred Ideas에 "다중 세이브 슬롯 지원 — 필요 시 후속 페이즈"로 이미 예고되어 있었다.

No external specs — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — 싱글톤 패턴, `Save()`/`LoadGame()`/`NewGame()`/
  `HasSaveFile()` 전부 슬롯 개념 없이 단일 `_data`/`SavePath` 기준으로 짜여 있음. 슬롯화하려면
  이 파일의 핵심 API(`Save`, `LoadGame`, `NewGame`, `HasSaveFile`)를 슬롯 인자를 받는 형태로
  바꾸거나 "현재 활성 슬롯" 개념을 추가해야 한다.
- `Assets/SaveSystem/Script/SaveData.cs` — 슬롯 하나의 스키마 그 자체 (SceneName/SpawnPointName,
  PlayerStatsSaveData, BossProgress, MapGimmickState, Items). 이 클래스 자체는 슬롯 스키마로
  그대로 재사용 가능 — 슬롯화는 "이 객체를 몇 개 관리하느냐"의 문제.
- `Assets/Script/MainMenuUI.cs` — 이미 존재하는 메인 메뉴 컨트롤러. `loadGameButton`을
  `SaveLoadManager.Instance.HasSaveFile()`로 활성화/비활성화하고, `OnClickLoad()` →
  `SaveLoadManager.Instance.LoadGame()`, `OnClickStart()` → `SceneManager.LoadScene("Tutorial Map")`
  직접 호출(주의: `NewGame()`을 호출하지 않는 기존 동작 — 슬롯화하면서 이 부분도 손대야
  D-02/D-03 흐름이 성립함).

### ⚠️ 동시 진행 중인 변경 (충돌 주의)
- 이 논의와 별개로, **quick task `260827-h5y`**(PlayerPrefs → `setting.json` 전환)가 지금
  `Assets/SaveSystem/Script/SaveLoadManager.cs`를 수정 중이다: private 필드
  `SaveSettings` → `JsonSettings` 리네임, `SaveSettings()`/`LoadSettings()`/`Settings`/
  `CurrentSettings`/`SettingsPath` 신규 추가, `Awake()`에 `LoadSettings()` 한 줄 추가.
  이번 Phase 50의 연구/계획은 그 quick task가 완료된 **이후의** `SaveLoadManager.cs` 상태를
  기준으로 삼아야 한다 (지금 이 문서를 작성하는 시점엔 아직 진행 중). `setting.json`/
  `SettingsData`는 게임 진행도(`save.json`/`SaveData`)와 완전히 별개 파일·별개 모델이므로
  슬롯화 대상이 아니다 — 슬롯은 오직 `save*.json`/`SaveData`에만 적용된다.

### Established Patterns
- 싱글톤: `public static X Instance`, `Awake()` null 체크 + `DontDestroyOnLoad`, 아니면 `Destroy`.
- 디스크 쓰기는 명시적 트리거(체크포인트/보스 격파/Save 버튼)에서만 발생, 플레이 중 메모리만 사용.

### Integration Points
- `Assets/Script/MainMenuUI.cs` `OnClickStart()`/`OnClickLoad()` — 슬롯 선택 화면 진입점이 될
  가장 유력한 위치.
- 신규 슬롯 선택 화면(아직 없음) — 이번 페이즈에서 새로 만들어야 하는 UI.

</code_context>

<specifics>
## Specific Ideas

- "이어하기는 항상 슬롯 화면, 새시작은 빈 슬롯 있으면 자동으로 바로 시작" — 두 버튼의 동작이
  대칭이 아니라 비대칭이라는 점이 핵심 (D-01~D-03).
- "덮어쓰기는 무조건 확인창 거쳐야 함" — 예외 없음 (D-04, D-05).
- "슬롯마다 파일 따로" — 배열 직렬화 방식은 명시적으로 배제됨 (D-06).

</specifics>

<deferred>
## Deferred Ideas

- 슬롯 카드 진행도 표시에 플레이타임/마지막 저장 시각 등 `SaveData`에 없는 새 필드를 넣는 것 —
  이번 논의에서 요구되지 않음, 필요성이 명확해지면 후속 판단 (Claude's Discretion 참고).
- 슬롯 선택 화면의 비주얼 디자인/레이아웃 세부사항 — 기능 요구사항만 확정, 디자인은 범위 밖.

### Reviewed Todos (not folded)
None — no matching todos found for this phase.

</deferred>

---

*Phase: 50-2-3 (임시 번호)*
*Context gathered: 2026-08-27*
