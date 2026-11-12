---
phase: 21-inventory-ui-keybinding-pickup
plan: 03
subsystem: ui
tags: [unity, ugui, inventory]
requirements-completed: [D-04, D-05, D-08, D-09, D-10, D-11]
completed: 2026-11-05
---

# Phase 21 Plan 03: 런타임 인벤토리 패널 Summary

`Inventory.OnChanged` 이벤트와, 씬/프리팹 수작업 없이 `Inventory.Awake` 가 붙이는 `InventoryUI`(5x4 패널, Tab 토글, Esc 지연 닫기, 좌클릭 선택, 우클릭 사용, Progression 차단)를 추가했다.

InventoryUI baseline: 5b19e28face52d6fcb31e4fa5ff7b53c4019266c (288줄, `wc -l`)

## Task Commits

| Task | 커밋 |
|---|---|
| 1+2. `Inventory.OnChanged`, `InventoryUI.cs`(+.meta), `AddComponent` 1줄 | 5b19e28 |

## 실행 편차

- **Task 1/2 한 커밋:** `Inventory.cs` 의 두 변경(`OnChanged`, `AddComponent`)이 한 파일에 섞여 있어 한 커밋으로 묶었다. 커밋 grep(`(21-03)`) 기반 검증에는 영향 없다.
- Codex(`gpt-6.1-terra`, medium)가 계획의 코드 블록을 그대로 옮겼다(Codex 자체 검사에서 계획 코드와 동일 확인). 커밋은 오케스트레이터.
- `InventorySlotView` 분리 불필요 (AddComponent 정상).

## Play 검증

검증 환경: 메인 체크아웃의 Unity 에디터, 가상 키보드/마우스 입력, 슬롯 클릭은 `ExecuteEvents.pointerClickHandler` (좌/우 버튼 `PointerEventData`).

- [x] 1. 컴파일 에러 0. Player 에 `InventoryUI`, `Inventory UI Canvas` 하위 `InventorySlotView` 20개
- [x] 2. Tab -> `state=Inventory ts=0 panel=True`, HUD 요소 3개 모두 `activeSelf=False`. 캡처: `evidence/21-inventory-panel-open.png`
- [x] 3. Tab 다시 -> `state=Playing ts=1 panel=False`
- [x] 4. Tab 으로 연 뒤 Esc 3회 반복 -> 매번 `state=Playing ts=1 panel=False pauseMenu=False` (3/3 동일, 일시정지 메뉴 동시 열림 없음)
- [x] 5. Esc 로 일시정지 메뉴 열기 -> `state=Paused pauseMenu=True`, 그 상태에서 Tab -> `state=Paused panel=False` (열리지 않음). Esc 로 닫으면 `Playing`
- [x] 6. 표시: HealthPotion `icon=white_filled_water_bottle_0 name=[] count=[3]`, AncientKey `icon=none name=[고대의 열쇠] count=[1]`, 빈 슬롯 전부 빈 문자열
- [x] 7. 빈 슬롯 좌클릭 -> 선택/라벨 변화 없음. HealthPotion 좌클릭 -> 라벨 `체력 물약 x2`(다음 프레임), 슬롯 `selected`
- [x] 8. 체력 `50` 상태에서 선택된 HealthPotion 우클릭 -> 수량 3->2, 체력 `70`, 라벨 `체력 물약 x2` 즉시 갱신. 2회 더 -> 슬롯 비워짐(`IsEmpty=True`), 라벨 빈 문자열
- [x] 9. AncientKey 좌클릭 후 우클릭 2회 -> 수량 `1` 유지(D-09). 선택 안 된 슬롯 우클릭은 선택만 됨(수량 불변)
- [x] 10. 열린 동안 F/LMB/Space/X/1/E/R/Q -> 이벤트 0 (21-01 카운터 실측), 닫은 뒤 전부 정상
- [ ] 11. 세션 전체 Console 예외 0건: **미충족.** `CameraBoundsTrigger.OnTriggerExit2D` NRE 1회(Phase 21 코드 아님, `bugs/README.md` 관찰 항목), 컨트롤 탭 Reset 의 기존 결함(BUG-011). `InventoryUI`/`Inventory`/`WorldItem` 에서 난 예외는 0건.

미확인: 실제 물리 키보드/마우스 입력의 "느낌"은 확인하지 못했다(가상 장치 이벤트로 대체).
