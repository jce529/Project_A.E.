# Phase 19: 아이템 저장/로드 연동 - Context

**Gathered:** 2026-09-21
**Status:** Ready for planning

<domain>
## Phase Boundary

`SaveData.Items`를 `List<string>` 스텁에서 `List<ItemSaveEntry>(itemId, count)`로 교체하고,
`SaveVersion` 마이그레이션을 처리하며, `SaveLoadManager`가 `Save()`/`LoadGame()` 흐름에서
Player의 `Inventory`(Phase 18)를 캡처/복원한다.

**범위 밖**: 인벤토리 UI, 슬롯 위치 보존(정확한 슬롯 인덱스 저장), 아이템 정렬/자동 정리.

</domain>

<decisions>
## Implementation Decisions

### itemId → ItemData 자산 해석
- **D-01:** `Assets/Item/*.asset`(현재 `HealthPotion.asset`, `AncientKey.asset`)을
  `Assets/Resources/Items/`로 이동(또는 복사)하고, 로드 시 `Resources.Load<ItemData>("Items/" + id)`
  로 해석한다. 별도의 수동 관리 `ItemDatabase` 레지스트리 SO는 채택하지 않는다 — 새 아이템 추가 시
  등록을 깜빡할 위험을 없애기 위함. `id`와 파일명이 다를 수 있으므로 `Resources.Load`는 **파일명**
  기준이지 `ItemData.Id` 필드 기준이 아니다(둘이 다르면 해석 실패) — 계획 단계에서 파일명을
  `id`와 일치시키거나, `Resources.LoadAll<ItemData>("Items")`로 전체를 훑어 `Id` 필드로 매칭하는
  대안 중 하나를 확정한다(Claude's Discretion — 아래 참고).

### 슬롯 위치 보존 여부
- **D-02:** 정확한 슬롯 인덱스는 저장하지 않는다. `ItemSaveEntry`는 `itemId`/`count` 두 필드만
  가진다(ROADMAP이 이미 이 스키마를 못박음). 캡처 시 비어있지 않은 슬롯만 순서대로
  `List<ItemSaveEntry>`에 담고, 로드 시 그 순서대로 `Inventory.TryAddItem()`을 반복 호출해
  재구성한다. 같은 아이템이 서로 다른 슬롯에 나뉘어 있었다면, 로드 후에는 `TryAddItem`의 기존
  스택 우선 채움 로직(Phase 18 Pattern 3)에 따라 더 적은 슬롯으로 합쳐질 수 있다 — 이는 의도된
  동작이며 버그가 아니다. 인벤토리 UI가 없는 이 phase에서는 사용자가 슬롯 위치 차이를 직접 볼
  방법이 없으므로 허용 가능하다.

### 저장 트리거/범위
- **D-03:** 인벤토리 캡처는 `Save()` 내부에서 항상 수행한다 — 기존 `CapturePlayerStats()`와
  동일한 패턴으로 `CaptureInventoryItems()`를 추가해 `Save()` 본문에서 호출한다. 체크포인트,
  보스 처치, `SaveAnywhere()`(일시정지 메뉴) 등 `Save()`를 거치는 모든 트리거가 자동으로
  인벤토리를 포함한다. `PlayerStats.Instance`처럼 `Inventory`도 씬에 없을 수 있는 상황
  (예: 메인메뉴에서 호출)을 대비해 `CapturePlayerStats()`와 동일한 null 체크 관행을 따른다
  (`Inventory` 탐색 결과가 null이면 경고 로그 후 캡처 스킵 — 기존 `_data.Items`를 덮어쓰지 않음).

### 구버전 세이브 마이그레이션
- **D-04:** `SaveVersion`을 3으로 올린다. 별도의 `MigrateFromV2()` 같은 변환 함수는 만들지
  않는다 — `Items`는 지금까지 실제로 채워진 적이 없는 스텁(`List<string>`, 항상 빈 배열)이므로,
  기존 세이브 파일을 `List<ItemSaveEntry>`로 역직렬화해도 빈 배열은 타입 불일치 없이 그대로
  빈 리스트가 된다. `EnsureCollections()`에 `if (_data.Items == null) _data.Items = new
  List<ItemSaveEntry>();` 가드만 추가하면 충분하다.

### Claude's Discretion
- `Resources.Load` 시 파일명 기준 매칭 vs `Resources.LoadAll` + `Id` 필드 매칭 중 최종 선택
  (연구/계획 단계에서 확정, D-01 참고).
- `ItemSaveEntry` 클래스가 별도 파일인지 `SaveData.cs` 내 nested/동반 타입인지.
- `CaptureInventoryItems()`/복원 함수의 정확한 이름과 시그니처.
- 로드 시점에 `Inventory.Awake()`가 이미 20개 빈 슬롯으로 초기화를 마친 뒤 복원 로직을 실행하는
  정확한 훅 위치(기존 `ApplyPlayerStatsFromSave()`와 유사한 타이밍일 것으로 예상).

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 선행 구현 (변경 금지 원칙 — 최소한만 수정)
- `Assets/SaveSystem/Script/SaveData.cs` — `Items` 필드 위치, `SaveVersion` 필드, 기존 스텁 주석.
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — `Save()`/`LoadGame()`/`EnsureCollections()`/
  `CapturePlayerStats()`/`ApplyPlayerStatsFromSave()` 패턴. 새 캡처/복원 코드는 이 파일의 기존
  관행(예외 없는 `GetComponent`/`Instance` null 체크, `[ContextMenu("PhaseN/...")]` 디버그 훅)을
  그대로 따른다.
- `Assets/Player/Script/Inventory.cs` — Phase 18 산출물. `SlotCountTotal`, `GetSlot(int)`,
  `TryAddItem(ItemData, int)` 공개 API. 슬롯이 `List<InventorySlot>`(고정 20개)이며 `InventorySlot`
  이 `class`(struct 아님)라는 점, `GetSlot`이 범위 밖 인덱스에 null을 반환한다는 점을 유의.
- `Assets/Item/Script/ItemData.cs` — `Id` 프로퍼티(읽기 전용, `[SerializeField] private string id`).
  Phase 19가 이 `id`를 저장/복원 키로 쓸 것을 이미 전제하고 설계됨(파일 주석 참고).

### 선행 페이즈 결정 (참고)
- `.planning/phases/18-inventory-system-depends-on-phase-17/18-CONTEXT.md` — D-01(슬롯 20/스택99
  상수), D-02(class InventorySlot), D-06(가득 찬 인벤토리 데이터 유실 금지 원칙 — 이번 phase의
  캡처/복원 로직도 유실 없이 정확히 대칭이어야 함).
- `.planning/phases/18-inventory-system-depends-on-phase-17/18-RESEARCH.md` — struct/class,
  atomic TryAddItem, stack-merge 순서 등 Phase 18 패턴. 복원 로직이 이 패턴에 의존한다.

**로드맵 원문 외 별도 ADR/스펙 문서는 없음 — ROADMAP.md Phase 19 섹션과 이 CONTEXT.md가 스펙
역할을 겸한다.**

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SaveLoadManager.CapturePlayerStats()` / `ApplyPlayerStatsFromSave()` — 캡처/복원 함수 작성 시
  그대로 본뜰 참조 구현.
- `SaveLoadManager.EnsureCollections()` — 신규 컬렉션 null 가드를 추가할 위치.
- Phase 18의 `Inventory.GetSlot(int)`/`TryAddItem` — 캡처는 `GetSlot`으로 순회, 복원은
  `TryAddItem` 반복 호출로 구현 가능.

### 프로젝트 관행
- Newtonsoft.Json 기반 직렬화(`JsonConvert`) — `UnityEngine.JsonUtility`는 쓰지 않음(Dictionary
  직렬화 때문에 이미 Phase 11에서 결정됨). `ItemSaveEntry`도 이 직렬화 경로를 그대로 탄다.
- `[ContextMenu("PhaseN/설명")]` 디버그 훅 컨벤션 — `SaveLoadManager`는 이미 `Phase11/`,
  `Phase14/`, `Settings/` 접두사를 쓰고 있음. 이번 phase는 `Phase19/`를 새로 쓸 것으로 예상.
- 무-null가드 관행이되, `Instance`/`GetComponent`가 아예 없는 씬(메인메뉴 등)에서 호출될 수 있는
  지점(`CapturePlayerStats`)만은 예외적으로 null 체크 + 경고 로그를 남긴다 — Phase 19의
  `CaptureInventoryItems`도 이 예외 패턴을 따른다(D-03).

### Integration Points
- `SaveData.cs`의 `Items` 필드 타입 교체(`List<string>` → `List<ItemSaveEntry>`).
- `SaveLoadManager.Save()` 본문에 `CaptureInventoryItems()` 호출 추가.
- `SaveLoadManager.LoadGame()`의 씬 로드 완료 후 흐름(`LoadSceneAndRestoreRoutine` 또는
  `ApplyPlayerStatsFromSave` 인근)에 인벤토리 복원 호출 추가.
- `Assets/Item/*.asset` → `Assets/Resources/Items/*.asset` 이동(또는 복사) — 두 기존 에셋
  (`HealthPotion.asset`, `AncientKey.asset`) 모두 대상.

</code_context>

<specifics>
## Specific Ideas

- "Resources 폴더가 가장 단순하고 빌드에서도 동작한다" — 레지스트리 SO 대신 Resources를 고른
  핵심 이유.
- "슬롯 위치 보존은 지금 인벤토리 UI가 없으니 굳이 필요 없다" — 순차 재생성으로 충분하다는 판단.
- "Items가 지금까지 한 번도 채워진 적이 없어서 마이그레이션이 사실상 공짜" — 버전만 올리는
  선택의 근거.

</specifics>

<deferred>
## Deferred Ideas

- **슬롯 위치 정확 보존(`ItemSaveEntry.slotIndex`)** — 인벤토리 UI(드래그앤드롭) phase에서
  필요해지면 그때 스키마에 필드를 추가.
- **`ItemDatabase` 레지스트리 SO로 전환** — 아이템 수가 늘어나 `Resources.Load` 방식의 한계
  (로드 시간, 메모리)가 실제로 문제가 되면 재검토.
- **아이템 정렬/자동 정리** — Phase 18에서 이미 백로그로 넘어간 항목, 이번 phase에도 포함 안 함.

### Reviewed Todos (not folded)
None — 이번 세션에서 todo match-phase 19 결과 확인되지 않음(매칭 대상 없음).

</deferred>

---

*Phase: 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18*
*Context gathered: 2026-09-21*
