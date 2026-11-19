# 플레이어 상호작용 이식 기록

2026-11-19. 기준 프로젝트: Unity 6000.3.10f1.

## 적용 구조

- `IPlayerInteractable`: `CanInteract(PlayerInteraction)` / `Interact(PlayerInteraction)`.
- `PlayerInteraction`: 기본 반경 2, 지정한 2D 레이어, 트리거를 포함한 콜라이더 탐색. 부모 컴포넌트도 찾고 중복 콜라이더를 제거한 뒤 가장 가까운 유효 대상 한 개를 선택한다. 거리가 같으면 Unity 오브젝트 ID 기반 해시로 결정한다. 반경은 대상 피벗 기준이다.
- `PlayerInteractionPrompt`: 선택된 대상 위에 현재 바인딩을 표시한다. 대상 삭제/풀 반환에도 살아 있도록 캔버스는 플레이어가 소유하며 월드 위치를 따라간다. 키 재지정 결과는 매 갱신에 반영한다.
- `InputHandler`가 있는 씬은 `OnInteractEvent`를 사용한다. 기존 `Player/Action`을 유지하며 `Player/Interact`도 지원한다. 매니저가 없으면 플레이어 `PlayerInput`의 해당 액션을 사용하고, 둘 다 없을 때만 F 바인딩을 만든다. 씬 전환과 매니저 교체 시 구독을 다시 연결한다.
- 일시정지, 메뉴, 대화/이동 잠금, 감옥 상태에서는 상호작용을 차단한다. 같은 프레임의 중복 호출은 한 번만 실행한다.

## 기존 기능 연결

| 대상 | 이식 후 동작 |
|---|---|
| Checkpoint | 선택 후 상호작용하면 활성 체크포인트와 PlayerRespawn 갱신. 기존 프로젝트에 없는 저장 슬롯 UI는 추가하지 않음 |
| SignpostPortal / Portal | 기존 목적지와 스폰 이름 유지. 로드 가능한 씬만 대상으로 표시 |
| InteractableWall | 기존 보스 사망 이벤트의 UnlockWall 유지. 기본적으로 플레이어가 열 수 없음. 직접 열 벽만 allowPlayerInteraction 활성화 |
| NPCDialogueTrigger | 기존 JSON 시작 노드로 대화 시작. 대화 시작 프레임의 클릭으로 첫 줄이 건너뛰지 않도록 방지 |
| ElevatorButton | 기존 onButtonPressed UnityEvent와 일회성 설정 유지 |
| InteractivePrompt | 기존 promptUI, interactionDistance, waterController 필드와 스크립트 GUID 보존. 기존 안내는 숨기고 공통 안내로 대체. 빈 물병이 있을 때 물 회복 |
| PureWater / CorruptedWater | 빈 병이 있을 때 기존 물 회복 함수를 호출. 상호작용 대상에 Collider2D가 필요함 |
| WaterPuddle | 물병 회복 후 파괴 불가로 전환. 이미 흡수했거나 물병이 가득 차면 후보에서 제외 |
| PlayerAbsorb | 기본 Interact 입력은 공통 디스패처가 전담. 기존 공격/스킬/회복 입력에 설정된 흡수 동작은 유지 |

기존 W 포털, S 체크포인트, Z 엘리베이터, NPC F, 물 Q 등의 개별 입력은 위 대상에서 공통 상호작용 키(기본 F)로 통합했다. 장애물 Q/공격 콤보/스킬 입력은 유지한다. 상호작용 키를 공격/스킬 키로 재지정한 경우, 유효 대상이 있으면 상호작용을 우선한다.

## 플레이어 컨트롤러

첨부 컨트롤러를 그대로 덮어쓰지 않고 현재 컨트롤러에 동작을 이식했다.

- Shift를 처음 누르면 대시, 누르고 있으면 달리기. `dashOnRunPress`를 끄면 이전 X 대시로 복귀한다.
- `dashSpeed`, `dashDuration`, `dashCooldown` 등 기존 직렬화 필드는 유지한다. 첨부의 `dashPower` 이름은 `dashSpeed`와 연결된 속성으로 제공한다.
- 대시 중에도 지형 콜라이더를 유지한다. 기존 대시 쿨다운도 유지한다.
- 기존 점프 감각, 사다리, 발판 내려가기, 장판, 넉백, 애니메이션, 대화 잠금과 `movementLocked`, `IsImprisoned`, `ApplyKnockback`, 공개 `IsGrounded` API를 보존한다.
- 기존 발판 전체를 Trigger로 바꾸지 않고 해당 플레이어와 발판 사이의 충돌만 잠시 무시한다.
- Player 프리팹과 1 stage의 직접 배치 플레이어에 상호작용/안내 컴포넌트를 연결했다. Tutorial Map은 Player 프리팹을 통해 상속한다. 런타임의 별도 플레이어 생성에도 컨트롤러가 누락 컴포넌트를 보완한다.

기존 `InteractivePrompt.cs`의 클래스 이름이 `InteractionPrompt`였던 불일치는 파일명과 같은 `InteractivePrompt`로 맞추고 `MovedFrom`을 지정했다. 파일과 .meta는 이동/삭제하지 않았다.

## 검증과 한계

전체 런타임/에디터 C# 소스는 원본 패키지의 컴파일 참조와 설치 엔진 API를 이용한 별도 Roslyn 컴파일로 확인했다. 원본 Unity 버전은 바꾸지 않았다. 설치된 엔진은 6000.6.9f1이므로 실제 Unity 동작 검증은 작업 폴더의 격리 프로젝트에서 수행했다. 격리 프로젝트에서는 해당 엔진용 uGUI와 Input System 에디터 호환 수정을 사용하며, 원본 Packages/Library에는 적용하지 않았다.

검증 결과: 런타임 171개 및 에디터 7개 소스 컴파일 통과. 격리 Unity 검사 18개 통과(직렬화 연결, 기존 안내 스크립트 타입 복구, 대상 거리/레이어/부모/중복/트리거, 입력 중복, 일시정지, 대상 삭제와 안내 재사용, 체크포인트, 보스 잠금 벽, 일회성 버튼, 물 회복/흡수, 매니저 구독 수명, 키 재지정, 컨트롤러 API와 대시 충돌, 대화 시작 클릭, 매니저 교체). 트리거 탐지 시 전역 쿼리 설정이 꺼져 있어도 해당 탐지에만 포함하고 즉시 원복한다.

원본에 기존부터 존재하는 다음 문제는 이번 작업에서 변경하지 않았다.

- MainMenuUI와 빌드 목록은 삭제된 InGame 씬을 참조한다. Tutorial Map은 빌드 목록에 없다. 전체 게임 시작/빌드 경로는 별도 정리가 필요하다.
- 기존 일반 적/보스/플레이어의 일부 초기화 메서드가 기반 클래스 메서드를 숨긴다는 컴파일 경고가 있다.
- 씬 전체를 원래 버전에서 플레이하며 밸런스와 연출까지 검증한 것은 아니다.

후속 수정 시 기존 미커밋 변경을 리셋하거나 .meta를 재생성하지 않는다.
