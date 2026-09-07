# 플레이어 런타임

## 책임

플레이어 이동, 입력 이벤트 소비, 체력, 근접/스킬 공격, 물병 자원을 소유한다. 저장 파일 I/O와 씬 전환은 소유하지 않으며 `SaveLoadManager`와 `PlayerSpawner`가 담당한다.

## 런타임 구조

| 영역 | 구현 | 주요 상태 |
|---|---|---|
| 입력 | `InputHandler` | Input System 액션을 이벤트로 변환하고 바인딩 override를 설정 메모리에 반영 |
| 이동 | `PlayerController` | 걷기/달리기, 1회 점프, 대시, 사다리, 일방향 플랫폼 통과, 지면 속도 modifier |
| 전투 | `PlayerAttack` | 3단 기본 콤보, 물 강화, 도약 및 Q 액션 |
| 체력 | `PlayerStats : HP` | 현재/최대/성장 상한 체력, 피해 시 카메라 흔들림, 저장값 복원 |
| 물 자원 | `WaterController` | `[채움 여부, 오염 여부]` 배열의 병 목록과 순수/오염 모드 |

## 동작

- `PlayerController.Start`가 `InputHandler.Instance`의 이동·점프·달리기·대시 이벤트를 구독하고 `FixedUpdate`에서 Rigidbody2D 속도를 적용한다. 이동 잠금, 대시, 넉백 중에는 일반 이동을 건너뛴다.
- 기본 공격은 공격 방향에 `attackBox`를 생성하고 3타에 피해량/쿨다운 변형을 적용한다. 공격 박스가 없거나 `SpriteRenderer`가 없으면 정상 실행되지 않는다.
- `HP.Awake`는 시작 체력을 최대 체력으로 설정한다. `PlayerStats.RestoreStats`는 최대 성장 체력, 최대 체력, 현재 체력 순서로 복원한 뒤 clamp 및 UI callback을 수행한다.
- `WaterController.Start`는 병 5개를 추가한다. 매 프레임 순수 물과 오염된 물의 개수에 따라 두 모드 플래그를 갱신한다.

## 의존성과 Unity wiring

- `Player.prefab`은 `PlayerController`, `PlayerAttack`, `PlayerStats`, `PlayerSpawner`를 보유하고, 자식 `WaterController`를 `PlayerAttack.waterController`에 연결한다.
- `PlayerAttack.playerStats`는 같은 프리팹의 `PlayerStats`를 참조하지만 `attackBox`는 null로 직렬화되어 있다.
- 레이어 마스크, Rigidbody2D, Collider2D, Animator 및 입력 액션의 실제 런타임 유효성은 Editor/Play Mode 확인이 필요하다.

## 근거

- `Assets/Player/Script/InputHandler.cs:75`
- `Assets/Player/Script/PlayerController.cs:59`
- `Assets/Player/Script/PlayerController.cs:116`
- `Assets/Player/Script/PlayerController.cs:161`
- `Assets/Player/Script/PlayerAttack.cs:62`
- `Assets/Player/Script/PlayerStats.cs:54`
- `Assets/Player/Script/PlayerStats.cs:70`
- `Assets/Player/Script/WaterController.cs:79`
- `Assets/Script/HP.cs:35`
- `Assets/Player.prefab`

## 검증

`partial` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`. 코드 계약과 프리팹 참조는 확인했지만 공격 박스 참조와 Play Mode 동작은 확인되지 않았다.

