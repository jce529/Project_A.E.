# BUG-011: ControlsSettingsPanel 의 BasicAttack 엔트리(bindingIndex 1) 범위 초과로 라벨 갱신이 중단됨

- 심각도: 중간 (기존 결함, 컨트롤 탭 라벨 표시와 Reset 동작에 영향)
- 상태: 확인됨 — 미해결 (Inventory 엔트리에 한해 우회 적용, 근본 수정은 범위 밖)
- 발견일: 2026-11-05
- 발견 경로: Phase 21-05 Play 검증 (컨트롤 탭 Inventory 행 리바인딩/초기화)
- 영향 범위: `Assets/Scenes/1 stage.unity`, `Assets/Scenes/Tutorial Map.unity` 의 `ControlsSettingsPanel.entries`, `Assets/Player/Script/Menu/ControlsSettingsPanel.cs`

## 증상 및 재현
1. Play 후 Esc -> 컨트롤 탭을 연다.
2. Attack(KB), Skill 1, Skill 2, Heal, Interact 행의 키 라벨이 `...` 로 남는다. 실제 바인딩은 E, R, 1, Q 로 정상이다.
3. `Reset to Default` 를 누르면 `ArgumentOutOfRangeException: Binding index 1 is out of range on action 'Player/BasicAttack' with 1 bindings` 가 발생한다.

## 기대 / 실제
기대: 모든 행의 라벨이 현재 바인딩을 표시하고 Reset 이 예외 없이 끝난다.
실제: `RefreshAllLabels` 가 `BasicAttack` 엔트리에서 예외를 던져 그 뒤에 있는 엔트리의 라벨이 갱신되지 않는다.

## 근거 / 원인
- 씬의 `ControlsSettingsPanel.entries` 에 `actionName: BasicAttack`, `bindingIndex: 1` 이 있으나 `InputSystem_Actions.inputactions` 의 BasicAttack 바인딩은 `<Mouse>/leftButton` 1개뿐이다 (인덱스 0).
- `RefreshLabel` 이 `action.GetBindingDisplayString(entry.bindingIndex)` 를 가드 없이 호출한다. (`ControlsSettingsPanel.cs`)
- 실측 (Play, 가상 입력 장치): `BasicAttack#1 disp=[NOBINDING]`, `Skill_1/Skill_2/Heal/Action` 라벨 `...`, 실제 `disp` 는 E/R/1/Q.
- 이 결함은 Phase 21 이전부터 존재했다. 21-05 의 변경은 씬에 행/엔트리를 추가만 했다.

## Phase 21 에서의 영향과 우회
Inventory 엔트리가 목록 마지막(13번째)이면 패널을 다시 열거나 Reset 할 때 라벨이 갱신되지 않아, 리바인딩한 키와 다른 라벨이 보인다 (D-03 위반).
우회: 두 씬에서 Inventory 엔트리를 `BasicAttack` 앞(인덱스 7)으로 옮겼다 (커밋 2239965). 행의 화면 위치는 그대로다.
우회 후 실측: 리바인딩(I) -> 탭 재진입 시 라벨 `I`, Reset 후 라벨 `Tab`. Reset 의 예외 자체는 여전히 발생한다.

## 수정 방향 및 완료 기준
- `BasicAttack` 엔트리의 `bindingIndex` 를 0 으로 바꾸거나 엔트리를 제거한다. 또는 `RefreshLabel` 에 범위 검사를 추가한다.
- 완료 기준: 컨트롤 탭을 열었을 때 모든 행의 라벨이 현재 바인딩을 표시하고 Reset 이 예외 없이 끝난다 (씬 2곳).
- 공격 행을 UI 로 리바인딩해도 되는지는 별도 결정이 필요하다 (이 phase 에서는 건드리지 않았다).

## 관련 phase 문서
[21-05-PLAN.md](../21-05-PLAN.md), [21-05-SUMMARY.md](../21-05-SUMMARY.md), [Check.md](../../../../Assets/Item/Check.md)
