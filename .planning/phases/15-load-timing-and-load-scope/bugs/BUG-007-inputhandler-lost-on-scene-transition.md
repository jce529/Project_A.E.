# BUG-007: Manager 계층의 전역 싱글톤 영속화 실패와 입력 유실

- 심각도: 높음
- 상태: 수정됨 — 재검증 필요
- 발견일: 2026-09-11
- 발견 경로: Phase 15 세이브 슬롯 2 로드 후 Tutorial Map → 1 stage 전환 라이브 테스트, Unity MCP 조사 및 후속 Manager 계층 감사
- 영향 범위: 씬 전환 뒤 `InputHandler`, `GameStateManager`, `AudioManager`, `GameManager`와 이에 의존하는 입력·게임 상태·오디오 기능

## 현상

세이브 슬롯 2로 `Tutorial Map`을 진행한 뒤 `1 stage`로 전환하면 다음 오류가 발생하고 플레이어가 움직이지 않는다.

```text
InputHandler: Input Action Asset이 할당되지 않았습니다! Inspector에서 InputSystem_Actions를 연결하거나 Resources 폴더에 넣으세요.
InputHandler:Awake () (at Assets/Player/Script/InputHandler.cs:78)
```

후속 조사 결과, 이 문제는 `InputHandler` 하나의 참조 누락이 아니라 Tutorial의 `Manager` 계층에 포함된 전역 singleton 전반의 생명주기 결함이다. `GameManager`, `GameStateManager`, `InputManager`, `AudioManager`가 모두 루트가 아닌 자식 GameObject인 상태에서 각자 `DontDestroyOnLoad(gameObject)`를 호출한다. Unity가 영속화를 거부하므로 씬 전환 후 이 서비스들의 생존이 보장되지 않는다.

`InputHandler`는 다음 씬의 대체 인스턴스가 끊어진 액션 에셋 참조를 사용하면서 즉시 오류를 출력했지만, 다른 매니저는 소비자가 null을 허용하거나 다음 씬에 대체 인스턴스가 없을 경우 명확한 오류 없이 기능만 사라질 수 있다.

## 재현 절차

### 입력 유실

1. 세이브 슬롯 2를 로드하여 `Tutorial Map`에 진입한다.
2. 다음 진행 경로로 `1 stage` 씬으로 이동한다.
3. Console의 `DontDestroyOnLoad` root 제한 경고와 `InputHandler.Awake()` 오류를 확인한다.
4. 이동·공격·상호작용 입력을 시도한다.

### 공통 Manager 생명주기

1. `Tutorial Map`을 열고 루트 `Manager`의 자식들을 확인한다.
2. `GameManager`, `GameStateManager`, `InputManager`, `AudioManager`가 모두 `Manager`의 자식임을 확인한다.
3. 각 스크립트의 `Awake()`에서 `DontDestroyOnLoad(gameObject)`를 호출하는지 확인한다.
4. Play Mode에서 다음 씬으로 전환한 뒤 기존 singleton 인스턴스의 생존 여부와 중복 여부를 확인한다.

## 기대 결과

- 전역 서비스는 최초 진입 씬과 무관하게 정확히 하나 자동 생성된다.
- `InputHandler`, `GameStateManager`, `AudioManager` 등 전역으로 분류된 인스턴스는 씬 전환 뒤에도 유지된다.
- `InputHandler`의 유효한 `InputSystem_Actions`와 `Player` 액션 맵이 계속 활성 상태다.
- Player 이동·공격·상호작용이 정상 작동한다.
- 씬 오브젝트를 참조하는 `EnvironmentManager`와 `EventSystem`은 현재 씬과 함께 교체된다.
- `DontDestroyOnLoad` root 제한 경고가 발생하지 않는다.

## 실제 결과

