# BUG-009: 플레이어 이동 시 상호작용 키 UI 떨림

- 심각도: 중간 — 시각적 결함, 입력 실행 실패는 보고되지 않음
- 상태: 해결됨
- 발견일: 2026-09-12
- 발견 경로: Phase 15 BUG-007 후속 확인 대화에서 사용자가 실제 플레이 현상 보고; quick-260912-kih 가져오기 이후 관찰
- 영향 범위: PlayerInteraction / PlayerInteractionPrompt와 CameraController의 화면 좌표 동기화
- 추적 위치: Phase 15 후속 발견으로 이 폴더에서 관리. 기능 도입 출처는 quick-260912-kih이며 로드 버그 자체와는 별개다.

## 증상과 재현 절차

사용자 보고: “객체 위에 생성된 F키 UI가 플레이어가 움직일 때마다 떨려.”

1. 상호작용 대상 근처로 이동해 키 안내 UI를 표시한다.
2. 대상이 범위 안에 있는 상태로 플레이어를 이동한다.
3. 대상 위 안내문이 대상과 일정한 간격을 유지하는지 관찰한다.

- 기대: 카메라가 움직여도 같은 대상의 지정된 월드 오프셋에 안내문이 안정적으로 붙어 있다.
- 실제: 이동 중 안내문이 떨린다는 사용자 확인. 정확한 씬, 대상 종류, 프레임률, 단일/다중 대상 여부는 미수집.
- 이번 조사에서 Unity 재현 및 프레임 계측은 실행하지 않았다. 현상 확인과 원인 확정을 구분한다.

## 수정 전 코드 근거 및 원인 분석

1. `Assets/Player/Script/PlayerInteraction.cs`의 `LateUpdate()` → `RefreshTargetAndPrompt()` → `prompt.Show()`가 대상 선택과 UI 위치 갱신을 수행한다.
2. `Assets/Player/Script/PlayerInteractionPrompt.cs`의 `Show()`는 `Camera.main.WorldToScreenPoint(target.transform.position + worldOffset)` 결과를 ScreenSpaceOverlay의 `label.rectTransform.position`에 즉시 넣는다. 해당 좌표는 이후 카메라 이동에 자동으로 재투영되지 않는다.
3. `Assets/Camera/Script/CameraController.cs`의 `LateUpdate()`는 추적 위치, 데드존, orthographicSize, 경계 보정, 마지막 피격 흔들림까지 카메라를 변경한다.
4. 조사한 두 클래스에는 DefaultExecutionOrder가 없으며 각 .meta에도 executionOrder 설정이 없다. 카메라 최종 갱신 이후 UI를 배치한다는 명시적 순서가 없다.

**가장 유력한 원인: 카메라의 최종 상태와 UI 화면 좌표 계산 시점 불일치.** UI LateUpdate가 카메라 LateUpdate보다 먼저 실행되면 이전 카메라 상태로 계산한 안내문과 최종 카메라로 렌더링된 대상 사이에 오차가 생긴다. 이동/줌/흔들림에 따라 오차가 달라져 떨림처럼 보일 수 있다. 실행 순서가 매 프레임 무작위로 바뀐다는 주장은 아니다.

이는 정적 코드로 확인한 동기화 결함 후보이며, 실제 보고 현상의 단독 원인인지는 미확정이다. Unity 문서도 서로 다른 GameObject의 같은 이벤트 함수 순서에 의존하지 말도록 명시한다: https://docs.unity3d.com/es/2020.2/Manual/ExecutionOrder.html

추가 구분 대상:

- 여러 후보 사이에서 최근접 대상이 바뀌는 현상: FindNearest는 매 프레임 거리를 비교하고 선택 유지 여유 폭이 없다. 대상 ID 변경 로그로 구분해야 한다.
- 소수 픽셀 좌표에 따른 글자 가장자리 떨림: 현재 화면 좌표를 반올림하지 않는다. 실제 위치 오차와 글꼴 렌더링 문제를 구분해야 한다.
- Canvas가 플레이어 자식인 사실만으로 이동 상속이 원인이라고 단정하지 않는다. 현재 Canvas는 ScreenSpaceOverlay다.

