# Player 프리팹

## 책임

`Assets/Player.prefab`은 플레이어의 물리, 이동, 공격, 체력, 애니메이션, 스폰/리스폰 기능과 물 자원 child를 조립한다. 입력 singleton, UI, 저장 manager와 씬의 스폰 지점은 프리팹 외부 의존성이다.

## 직렬화 구성

| 항목 | 확인된 구성 |
|---|---|
| 루트 | `Player` GameObject에 Rigidbody2D, Collider2D, SpriteRenderer와 플레이어 스크립트들이 결합 |
| 물 | `WaterController` child component |
| 공격 참조 | `waterController`와 `playerStats`는 로컬 fileID로 연결; `attackBox`는 `{fileID: 0}` |
| 체력 | `maxHealth: 400`, `maxTotalHealth: 200` |
| 스폰 | `PlayerSpawner` 및 `PlayerRespawn` component 존재 |

## 런타임 가정과 제약

- `PlayerController.Awake`는 같은 GameObject의 Rigidbody2D, SpriteRenderer, CapsuleCollider2D, PlayerAnimator를 가져오므로 구성 누락 시 null 사용 위험이 있다.
- `PlayerAttack`은 기본 공격에서 `attackBox`를 Instantiate한다. 현재 null 직렬화 값 때문에 다른 런타임 할당 경로가 없다면 기본 공격 생성은 실패한다.
- `PlayerStats`의 성장 조건은 `maxHealth < maxTotalHealth`인데 serialized 값은 반대이므로 초기 구성에서는 `AddHealth`가 최대 체력을 늘리지 않는다.

## 근거

- `Assets/Player.prefab`
- `Assets/Player/Script/PlayerController.cs:59`
- `Assets/Player/Script/PlayerAttack.cs:84`
- `Assets/Player/Script/PlayerStats.cs:38`
- `Assets/Player/Script/WaterController.cs:79`
- `Assets/map/script/PlayerSpawner.cs:8`

## 검증

`editor-verification-required` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`. YAML component 및 local reference는 확인했지만 프리팹 import/compile/Play Mode와 null 공격 참조의 실제 결과는 확인하지 않았다.

