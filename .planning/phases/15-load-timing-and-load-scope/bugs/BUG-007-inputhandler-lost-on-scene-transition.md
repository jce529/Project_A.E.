# BUG-007: 씬 전환 후 InputHandler 액션 에셋 유실로 플레이어 입력 불가

- 심각도: 높음
- 상태: 확인됨 — 미해결
- 발견일: 2026-09-11
- 발견 경로: Phase 15 세이브 슬롯 2 로드 후 Tutorial Map → 1 stage 전환 라이브 테스트 및 Unity MCP 조사
- 영향 범위: 씬 전환 뒤 모든 Player Input Action 기반 이동·공격·상호작용

## 현상

세이브 슬롯 2로 `Tutorial Map`을 진행한 뒤 `1 stage`로 전환하면 다음 오류가 발생하고 플레이어가 움직이지 않는다.

```text
InputHandler: Input Action Asset이 할당되지 않았습니다! Inspector에서 InputSystem_Actions를 연결하거나 Resources 폴더에 넣으세요.
InputHandler:Awake () (at Assets/Player/Script/InputHandler.cs:78)
```

## 재현 절차

1. 세이브 슬롯 2를 로드하여 `Tutorial Map`에 진입한다.
2. 다음 진행 경로로 `1 stage` 씬으로 이동한다.
3. Console의 `InputHandler.Awake()` 오류를 확인한다.
4. 이동 입력을 시도한다.

## 기대 결과

- Tutorial의 `InputHandler`가 씬 전환 뒤에도 유지되거나, 1스테이지의 새 `InputHandler`가 동일한 액션 에셋으로 정상 초기화된다.
- Player 액션 맵이 활성화되어 이동과 상호작용이 가능하다.

## 실제 결과

- Tutorial의 `InputHandler`는 씬 전환 시 유지되지 않는다.
- 1스테이지 `InputHandler.inputActions`는 `null`이다.
- `Resources.Load<InputActionAsset>("InputSystem_Actions")`도 `null`을 반환한다.
- 액션 필드가 초기화되지 않아 플레이어 입력이 동작하지 않는다.

## 근거

- Unity MCP 씬 검사:
  - `Tutorial Map/InputManager`: `inputActions=InputSystem_Actions`
  - `1 stage/InputManager`: `inputActions=null`
  - 두 `InputManager` 모두 루트가 아니라 `Manager`의 자식이다.
- 액션 에셋 실제 경로: `Assets/InputSystem_Actions.inputactions`
- `Resources.Load("InputSystem_Actions")` 조회 결과: `null`
- Unity Console:

```text
DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
InputHandler:Awake () (at Assets/Player/Script/InputHandler.cs:66)
```

## 원인

`InputHandler.Awake()`는 `DontDestroyOnLoad(gameObject)`를 호출하지만 `InputManager`가 `Manager`의 자식이므로 Unity가 영속화를 거부한다. Tutorial 핸들러가 씬과 함께 파괴된 뒤 1스테이지의 새 핸들러가 singleton이 되지만, 해당 컴포넌트에는 `inputActions`가 직렬화되어 있지 않다. fallback 경로 역시 액션 에셋이 `Resources` 폴더 밖에 있어 실패한다.

## 수정 방향

1. `InputHandler`가 붙은 오브젝트 또는 그 루트 `Manager`를 올바르게 `DontDestroyOnLoad` 대상으로 만든다.
2. 모든 진입 씬에서 안전하도록 액션 에셋 참조를 공통 프리팹/부트스트랩에서 보장하거나, 에셋을 실제 Resources 경로에서 로드 가능하게 만든다.
3. 중복 `InputHandler`가 생성될 때 기존 singleton의 액션 맵이 유지되는지 확인한다.
4. Tutorial Map → 1 stage 실제 전환 뒤 이동·상호작용을 재검증한다.

## 완료 조건

- [ ] `DontDestroyOnLoad` root 경고가 발생하지 않는다.
- [ ] Tutorial Map → 1 stage 전환 뒤 유효한 `InputHandler.Instance`가 정확히 하나 존재한다.
- [ ] 해당 인스턴스의 `inputActions`와 `Player` 액션 맵이 유효하고 활성 상태다.
- [ ] 전환 뒤 이동·공격·상호작용 입력이 정상 동작한다.
- [ ] Unity Console에 관련 오류나 예외가 없다.
- [ ] 수정 커밋과 Unity MCP/Play 모드 검증 결과를 기록한다.

## 해결 기록

- 해결일:
- 수정 커밋:
- 검증 증거:

## 최신 감사 (2026-09-11)

- `InputHandler.cs`는 여전히 자식 `InputManager` 자신에게 `DontDestroyOnLoad(gameObject)`를 호출한다.
- `1 stage`의 `InputManager.inputActions=null` 및 `Resources.Load("InputSystem_Actions")=null` 상태가 유지된다.
- Unity MCP Console에서 root 제한 경고와 Input Action Asset 미할당 오류를 재확인했으므로 `확인됨 — 미해결` 상태가 최신이다.

## 관련 문서

- `.planning/phases/15-load-timing-and-load-scope/15-UAT.md`
- `.planning/phases/15-load-timing-and-load-scope/bugs/README.md`
- `Assets/Player/Script/InputHandler.cs`
- `Assets/Scenes/Tutorial Map.unity`
- `Assets/Scenes/1 stage.unity`
