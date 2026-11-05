---
phase: 21-inventory-ui-keybinding-pickup
plan: 05
subsystem: ui
tags: [unity, input-system, rebinding, scene]
requirements-completed: [D-03]
completed: 2026-11-05
---

# Phase 21 Plan 05: 컨트롤 탭 Inventory 리바인딩 행 Summary

`1 stage`, `Tutorial Map` 씬의 `ControlsSettingsPanel` 에 Inventory 행과 엔트리를 Unity CLI eval 스크립트로 추가했다. `ControlsSettingsPanel.cs` 는 수정하지 않았다.

## Task Commits

| Task | 커밋 |
|---|---|
| 1. 씬 편집 스크립트 | 225e7e5, 0527fdf (eval 래퍼 형식 수정) |
| 2. 두 씬 편집 | bf417ed |
| (편차) 엔트리 순서 수정 | 2239965 |

실행 보고 (스크립트 출력): 두 씬 모두 `row y=-738, reset y=-793, content h=828, entries=13`. 재실행 시 두 씬 모두 `Inventory entry already present - skipped` (멱등).

## 실행 편차

- **eval 형식:** `unity command eval` 은 스크립트를 메서드 본문으로 감싸 `using` 지시문을 받지 못한다. `using` 을 제거하고 타입을 전체 이름으로 바꿨다(로직 동일).
- **메인 에디터 사용:** 워크트리에는 `Library` 가 없어, 메인 체크아웃의 에디터에서 스크립트를 실행하고 **복사 전/후 씬의 diff 만** 이 브랜치에 패치로 적용했다(메인 씬에는 사용자의 커밋 안 된 Spirit 변경이 있어 섞이지 않게 함). 씬 변경은 각 씬 추가 487줄 / 삭제 2줄.
- **엔트리 순서 수정 (계획 밖):** 리바인딩 후 패널을 다시 열면 Inventory 라벨이 실제 키와 달라지는 문제를 Play 에서 발견했다. 기존 `BasicAttack bindingIndex: 1`(범위 밖)이 `RefreshAllLabels` 에서 예외를 던져 그 뒤 엔트리의 라벨이 갱신되지 않기 때문이다 ([BUG-011](bugs/BUG-011-controls-basicattack-bindingindex-out-of-range.md)). Inventory 엔트리를 `BasicAttack` 앞으로 옮겼다(씬 4줄 이동). 행의 화면 위치는 그대로다.

BasicAttack bindingIndex: 1 - 관찰됨, 미수정 (근본 수정은 범위 밖. 우회만 적용, BUG-011 에 기록)

## Play 검증

- [x] 1. Esc -> 컨트롤 탭: Inventory 행이 Interact 행 아래에 키 라벨 `Tab` 으로 표시, Reset 버튼과 겹치지 않음 (캡처: `evidence/21-controls-tab-inventory-row.png`)
- [x] 2. 키 버튼 클릭 -> 대기 오버레이 `True` -> `I` 입력 -> 라벨 `I`, `effectivePath=<Keyboard>/i`, `SaveLoadManager.CurrentSettings.InputBindingsJson` 에 `<Keyboard>/i` 포함
- [x] 3. 메뉴를 닫고 `I` -> 패널 열림/닫힘, `Tab` -> 반응 없음 (`state=Playing` 유지)
- [x] 4. (순서 수정 후) 탭을 나갔다 다시 열면 라벨 `I`, Reset to Default -> 라벨 `Tab`, `effectivePath=<Keyboard>/tab`, `Tab` 으로 다시 토글
- [ ] 5. Tutorial Map 에서의 행 표시는 두 씬 모두 같은 스크립트/패치이고 직렬화 diff 가 동일하나, 화면 확인은 Tutorial Map 한 곳에서만 했다 (`1 stage` Play 는 하지 않음).
- [ ] 6. 기존 행 회귀: Jump/Dash/Run/Move 라벨은 정상. Skill 1/2, Heal, Interact 는 `...` 로 보임 (기존 결함, BUG-011, 실제 바인딩은 E/R/1/Q). Reset 은 기존 `ArgumentOutOfRangeException` 이 계속 발생(BUG-011).
- [ ] 7. 구버전 저장 오버라이드 JSON 로드: 해당 없음 (저장된 설정 파일 없음, `No settings file - using defaults.`)
