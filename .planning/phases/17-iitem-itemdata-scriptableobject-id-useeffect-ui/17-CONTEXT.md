# Phase 17: 아이템 코어 (IItem + ItemData ScriptableObject) - Context

**Gathered:** 2026-09-19
**Status:** Ready for planning

<domain>
## Phase Boundary

`IItem` 인터페이스는 이미 구현되어 있다 (`Assets/Item/Script/IItem.cs`, `void UseEffect(PlayerInteraction player)`).
이번 phase는 `ItemData` ScriptableObject를 정의해 아이템의 정적 데이터(id, 종류, UseEffect 파라미터)를
스키마화하고, `IItem`을 실제로 구현해 소모품 1종(체력 회복)이 end-to-end로 동작하는 것까지 만든다.

**범위 밖**: 인벤토리 자료구조/슬롯/스택(Phase 18), 월드 아이템 획득 및 `PlayerInteraction` 연동 배선(Phase 18),
세이브/로드 연동(Phase 19), 아이템 아이콘/이름 등 UI 표시 자체(추후 UI phase).

</domain>

<decisions>
## Implementation Decisions

### IItem 구현 위치
- **D-01:** `ItemData : ScriptableObject, IItem` — `ItemData`가 `IItem`을 직접 구현한다.
  데이터와 사용 로직이 한 파일에 공존하며, 별도의 런타임 아이템 인스턴스 클래스는 만들지 않는다.
  프로젝트에 ScriptableObject 선례가 전혀 없으므로 이 phase가 그 패턴을 확립한다.

### UseEffect 동작 범위
- **D-02:** `UseEffect()`는 스텁이 아니라 실제로 끝까지 구현한다. 종류(소모품/진행아이템)에 따라
  분기하며, **소모품만** `effectType`을 실제로 적용하고, **진행아이템은 빈 구현(no-op)**으로 둔다.
  진행아이템의 실제 사용 의미(퀘스트 해결 등)는 인벤토리/콘텐츠 단계(Phase 18+)에서 처리한다.
- **D-03:** 소모품 `effectType` enum은 이번 phase에서 **`Heal` 하나만** 정의한다. `amount`(float)
  파라미터와 함께 `PlayerInteraction` → `GetComponent<PlayerStats>()` → 기존 `PlayerStats.Heal(float)`
  (`Assets/Player/Script/PlayerStats.cs:37`)를 호출해 실제 회복이 일어나야 한다. 신규 `PlayerStats`
  메서드를 추가하지 않고 기존 `Heal()`을 그대로 재사용한다. 향후 효과가 늘어나면 enum 값만 추가한다.

### 아이템 종류(타입)
- **D-04:** 종류는 `ItemType` enum 2가지 — `Consumable`(소모품) / `Progression`(진행아이템).
  타입별 서브클래스(SO 상속 구조)는 만들지 않고, 단일 `ItemData` 클래스 + enum 분기로 처리한다.

### 아이템 ID 스킴
- **D-05:** `id`는 `[SerializeField] string id` — 수동 입력. 자동 생성(에셋 파일명 기반 등) 스크립트는
  만들지 않는다. 중복/공백 검증 도구도 이번 phase 범위 밖(수동 관리). Phase 18의 `SaveData.Items`가
  `List<ItemSaveEntry>{itemId: string, count}`로 교체될 예정이므로 타입은 string으로 고정.

### 스키마 범위 (필드 목록)
- **D-06:** 이번 phase는 **최소 필드만** 정의한다 — `id`(string), `type`(ItemType enum),
  `effectType`(enum, Consumable 전용 파라미터), `amount`(float, Consumable 전용 파라미터).
  `displayName`/`icon`/`description` 등 표시용 메타데이터는 **포함하지 않는다** — UI가 없는데
  아이콘/이름을 둘 이유가 없다는 원칙. 필요해지면 Phase 17에서 필드를 추가한다.

### 예시 에셋
- **D-07:** 클래스 정의뿐 아니라 실제 `.asset` 파일 1~2개를 `Assets/Item`에 생성해 스키마가
  실제로 동작함을 눈으로 확인한다 — 소모품(Heal) 1개 + 진행아이템 1개. 이 에셋은 Phase 18
  인벤토리 개발 시 테스트 데이터로 재사용 가능하다.

### Claude's Discretion
- `ItemType`/effectType enum의 정확한 네이밍과 파일 배치(같은 파일 내 nested enum vs 별도 파일).
- `CreateAssetMenu` 속성의 menuName/fileName 문자열.
- `PlayerInteraction`에서 `PlayerStats`를 얻는 구체적 방법(`GetComponent` 캐싱 여부) — 단, null
  가드 없이 호출하는 기존 프로젝트 관행(Phase 12/15 CONTEXT 참고)을 따를지는 연구/계획 단계에서 확인.
- 예시 에셋 2개의 구체적인 이름/수치(id 문자열, amount 값).

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 선행 구현 (변경 금지 — 그대로 구현해야 하는 계약)
- `Assets/Item/Script/IItem.cs` — 이미 완료된 인터페이스. `void UseEffect(PlayerInteraction player)`
  시그니처를 그대로 구현해야 한다.
