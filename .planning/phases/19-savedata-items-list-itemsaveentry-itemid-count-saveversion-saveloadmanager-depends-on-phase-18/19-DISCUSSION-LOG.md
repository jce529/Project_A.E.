# Phase 19: 아이템 저장/로드 연동 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-09-21
**Phase:** 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18
**Areas discussed:** itemId → ItemData 에셋 해석 방식, 저장/복원 시 슬롯 위치 보존 여부, 저장 트리거/범위, 구버전 세이브 마이그레이션

---

## itemId → ItemData 에셋 해석 방식

| Option | Description | Selected |
|--------|-------------|----------|
| Resources 폴더 + Resources.Load | Assets/Item/*.asset을 Assets/Resources/Items/로 이동. Resources.Load<ItemData>("Items/"+id)로 해석. 가장 단순, 빌드에서도 동작. 단점: Resources 폴더 관례적 비판(로드 시간, 메모리) — 아이템 개수가 적은 이 프로젝트에서는 무관. | ✓ |
| 수동 관리 ItemDatabase ScriptableObject | 모든 ItemData를 등록해두는 단일 에셋(List<ItemData>)을 새로 만들고 SaveLoadManager가 참조. 새 아이템 추가 시 이 리스트에 수동 등록이 필요해 깜빡 위험이 있음. | |

**User's choice:** Resources 폴더 + Resources.Load
**Notes:** 새 아이템 추가 시 등록을 깜빡할 위험을 없애기 위해 레지스트리 방식은 배제.

---

## 저장/복원 시 슬롯 위치 보존 여부

| Option | Description | Selected |
|--------|-------------|----------|
| 순차 재생성 | 빈 아니뇸 슬롯만 List<ItemSaveEntry>로 저장하고, 로드 시 순서대로 TryAddItem 반복 호출. 같은 아이템이 여러 슬롯에 나뉘어 있었다면 로드 후 합쳐질 수 있음(Phase 18 TryAddItem의 기존 스택 우선 채움 로직 때문). UI가 없는 이 phase에서는 사용자가 차이를 볼 수 없음. | ✓ |
| 정확한 슬롯 위치 보존 | ItemSaveEntry에 slotIndex 필드를 추가해서 로드 시 각 슬롯에 직접 대입. 향후 UI가 생겼을 때 슬롯 위치가 이동하지 않는 대응이 생기지만, ROADMAP이 이미 ItemSaveEntry(itemId,count)로 이름 구조를 못박아 재확인이 필요. | |

**User's choice:** 순차 재생성
**Notes:** ROADMAP에 이미 명시된 ItemSaveEntry(itemId,count) 스키마와 일치. 인벤토리 UI가 없어 차이가 관측되지 않음.

---

## 저장 트리거/범위

| Option | Description | Selected |
|--------|-------------|----------|
| Save() 내부에서 항상 | CapturePlayerStats()와 동일한 패턴으로 Save() 안에 CaptureInventoryItems() 호출 추가. 체크포인트/보스처치/일시정지 메뉴 등 Save()를 거치는 모든 트리거가 자동 포함. Inventory가 없는 씬(메인메뉴 등)에서 Save가 불린다면 null 체크 필요. | ✓ |
| 명시적으로 체크포인트/보스처치만 | SaveAtCheckpoint()와 SaveOnBossDefeated()에서만 인벤토리 캡처. SaveAnywhere()(일시정지 메뉴)는 제외. 일관성은 떨어지지만 원하면 선택 가능. | |

**User's choice:** Save() 내부에서 항상
**Notes:** 기존 CapturePlayerStats() 패턴과 일관성 유지.

---

## 구버전 세이브 마이그레이션

| Option | Description | Selected |
|--------|-------------|----------|
| 버전만 올리고 EnsureCollections로 방어 | SaveVersion=3으로 올리고, 기존 List<string> Items 필드는 List<ItemSaveEntry>로 타입만 교체. 구버전 파일은 항상 빈 배열이었으므로 JSON 역직렬화가 타입 불일치 없이 자연스럽게 빈 List<ItemSaveEntry>가 됨. 별도 마이그레이션 함수 불필요. | ✓ |
| 명시적 마이그레이션 함수 추가 | SaveVersion 값을 보고 2이면 별도 변환 로직을 타는 MigrateFromV2() 같은 함수 추가. 지금은 사실상 no-op이지만 향후 마이그레이션 패턴을 미리 세우는 의미가 있음. | |

**User's choice:** 버전만 올리고 EnsureCollections로 방어
**Notes:** Items 필드가 지금까지 실제로 채워진 적이 없는 스텁이라 마이그레이션 리스크가 사실상 없음.

---

## Claude's Discretion

- Resources.Load 시 파일명 기준 매칭 vs Resources.LoadAll + Id 필드 매칭 중 최종 선택.
- ItemSaveEntry 클래스가 별도 파일인지 SaveData.cs 내 동반 타입인지.
- CaptureInventoryItems()/복원 함수의 정확한 이름과 시그니처.
- 로드 시점에 Inventory.Awake() 초기화 이후 복원 로직을 실행하는 정확한 훅 위치.

## Deferred Ideas

- ItemSaveEntry.slotIndex로 정확한 슬롯 위치 보존 — 인벤토리 UI(드래그앤드롭) phase에서 필요해지면 추가.
- ItemDatabase 레지스트리 SO로 전환 — Resources.Load 방식의 한계가 실제로 문제가 되면 재검토.
- 아이템 정렬/자동 정리 — Phase 18에서 이미 백로그로 넘어간 항목.
