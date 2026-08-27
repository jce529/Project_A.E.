# Phase 14: 키바인딩 keybind.json 저장 + SaveLoadManager 위임 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-20
**Phase:** 14-keybinding-keybind-json-saveloadmanager
**Areas discussed:** 위임 범위 (Delegation Boundary), 마이그레이션 (Migration), 파일 독립성 (File Independence)

---

## 위임 범위 (Delegation Boundary)

| Option | Description | Selected |
|--------|-------------|----------|
| 파일 I/O만 위임 | InputHandler가 여전히 InputActionAsset과 JSON 직렬화(SaveBindingOverridesAsJson/LoadBindingOverridesFromJson)를 담당하고, SaveLoadManager는 새 SaveKeybindings(json)/LoadKeybindings() 메서드로 파일 읽기/쓰기만 담당. 기존 SaveLoadManager D-01 패턴(호출자가 불러서 씀, 이벤트버스 없음)과 일치 | ✓ |
| SaveLoadManager가 InputActionAsset까지 직접 참조 | SaveLoadManager가 InputActionAsset을 직접 들고 있으면서 바인딩 직렬화/역직렬화까지 전부 수행. InputHandler와의 결합도가 늘어나고 SaveLoadManager가 입력 시스템 세부사항까지 알아야 함 | |

**User's choice:** 파일 I/O만 위임 (권장안)

**Notes:** 사용자가 추가로 "JSON 형태로 파일을 저장하는 행위 자체를 SaveLoadManager만 할 수 있도록 하고 싶은데 문제가 있을까?"라고 질문. 코드베이스 전체를 스캔한 결과 `File.WriteAllText`/`File.ReadAllText`/`JsonConvert.Serialize·Deserialize`를 직접 호출하는 곳은 `SaveLoadManager.cs` 하나뿐이었고(InputHandler는 지금까지 PlayerPrefs만 사용, 파일 쓰기 아님), 이 원칙을 keybind.json에 적용해도 다른 파일을 건드릴 필요가 없음을 확인. 사용자가 "그 방향으로 확정"이라고 응답 — "파일 I/O만 위임" 옵션이 정확히 이 원칙과 일치함을 확인하고 채택.

---

## 마이그레이션 (Migration)

| Option | Description | Selected |
|--------|-------------|----------|
| 마이그레이션 없음 | 아직 출시 전/테스트 단계이므로 PlayerPrefs 기존 값은 무시하고 keybind.json 기준으로 새로 시작. 코드가 가장 단순해짐 | ✓ |
| 1회성 마이그레이션 추가 | PlayerPrefs에 값이 있으면 최초 로드 시 keybind.json으로 옮겨 쓰고 PlayerPrefs는 이후 무시. 기존 테스터의 리바인딩을 보존하지만 코드가 조금 늘어남 | |

**User's choice:** 마이그레이션 없음
**Notes:** 없음.

---

## 파일 독립성 (File Independence)

| Option | Description | Selected |
|--------|-------------|----------|
| 완전 독립 | keybind.json은 설정 파일로 취급 — NewGame()이나 세이브 삭제/리셋 로직이 있어도 keybind.json은 절대 건드리지 않음. 키 설정은 세이브 슬롯과 무관하게 유지 | ✓ |
| 세이브와 함께 관리 | 세이브 리셋/삭제 시 keybind.json도 함께 초기화되도록 연동 | |

**User's choice:** 완전 독립
**Notes:** 없음.

---

## Claude's Discretion

- `SaveKeybindings`/`LoadKeybindings` 정확한 메서드 시그니처
- 파일 없음/손상 시 에러 처리 방식 (기존 `LoadGame()`의 try/catch 패턴 참고)
- `KeybindPath` static 프로퍼티 노출 여부
- `InputHandler.Awake()` 시점의 `SaveLoadManager.Instance` 존재 여부 재확인 (부트스트랩 순서상 문제없을 것으로 예상되나 계획 단계에서 재검증)

## Deferred Ideas

없음 — 논의가 phase 범위 내에서 유지됨.