- `Assets/Player/Script/PlayerInteraction.cs` — `UseEffect`의 파라미터 타입. `IItem` 구현체가
  플레이어 관련 컴포넌트(`PlayerStats` 등)를 얻으려면 이 클래스의 `GetComponent<T>()`를 통해야 한다
  (이 클래스는 `MonoBehaviour`이므로 같은 GameObject에서 `PlayerStats`를 찾을 수 있다 — Player.prefab
  구조 확인은 계획 단계에서).

### 재사용 대상 (수정 금지)
- `Assets/Player/Script/PlayerStats.cs:37` `public override void Heal(float amount)` — Heal
  effectType이 호출할 기존 메서드. 신규 Heal 로직을 만들지 않는다.

### 선행 페이즈 결정 (참고 — 향후 연동 예정)
- `.planning/phases/15-load-timing-and-load-scope/15-CONTEXT.md` — D-10: `SaveData.Items`는
  현재 스텁이며 이번 phase가 그 스텁을 채우는 것은 아니다(Phase 19 담당). 다만 id를 string으로
  고정한 배경.
- `Assets/SaveSystem/Script/SaveData.cs:33` `public List<string> Items` — Phase 18이 교체할
  스텁 필드. 이번 phase는 건드리지 않는다.

**로드맵 원문 외 별도 ADR/스펙 문서는 없음 — ROADMAP.md Phase 17 섹션과 이 CONTEXT.md가 스펙 역할을 겸한다.**

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `PlayerStats.Heal(float amount)` (`Assets/Player/Script/PlayerStats.cs:37`) — Consumable Heal
  effectType이 그대로 호출할 기존 메서드.
- `PlayerInteraction` (`Assets/Player/Script/PlayerInteraction.cs`) — `IItem.UseEffect`의 파라미터.
  아이템 사용 시 이 컴포넌트를 통해 플레이어의 다른 컴포넌트에 접근한다.

### 프로젝트 최초 ScriptableObject
- 프로젝트 전체에 `ScriptableObject` 사용 사례가 **전혀 없다** (grep 결과 0건). `ItemData`가 이
  프로젝트의 첫 SO이므로 참고할 기존 패턴이 없다 — 표준 Unity SO 관례(`CreateAssetMenu`,
  `[SerializeField]` private 필드 + public 읽기 전용 프로퍼티)를 따르면 된다.

### Integration Points
- `Assets/Item/Script/` — `ItemData.cs` 신규 위치 (기존 `IItem.cs`와 같은 폴더).
- `Assets/Item/` — 예시 `.asset` 2개가 생성될 위치 (D-07).
- Phase 17이 인벤토리에서 `ItemData` 참조 배열/딕셔너리를 만들고 `UseEffect(playerInteraction)`을
  호출하는 "사용 API"를 구현할 예정 — 이번 phase의 `ItemData` public 인터페이스가 그 API의 기반이 된다.

</code_context>

<specifics>
## Specific Ideas

- "ItemData가 IItem을 직접 구현" — 별도 런타임 클래스 분리보다 단일 파일 구조를 명시적으로 선택함.
- "UseEffect는 최소 구현이 아니라 끝까지" — 소모품 Heal 하나만큼은 실제로 `PlayerStats.Heal()`을
  호출해 눈으로 검증 가능해야 한다는 게 사용자 의도.
- "표시용 메타데이터는 이번 phase 범위 밖" — icon/displayName은 UI가 생기기 전까지 미룬다.

</specifics>

<deferred>
## Deferred Ideas

- **표시용 메타데이터(displayName/icon/description)** — Phase 18(인벤토리) 또는 이후 UI phase에서
  `ItemData`에 필드 추가 필요.
- **아이템 ID 중복/형식 검증 도구** — 수동 관리로 시작, 아이템 수가 늘어나면 Editor 검증 스크립트
  필요할 수 있음.
- **소모품 effectType 확장**(회복 외 버프/디버프/화폐 등) — 필요해지는 시점에 enum 값만 추가.

### Reviewed Todos (not folded)
None — `todo match-phase 16` 결과 매칭되는 todo 없음.

### 참고 (이번 phase와 무관한 발견)
- Phase 15 CONTEXT.md에 "일정 간격 자동저장을 Phase 16으로 분리하기로 결정"이라는 기록이 있으나,
  그 이후 Phase 17 번호가 재번호되며 실제로는 이 아이템 코어 phase가 배정되었다. 자동저장 아이디어는
  현재 로드맵 어디에도 정식 phase로 남아있지 않다 — 별도 백로그 항목으로 필요시 재등록 검토 필요.
  (이번 discuss-phase 범위 밖이라 여기서는 기록만 남김.)

</deferred>

---

*Phase: 16-iitem-itemdata-scriptableobject-id-useeffect-ui*
*Context gathered: 2026-09-19*
