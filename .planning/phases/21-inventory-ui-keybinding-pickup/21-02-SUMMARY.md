---
phase: 21-inventory-ui-keybinding-pickup
plan: 02
subsystem: items
tags: [unity, scriptableobject, inventory]
requirements-completed: [D-06, D-07, D-09]
completed: 2026-11-05
---

# Phase 21 Plan 02: ItemData 표시 메타데이터 Summary

`ItemData` 에 `icon`/`displayName` 필드와 `Icon`/`DisplayName`/`Type` 접근자를 추가하고 두 에셋을 채웠다. `Id`, `UseEffect`, 기존 필드는 바뀌지 않았다.

## Task Commits

| Task | 커밋 |
|---|---|
| 1. `ItemData.cs` 필드 2 + 접근자 3 | ef1e17b |
| 2. `HealthPotion.asset`, `AncientKey.asset` 값 채우기 | 0ca2a3f |

## 실행 편차

- **Codex 한글 깨짐:** Codex 가 `displayName` 을 `?`(리터럴 물음표)로 저장했다. 오케스트레이터가 Node 로 UTF-8 값(`체력 물약`, `고대의 열쇠`)을 다시 써서 바이트 단위로 확인(BOM 없음, CRLF 유지)한 뒤 커밋했다. 이후 한글이 들어가는 작업은 바이트로 검증했다.
- **커밋 주체:** Codex 샌드박스가 `.git` 에 쓰지 못해 커밋은 오케스트레이터가 했다.

## Play 검증

- [x] 1. 컴파일 에러 0
- [x] 2. UI 경로 실측 (아래 3 항목의 `Resources.LoadAll` 직접 출력은 에디터가 모달 대화상자로 막혀 마지막에 재실행하지 못했다. 대신 패널이 읽은 값으로 확인)
  - HealthPotion 슬롯: 아이콘 `white_filled_water_bottle_0`, 수량 `3`, 표시 이름 `체력 물약` (선택 라벨 `체력 물약 x2`)
  - AncientKey 슬롯: 아이콘 없음, 텍스트 폴백 `고대의 열쇠`, 선택 라벨 `고대의 열쇠 x1`
- [x] 3. 회귀: 체력 50 상태에서 UI 로 HealthPotion 사용 -> 체력 70 (+20), 수량 3 -> 2. AncientKey 는 UI 에서 사용이 차단되어 수량 1 유지 (D-09)
- [x] 4. 세이브 복원 (`ResolveItemData` 경로): 복원 전후 슬롯 동일, `[SaveLoadManager] Restored 3/3 item entries.` — unknown itemId 경고 없음
- [ ] 5. 에셋 재직렬화 diff: Unity 가 에셋을 다시 저장했는지 확인하지 못했다(에디터 응답 불가). 작업 트리 diff 는 두 파일 모두 추가 2줄, 삭제 0줄이고 `id/type/effectType/amount` 는 불변.
- [ ] 6. `Resources.LoadAll<ItemData>("Items")` 한 줄 출력 (`Id | Type | DisplayName | Icon`): **미실행**. 위 UI 실측으로 갈음하며 이 항목은 PASS 로 적지 않는다.