- Tutorial의 `InputHandler`는 root 제한 때문에 씬 전환 시 유지되지 않는다.
- `1 stage` 및 다수 스테이지 씬의 `InputHandler.inputActions`는 존재하지 않는 GUID `b5f9278df1cbb464580bcef2433df8a8`을 참조하여 런타임에서 `null`이 된다.
- 정상 액션 에셋 GUID는 `2bcd2660ca9b64942af0de543d8d7100`이며 Tutorial만 이 참조를 사용한다.
- `Resources.Load<InputActionAsset>("InputSystem_Actions")`도 에셋이 Resources 밖에 있어 `null`을 반환한다.
- `GameManager`, `GameStateManager`, `InputManager`, `AudioManager` 모두 자식 GameObject 자신을 영속화하려 하므로 같은 root 제한의 영향을 받는다.
- `EnvironmentManager`와 `EventSystem`도 같은 부모 아래에 있어 현재 `Manager` 전체를 그대로 영속화하면 씬 로컬 객체까지 함께 남는다.

## 근거

- `Assets/Scenes/Tutorial Map.unity`
  - 루트 `Manager`의 자식: `EnvironmentManager`, `GameManager`, `GameStateManager`, `InputManager`, `EventSystem`, `AudioManager`
  - `Tutorial Map/InputManager.inputActions`: 정상 GUID `2bcd2660ca9b64942af0de543d8d7100`
  - `EnvironmentManager`는 Tutorial의 `WaterController`, `SpriteRenderer`, BGM/필터를 직렬화 참조한다.
- 스테이지 씬들의 `InputManager.inputActions`
  - `1 stage`, `2 stage`, `3 stage main` 및 조사된 3 stage 하위 씬들이 존재하지 않는 GUID `b5f9278df1cbb464580bcef2433df8a8`을 참조한다.
- 액션 에셋 실제 경로: `Assets/InputSystem_Actions.inputactions`
- 액션 에셋 정상 GUID: `2bcd2660ca9b64942af0de543d8d7100`
- `Resources.Load("InputSystem_Actions")` 조회 결과: `null`
- 개별 영속화 호출:
  - `Assets/map/script/GameManager.cs`
  - `Assets/Player/Script/GameStateManager.cs`
  - `Assets/Player/Script/InputHandler.cs`
  - `Assets/Script/AudioManager.cs`
- Unity Console:

```text
DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
InputHandler:Awake () (at Assets/Player/Script/InputHandler.cs:66)
```

## 원인

현재 `Manager` 계층은 서로 다른 생명주기를 가진 객체를 한 부모 아래에 혼합한다.

- 전역 의도: `GameManager`, `GameStateManager`, `InputManager`, `AudioManager`
- 씬 로컬: 씬 오브젝트를 직접 참조하는 `EnvironmentManager`, UI 수명에 종속된 `EventSystem`

전역 singleton들은 자신이 루트라는 전제 아래 개별적으로 `DontDestroyOnLoad(gameObject)`를 호출하지만 실제로는 모두 `Manager`의 자식이다. Unity는 해당 요청을 거부한다. 이후 새 씬의 대체 인스턴스가 존재하더라도 씬마다 직렬화 상태가 달라질 수 있으며, 실제로 스테이지들의 Input Action Asset 참조가 이미 끊어져 있다.

또한 현재 `InputHandler.OnEnable()`은 람다 콜백을 등록하지만 `OnDisable()`에서는 액션 에셋을 비활성화할 뿐 콜백을 해제하지 않는다. 중복 인스턴스나 재활성화가 발생하면 기존 인스턴스의 공유 액션 에셋을 끄거나 입력 이벤트가 중복 등록될 위험이 있다.

## 결정된 수정 방향

현재 `Manager` 전체를 그대로 영속화하지 않고 전역 서비스와 씬 로컬 관리자를 분리한다.

### 전역 영역

`PersistentManagers.prefab`을 만들고 다음 서비스를 포함한다.

- `InputHandler`
- `GameStateManager`
- `AudioManager`
- 전역 책임이 확정된 추가 서비스