## 초기 제안 수정 방향 및 검증 (최종 구현은 해결 기록 참고)

대상 선택/입력 처리와 표시 위치 갱신을 분리하고, 카메라의 추적·줌·경계·흔들림 적용이 모두 끝난 뒤 최종 화면 좌표를 계산하도록 순서를 보장한다. 실행 순서 지정 또는 적합한 렌더 직전 갱신 경로를 검토한다. 임의의 UI 보간은 오차 원인 확인 전 적용하지 않는다.

원인 확정용 계측: 단일 정지 대상을 두고 프레임 번호, 대상 ID, Show 시점 카메라 위치/줌, 카메라 LateUpdate 종료 위치/줌, 최종 투영점과 실제 label 위치 차이를 기록한다. 카메라 갱신 이후 배치하는 실험으로 오차 및 떨림이 사라지는지 비교한다.

## 완료 조건

- [x] World Space Canvas를 대상의 자식으로 배치하고 높이 오프셋 적용.
- [x] 줌에 따른 글자 확대/축소 및 화면 좌표 계산 제거.
- [x] C# 빌드 통과(오류 0).
- [x] 수정 후 사용자 확인 및 완료 승인: 2026-09-13 “확인완료했어 완료처리해줘”.

사용자의 최종 확인을 Play 수용 검증 근거로 해결 처리한다. 개별 이동·피격·키 재설정 등 세부 시나리오별 결과나 프레임 계측 로그는 별도 수집하지 않았으므로 자동 검증 통과로 주장하지 않는다. 정확한 수정 커밋은 아래 해결 기록에 기재한다.
## 관련 문서

- ../15-UAT.md
- README.md
- ../../../quick/260912-kih-centralize-interaction-detection-on-play/260912-kih-PLAN.md
- ../../../quick/260912-kih-centralize-interaction-detection-on-play/260912-kih-SUMMARY.md (시각적 표시/배치는 미검증으로 기록됨)

## 해결 기록

- 수정일: 2026-09-13
- 상태: 해결됨. 해결일: 2026-09-13.
- 확정 계획: 사용자 승인에 따라 World Space Canvas로 전환. 대상의 자식으로 배치하며 줌에 따라 글자도 함께 확대/축소한다.
- 구현: PlayerInteractionPrompt에서 RenderMode.WorldSpace를 사용한다. Canvas를 재사용하여 선택된 대상의 자식으로 옮기고 localPosition에 기존 worldOffset(기본 0, 1.5, 0)을 적용한다. worldScale 기본값 0.02로 28pt 글자를 월드 크기로 조절한다. Inspector에서 높이와 크기를 조절할 수 있다. 부모의 회전과 스케일도 상속한다.
- 이전 Overlay 수정의 화면 좌표 계산과 Canvas 렌더 콜백은 제거했다. 대상 선택·상호작용 입력·키 재설정 표시는 유지했다.
- 수명 관리: 숨길 때 Canvas를 비활성화하고 플레이어 아래로 회수한다. 대상과 함께 Canvas가 파괴되면 다음 Show에서 다시 만든다. 플레이어의 Prompt가 파괴될 때 대상 아래에 남은 Canvas도 정리한다.
- 수정 커밋: `22e1a21c3ef0a7f49bb7c89544e484fdc8d57747` (fix(interaction): attach world-space key prompts to targets).
- 검증: `dotnet build Assembly-CSharp.csproj --verbosity quiet` 성공(오류 0, 다른 파일의 기존 경고 6). 첫 --no-restore 시도는 Unity Temp의 project.assets.json 부재로 실패했으며 정상 복원을 포함한 빌드로 통과했다.
- Play 검증: 2026-09-13 사용자 “확인완료했어 완료처리해줘”로 수정 결과 확인 및 종료 승인.
- 종료 근거: 위 사용자 수용 확인과 기존 빌드 성공. 세부 시나리오별 독립 계측은 미수집.