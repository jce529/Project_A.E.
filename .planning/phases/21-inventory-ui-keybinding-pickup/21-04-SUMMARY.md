---
phase: 21-inventory-ui-keybinding-pickup
plan: 04
subsystem: ui
tags: [unity, inventory, toast]
requirements-completed: [D-10, D-12, D-13, D-14]
completed: 2026-11-05
---

# Phase 21 Plan 04: 월드 아이템 획득 토스트 Summary

`WorldItem.PickupResolved(ItemData, int, bool)` 정적 이벤트와, `InventoryUI` 의 토스트(unscaled 2초, 교체 방식)를 추가했다. 토스트는 `Inventory.OnChanged` 에 연결하지 않았다.

## Task Commits

| Task | 커밋 |
|---|---|
| 1. `WorldItem.PickupResolved` 발행 (Destroy 는 성공 시에만) | 79b20a7 |
| 2. 토스트 라벨, unscaled 타이머, 구독/해제 | a53a7ce |

## 실행 편차

- Codex(`gpt-6.1-terra`)가 한글 문자열을 UTF-8 로 정상 저장했다(`x{count} 획득`, `인벤토리가 가득 찼습니다` 바이트 검사 `true true false`).
- `InventoryUI.cs` 는 baseline(5b19e28) 대비 삭제 0줄 / 추가 34줄(순수 삽입, 기준 30줄 이상).

## Play 검증

토스트는 CLI 왕복 지연(1~2초)이 지속 시간(2초)에 가까워 직접 읽기가 불가능해서, 에디터 안에서 매 업데이트 토스트 상태 변화를 시간과 함께 기록하는 프로브로 측정했다.

- [x] 1. 성공 토스트: `38.89 [체력 물약 x3 획득]` -> `40.81 hidden` (1.92초), 월드 오브젝트 소멸, 슬롯 `health_potion_01 x2` 확인 (픽업 실경로: F 입력 -> `PlayerInteraction` -> `WorldItem.Interact`)
- [x] 2. 열쇠: `[고대의 열쇠 x1 획득]` 표시, 슬롯 `ancient_key_01 x1`
- [x] 3. 가득 참: 20슬롯 모두 99개로 채운 뒤 픽업 -> `63.54 [인벤토리가 가득 찼습니다]` -> `65.50 hidden` (1.96초), **월드 오브젝트 생존**(`worldObjAlive=True`, 별도 측정) (Phase 18 D-06 유지)
- [x] 4. 교체 방식: 한 `eval` 안에서 이벤트 2회 연속 발생 -> 마지막 메시지 `[고대의 열쇠 x1 획득]` 하나만 표시, `Pickup Toast` 라벨은 1개
- [x] 5. unscaled: 토스트 표시 직후 패널을 열어 `timeScale=0` 이 된 상태에서 `125.26 [체력 물약 x9 획득]` -> `127.22 hidden ts=0 st=Inventory` (1.95초, timeScale 0 중에도 감소)
- [x] 6. 세이브 복원: `CaptureInventoryItems` -> 슬롯 비움 -> `ApplyInventoryFromSave` 동안 `PickupResolved` 이벤트 0회, 토스트 숨김 유지
- [ ] 7. "연속 획득 시 타이머 재시작" 의 정확한 재시작 시점은 CLI 지연 때문에 시간으로 증명하지 못했다 (교체 동작만 확인).