프리팹의 루트 생명주기 소유자 하나만 `DontDestroyOnLoad`를 호출한다. 자식 서비스의 개별 `DontDestroyOnLoad` 호출은 제거하거나 루트에 위임한다. `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)` bootstrap이 최초 씬 로드 전에 프리팹을 정확히 한 번 생성하여 MainMenu와 임의의 게임플레이 씬 직접 실행을 모두 지원한다.

프리팹은 `InputSystem_Actions`를 직렬화 참조한다. Resources 문자열 로드가 필요하다면 프리팹 하나만 Resources에서 로드하고, 액션 에셋 자체는 프리팹 참조로 포함한다.

`GameManager`는 현재 실제 책임이 없는 빈 singleton이므로 전역 프리팹 포함 여부를 구현 전에 확정한다. 사용처가 없다면 제거 대상으로 검토한다.

### 씬 로컬 영역

- `EnvironmentManager`: 현재 씬의 `WaterController`, 배경 렌더러, 오디오 필터 참조를 유지한다.
- `EventSystem`: 현재 씬 UI와 함께 유지한다.
- 그 밖의 씬 오브젝트 직접 참조 관리자는 현재 씬에 남긴다.

### InputHandler 생명주기 보강

1. 정상 `InputSystem_Actions` 참조를 공통 프리팹 한 곳에서 보장한다.
2. 모든 기존 씬의 중복 `InputManager`와 끊어진 GUID 참조를 제거한다.
3. 주 인스턴스만 액션을 Enable/Disable하도록 소유권을 명확히 한다.
4. 람다 대신 해제 가능한 이름 있는 콜백을 사용하고 등록·해제를 대칭적으로 처리한다.
5. 필요하면 인스턴스 전용 액션 에셋 복제본을 사용해 공유 상태 충돌을 차단한다.

### 상태 전환 보강

영속 `GameStateManager`가 이전 씬의 `Paused`, `Puzzle`, `Loading` 상태 또는 `Time.timeScale = 0`을 다음 씬에 누출하지 않도록 씬 전환 완료 시 `Playing`과 `Time.timeScale = 1` 복구 규칙을 둔다.

## 완료 조건

- [x] MainMenu 및 임의의 게임플레이 씬 직접 실행 시 전역 매니저 루트가 정확히 하나 자동 생성된다.
- [x] Tutorial Map → 1 stage 및 후속 씬 전환 뒤 같은 전역 서비스 인스턴스가 유지된다.
- [x] `InputHandler.Instance`가 정확히 하나이고 유효한 `InputSystem_Actions`와 활성 `Player` 액션 맵을 가진다.
- [ ] 전환 뒤 이동·공격·상호작용 입력이 정상 동작하고 중복 호출되지 않는다.
- [x] `GameStateManager`가 새 씬에서 올바른 상태와 `Time.timeScale`을 가진다.
- [ ] `AudioManager`가 중복 생성되지 않으며 BGM/SFX 설정이 유지된다.
- [ ] `EnvironmentManager`가 항상 현재 씬의 오브젝트를 참조한다.
- [x] 활성 `EventSystem`이 정확히 하나다.
- [x] `DontDestroyOnLoad` root 제한 경고와 관련 오류·예외가 없다.
- [ ] 수정 커밋과 Unity MCP/Play Mode 검증 결과를 기록한다.

현재 판정: **6/10 충족**. 입력 실동작, 오디오 설정 유지, Environment 직렬화 참조, 수정 커밋 기록은 후속 재검증 대상이다.

## 검증 시나리오

1. MainMenu 직접 실행 후 전역 서비스 자동 생성 확인
2. Tutorial Map 직접 실행 후 전역 서비스 자동 생성 확인
3. 1 stage, 2 stage, 3 stage 직접 실행 후 입력·상태·오디오 확인
4. Tutorial Map → 1 stage 연속 전환 후 인스턴스 동일성 및 입력 확인
5. 일시정지 상태에서 씬 전환 후 `Playing`/`Time.timeScale = 1` 복구 확인
6. 각 씬에서 현재 Environment 참조와 활성 EventSystem 수 확인
7. Console에서 root 제한 경고, 액션 미할당, 중복 이벤트 오류 부재 확인

## 해결 기록

- 해결일:
- 수정 커밋:
- 검증 증거:
  - Unity 컴파일 오류 0건.
  - `Resources/PersistentManagers.prefab`에 `InputHandler`, `GameStateManager`, `AudioManager`와 정상 `InputSystem_Actions` GUID가 연결됨.
  - `Assets/Scenes`의 10개 게임플레이 씬에서 기존 전역 매니저 컴포넌트와 끊어진 입력 GUID가 0건임을 정적으로 확인.
  - MainMenu 직접 실행에서 영속 루트 1개, 활성 Player 액션 맵, `Playing`, `Time.timeScale = 1`, EventSystem 1개 확인.
  - MainMenu → Tutorial Map → 1 stage에서 `InputHandler=-5646`, `GameStateManager=-5652`, `AudioManager=-5658` 인스턴스 ID가 동일하게 유지됨.
  - Tutorial Map 및 1 stage에서 영속 루트 1개, 활성 Player 액션 맵, `Playing`, `Time.timeScale = 1`, 현재 씬 `EnvironmentManager` 1개, EventSystem 1개 확인.
  - MainMenu에서 가상 W/F/마우스 왼쪽 입력으로 이동 performed/canceled, 상호작용, 공격 이벤트가 각각 정확히 1회 발생함을 확인.
  - 실제 `Tutorial Map → 1 stage` 키보드 이동 재검증과 2/3 stage 직접 실행 후속 조회는 Unity MCP 연결 승인 해제로 보류.

## 수정 내역 (2026-09-11)

- `PersistentManagers` 자동 부트스트랩과 Resources 프리팹을 추가하고 루트 하나만 `DontDestroyOnLoad`를 소유하도록 변경했다.
- `InputHandler`의 입력 람다를 이름 있는 콜백으로 교체하고 등록/해제를 대칭화했으며 런타임 전용 액션 에셋 복제본을 사용한다.
- `GameStateManager`는 씬 로드 완료 시 `Playing`과 `Time.timeScale = 1`을 복구한다.
- `AudioManager`는 전역 설정을 유지하고 씬 로컬 `EnvironmentManager`가 BGM 소스를 등록/해제하도록 변경했다.
- 10개 게임플레이 씬에서 기존 `InputHandler`, `GameStateManager`, `AudioManager`, 빈 `GameManager` 배치를 제거했다.
- `GameManager`의 잘못된 개별 영속화 호출을 제거했다.

## 최신 감사 (2026-09-11)

- `GameManager`, `GameStateManager`, `InputHandler`, `AudioManager`는 여전히 `Manager` 자식 상태에서 자신에게 `DontDestroyOnLoad(gameObject)`를 호출한다.
- Tutorial은 정상 Input Action Asset을 참조하지만 스테이지 씬들은 존재하지 않는 GUID를 참조한다.
- Resources fallback도 실제 에셋 배치와 일치하지 않는다.
- 전역/씬 로컬 관리자가 같은 부모에 혼합되어 있어 현재 `Manager` 전체 영속화는 안전하지 않다.
- Unity MCP Console에서 root 제한 경고와 Input Action Asset 미할당 오류를 확인했으므로 `확인됨 — 미해결` 상태가 최신이다.

## 관련 문서

- `.planning/phases/15-load-timing-and-load-scope/15-UAT.md`
- `.planning/phases/15-load-timing-and-load-scope/bugs/README.md`
- `Assets/SaveSystem/Script/SaveLoadManager.cs`
- `Assets/Player/Script/InputHandler.cs`
- `Assets/Player/Script/GameStateManager.cs`
- `Assets/Script/AudioManager.cs`
- `Assets/map/script/GameManager.cs`
- `Assets/Script/EnvironmentManager.cs`
- `Assets/Scenes/Tutorial Map.unity`
- `Assets/Scenes/1 stage.unity`
