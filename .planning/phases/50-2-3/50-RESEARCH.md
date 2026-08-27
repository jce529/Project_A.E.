# Phase 50 (temp number, 50-2-3): 세이브 슬롯 확장 - Research

**Researched:** 2026-08-27
**Domain:** Unity C# save-system extension (single-slot → 3-slot), MainMenu UI flow
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** "이어하기" 버튼은 항상 슬롯 선택 화면으로 이동한다. 사용자가 슬롯을 직접 골라야
  로드가 시작된다 — 자동으로 아무 슬롯이나 이어서 로드하지 않는다.
- **D-02:** "새시작" 버튼은 빈 슬롯이 하나라도 있으면 슬롯 선택 화면을 거치지 않고 자동으로
  그 빈 슬롯을 골라 바로 새 게임을 시작한다.
- **D-03:** 3개 슬롯이 전부 데이터로 차 있으면 "새시작"도 슬롯 선택 화면으로 보내서, 사용자가
  덮어쓸 슬롯을 직접 고르게 한다.
- **D-04:** 슬롯 선택 화면에서 이미 데이터가 있는 슬롯을 "새로 시작"용으로 선택하면(D-03 경로,
  또는 슬롯 화면에서 직접 재시작을 고르는 모든 경로), 덮어쓰기 확인 다이얼로그가 반드시 뜬다
  ("이 슬롯을 덮어쓰고 새 게임을 시작하시겠습니까?"). 확인 없이 즉시 지우지 않는다.
- **D-05:** 확인 없는 즉시 덮어쓰기 경로는 만들지 않는다 — 진행 중인 세이브를 실수로 날리는
  사고를 막는 것이 목적.
- **D-06:** 슬롯마다 별도 JSON 파일로 저장한다 (배열을 담은 파일 1개 방식은 채택하지 않음).
  한 슬롯 저장이 다른 슬롯에 영향을 주지 않고, 파일 손상 시 그 슬롯 하나만 영향받는다.
- **D-07 (Claude's Discretion으로 넘어감, 이 문서 "파일 마이그레이션 전략" 절에서 해결):**
  정확한 파일명 규칙과 기존 `save.json` 마이그레이션 방식. 절대 기준: 기존 플레이어의
  `save.json`이 사라지거나 강제 리네임/삭제되면 안 된다.

### Claude's Discretion

- **슬롯 카드에 표시할 진행도 내용:** 기본 방향은 새 필드를 추가하지 말고 `SaveData`에
  이미 있는 값만 사용 — 씬 이름(`SceneName`)과 격파한 보스 수(`BossProgress.Count`, 예:
  "격파 보스 2/4"). 플레이타임/마지막 저장 시각 등 신규 필드는 필요성이 명확해지면 그때 판단.
- 정확한 슬롯 파일명 규칙 및 기존 `save.json` 마이그레이션 방식 (D-07).
- 슬롯 선택 화면의 정확한 레이아웃/비주얼 디자인 — 기능 요구사항(D-01~D-05)만 결정, UI
  디테일은 범위 밖.

### Deferred Ideas (OUT OF SCOPE)

- 슬롯 카드 진행도 표시에 플레이타임/마지막 저장 시각 등 `SaveData`에 없는 새 필드를 넣는 것 —
  이번 논의에서 요구되지 않음.
- 슬롯 선택 화면의 비주얼 디자인/레이아웃 세부사항.
- (Phase boundary, restated) Phase 11이 이미 구현한 저장 트리거 통합(체크포인트/보스 격파 시
  `Save()` 호출), 좌표 복원 경로(`PlayerSpawner`), 직렬화 방식(Newtonsoft.Json)은 이번
  페이즈에서 재설계하지 않는다.
</user_constraints>

<phase_requirements>
## Phase Requirements

No phase requirement IDs were mapped for this phase (`phase_req_ids` is null). This phase was
added ad hoc via roadmap evolution (see STATE.md "Roadmap Evolution" — Phase 50 entry) and
predates/is unrelated to `REQUIREMENTS.md` v2.0 (물의 정령 보스 구현, boss-fight scope only).
`REQUIREMENTS.md` should NOT be treated as in-scope for this phase; CONTEXT.md's Decisions
(D-01~D-07) are the sole functional spec.

| ID | Description | Research Support |
|----|-------------|-------------------|
| — | (none mapped) | CONTEXT.md D-01~D-07 serve as the requirement set; see "Locked Decisions" above and sections below for how each is satisfied. |
</phase_requirements>

## Summary

Phase 11 built a working single-slot save system: `SaveLoadManager` (DontDestroyOnLoad
singleton, self-bootstrapping via `RuntimeInitializeOnLoadMethod`) owns one in-memory `SaveData`
(`_data`) and reads/writes exactly one file, `save.json`, via `Save()` / `LoadGame()` /
`NewGame()` / `HasSaveFile()`. `SaveData` (POCO, Newtonsoft.Json-serialized) already contains
everything a slot needs: `SceneName`/`SpawnPointName`, `PlayerStatsSaveData`, `BossProgress`
(`Dictionary<string,bool>`), `MapGimmickState`, `Items`. A concurrent, now-substantially-complete
quick task (260827-h5y) added a second, fully independent file/model pair (`setting.json` /
`SettingsData`) to the same class — confirmed on disk, does not overlap with slot logic at all.

Turning this into 3 independent slots is a **small, additive change**, not a redesign:
add a `CurrentSlot` int field to `SaveLoadManager`, make the save-file path a function of slot
index instead of a hardcoded constant, and add a handful of slot-aware helper methods
(`HasSaveFile(int)`, `PeekSlotData(int)`, `SelectSlot(int)`) for the new slot-select screen.
Every existing call site (`Checkpoint.cs`, the four boss-death sites, `PauseMenu.cs`,
`GameSettingsPanel.cs`) keeps calling the same no-arg methods (`Save()`, `SaveAtCheckpoint()`,
`SaveOnBossDefeated()`) — those become implicitly "operate on whatever slot is currently
selected" and need zero changes. The only genuinely new code is (1) the slot-aware path/peek
logic in `SaveLoadManager`, (2) a new slot-select UI panel (no existing screen), and (3) rewiring
`MainMenuUI.OnClickStart()`/`OnClickLoad()` to the D-01~D-03 branching logic.

**Primary recommendation:** Keep `save.json` exactly as-is for slot 0 (no migration/rename), add
`save_1.json`/`save_2.json` for slots 1–2; add a `CurrentSlot` field + `GetSavePath(int)` helper
to `SaveLoadManager` rather than threading a slot parameter through every existing method; build
the slot-select screen as a same-scene toggled Panel (mirroring `PauseMenu`'s `SetActive()`
pattern), not a new scene.

## Architecture Patterns

### Recommended file layout (no new files needed for the manager itself — edit in place)

```
Assets/SaveSystem/Script/
├── SaveLoadManager.cs      # EDIT: add CurrentSlot, GetSavePath(int), HasSaveFile(int),
│                           #       PeekSlotData(int), SelectSlot(int), NewGameInSlot(int)
├── SaveData.cs             # NO CHANGE (schema already slot-shaped; each slot = one SaveData)
└── SettingsData.cs         # NO CHANGE (not slot-scoped, confirmed separate file/model)

Assets/Script/
├── MainMenuUI.cs           # EDIT: OnClickStart()/OnClickLoad() branch per D-01~D-03
├── SlotSelectPanel.cs      # NEW: slot-select screen controller (3 cards + confirm dialog)
```
(Exact folder for `SlotSelectPanel.cs` is a placement detail, not a functional one — `Assets/Script/`
is suggested because that's where `MainMenuUI.cs`, its direct caller, already lives.)

### File-per-slot naming (D-06 already locked this; this section fixes the exact scheme)

```csharp
// SaveLoadManager.cs
public const string SaveFileName = "save.json"; // UNCHANGED constant — stays slot 0's filename

private static string GetSlotFileName(int slot)
{
    return slot == 0 ? SaveFileName : "save_" + slot + ".json";
}

public string GetSavePath(int slot)
{
    return Path.Combine(Application.persistentDataPath, GetSlotFileName(slot));
}

// SavePath (currently `public static string SavePath`) becomes slot-aware.
// Verified: SavePath is referenced ONLY inside SaveLoadManager.cs itself
// (Save/LoadGame/HasSaveFile/DebugLogState) — no external caller in the codebase
// (grep confirmed zero hits for `SaveLoadManager.SavePath` / `.Instance.SavePath`
// anywhere under Assets/). Safe to change its shape.
public string SavePath { get { return GetSavePath(CurrentSlot); } }
```

**Why this scheme and not `save_0.json`/`save_1.json`/`save_2.json` uniformly:** slot 0 keeps
the exact filename `save.json` that already exists on every current player's disk. No file is
ever renamed, copied, or migrated — the "don't lose the existing save" constraint (D-07's
absolute floor) is satisfied structurally, not by writing migration code that could fail
mid-operation (interrupted `File.Move`, permission error, etc.). This was compared against the
CONTEXT.md-floated alternative (auto-migrate `save.json` → `save_0.json` on first run) and
rejected: that path adds a first-run migration routine, a "have we migrated already" flag, and a
failure mode (partial move) for zero functional benefit — slot 0 works identically either way.

### Slot dimension: "current active slot" field, not a parameter on every method

Two shapes were considered:

| Shape | Diff to existing call sites | Verdict |
|-------|------------------------------|---------|
| A: every method takes `int slot` (`Save(int slot)`, `LoadGame(int slot)`, ...) | `Checkpoint.cs`, 4 boss-death sites, `PauseMenu`/context-menu debug hooks all need edits to pass a slot they don't know/care about | Rejected — violates surgical-change principle (CLAUDE.md §3), touches files this phase explicitly must not touch |
| B: `CurrentSlot` instance field, set once by the slot-select screen before gameplay begins | Zero changes to `Checkpoint.cs`, boss-death sites, `PauseMenu.cs` — they keep calling `Save()`/`SaveAtCheckpoint()`/`SaveOnBossDefeated()` exactly as today | **Recommended** |

Gameplay-time save triggers never need to know "which slot" — by the time gameplay is running,
the player already picked a slot at the main menu. `CurrentSlot` is exactly analogous to how
`_data` already works today (implicit "the current save"): adding `CurrentSlot` extends that same
mental model to "the current save's slot index," rather than introducing a second axis of
parameterization everywhere.

```csharp
// SaveLoadManager.cs — new field, default 0 preserves current single-slot behavior
// for anything that runs before slot selection exists (e.g. Play-mode entering a
// gameplay scene directly, ContextMenu debug hooks).
public int CurrentSlot { get; private set; } = 0;

public void SelectSlot(int slot)
{
    CurrentSlot = slot;
}
```

`Save()`, `LoadGame()`, `NewGame()`, `HasSaveFile()` (no-arg) keep their exact existing bodies —
only the `SavePath` they read from changes shape (see above), because they already go through the
`SavePath` property/`SaveFileName` constant rather than duplicating the path expression.

### New slot-aware methods needed for the slot-select screen

```csharp
// Slot existence, for building the 3 cards + D-02/D-03 auto-pick logic.
public bool HasSaveFile(int slot)
{
    return File.Exists(GetSavePath(slot));
}

// Read-only peek: the slot-select screen must show progress for ALL 3 slots at once
// without disturbing the live _data/CurrentSlot the rest of the game relies on.
// Deliberately does NOT touch _data or CurrentSlot — this is the critical difference
// from LoadGame(), which is a mutating, scene-loading operation.
public SaveData PeekSlotData(int slot)
{
    string path = GetSavePath(slot);
    if (!File.Exists(path)) return null;
    try
    {
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
    }
    catch (System.Exception e)
    {
        Debug.LogError("[SaveLoadManager] PeekSlotData failed for slot " + slot + ": " + e.Message);
        return null;
    }
}

// D-02/D-03 "start new game in slot N" path: select the slot, then reuse the
// existing memory-only NewGame() body unchanged.
public void NewGameInSlot(int slot)
{
    SelectSlot(slot);
    NewGame();
}
```

`PeekSlotData` reuses the exact same `JsonConvert.DeserializeObject<SaveData>(json, JsonSettings)`
call already used by `LoadGame()` — no new serialization logic, just a read that doesn't assign
into `_data`. `JsonSettings` is already `private static readonly`, directly reusable.

### MainMenuUI flow rewrite (D-01 / D-02 / D-03)

```csharp
// Pseudocode — exact button/panel wiring is a UI-layout detail (Claude's Discretion),
// this is the decision logic the planner's tasks must implement.

public void OnClickLoad()   // "이어하기" — D-01: ALWAYS goes to slot-select, load intent
{
    slotSelectPanel.OpenForLoad();
}

public void OnClickStart()  // "새시작" — D-02/D-03: asymmetric vs OnClickLoad
{
    var mgr = SaveLoadManager.Instance;
    for (int slot = 0; slot < 3; slot++)
    {
        if (!mgr.HasSaveFile(slot))
        {
            // D-02: auto-pick first empty slot, no slot screen.
            mgr.NewGameInSlot(slot);
            SceneManager.LoadScene("Tutorial Map");
            return;
        }
    }
    // D-03: all 3 occupied -> slot-select screen, new-game intent (every pick needs D-04 confirm).
    slotSelectPanel.OpenForNewGame();
}
```

**Important existing-behavior gap this phase must fix (not scope creep — a direct precondition
of D-02/D-03):** `OnClickStart()` today calls `SceneManager.LoadScene("Tutorial Map")` directly
and never calls `NewGame()` (confirmed in current file — see Common Pitfalls). Without adding the
`NewGameInSlot()` call, starting fresh would silently reuse whatever `_data`/`CurrentSlot` state
happens to be left in memory from a prior `LoadGame()` in the same session, breaking D-02/D-03's
"new game" guarantee. CONTEXT.md's Existing Code Insights already flagged this explicitly.

**"이어하기" button interactable check:** currently `Start()` disables `loadGameButton` when
`!HasSaveFile()` (no-arg, i.e. slot 0 only). With 3 slots this should become "disabled only if
**all** 3 slots are empty" — CONTEXT.md doesn't decide this explicitly, but it's the direct
generalization of the existing single-slot check, not a new decision:
```csharp
bool anySave = mgr.HasSaveFile(0) || mgr.HasSaveFile(1) || mgr.HasSaveFile(2);
```

### Slot-select screen: same-scene toggled Panel, mirroring `PauseMenu`

No slot-select screen exists yet. The project's established UI pattern for auxiliary screens is
a `GameObject`/Canvas Panel toggled via `SetActive(true/false)`, not a scene transition:

```csharp
// Source: Assets/Player/Script/Menu/PauseMenu.cs (existing pattern, read in full)
private void Open()  { gameObject.SetActive(true);  /* ... */ }
private void Close()  { gameObject.SetActive(false); /* ... */ }
```

`GameSettingsPanel.cs` similarly reads current state into UI controls in `OnEnable()`
(`var s = SaveLoadManager.CurrentSettings; ...`) rather than `Start()`, because a re-toggled
panel's `Start()` only runs once. **A `SlotSelectPanel` should follow this exact convention:**
populate all 3 slot cards from `PeekSlotData(0/1/2)` in `OnEnable()`, so re-opening the panel
always reflects the latest saved state, not stale data cached at first activation.

There is **no existing confirm-dialog pattern** in this codebase to reuse (searched for
`Confirm`/`Dialog`/"덮어쓰기" — only hits are unrelated Editor tooling scripts). D-04/D-05 require
one new minimal component: a small panel with a message text, "확인"/"취소" buttons, and a
pending-slot-index field set right before `SetActive(true)`. Per CLAUDE.md §2 (simplicity first,
no unrequested flexibility), this should be a single-purpose "overwrite confirm" panel scoped to
this one use case — not a generalized reusable dialog/modal framework (nothing else in the
project needs one yet).

### Slot card progress content (Claude's Discretion, resolved)

Confirmed current `SaveData` fields available with zero new schema work:

```csharp
// SaveData.cs (current, unmodified)
public string SceneName;                          // e.g. "1 stage"
public Dictionary<string, bool> BossProgress;      // key=bossId, value=true (only added when defeated)
```

`BossProgress.Count` already equals the defeated-boss count directly — confirmed from
`SaveOnBossDefeated(bossId)`'s body: entries are only ever added with value `true` (there is no
code path that adds a `false` entry), so `Count` and "number of `true` values" are the same
number. `"격파 보스 " + _data.BossProgress.Count + "/4"` (4 = TutorialBoss/WoodBoss/WaterSpirit/
WaterMonster, per Phase 11's boss set) needs no dictionary filtering.

Empty-slot card state: `PeekSlotData(slot) == null` (equivalently `!HasSaveFile(slot)`) → render
as "빈 슬롯" / "새 게임" affordance instead of scene+progress text.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Per-slot JSON read/write | A new serialization path or a `List<SaveData>`/array-in-one-file scheme | The exact same `JsonConvert.SerializeObject/DeserializeObject(_, JsonSettings)` calls already in `Save()`/`LoadGame()`, just pointed at a slot-derived path | D-06 already rejected the array-in-one-file approach; Newtonsoft.Json + POCO already round-trips `SaveData` correctly, no reason to touch it |
| "Which slot is active" bookkeeping | A separate slot-tracking singleton/manager class | One `int CurrentSlot` field on the existing `SaveLoadManager` singleton | The manager is already the sole owner of save state; a second manager class for one int is unwarranted indirection (CLAUDE.md §2) |
| Overwrite confirmation | A generic reusable modal/dialog framework | One small purpose-built panel for this one confirm case | Nothing else in the project currently needs a generic dialog system; building one now is speculative flexibility CLAUDE.md explicitly warns against |
| Slot progress data | New persisted fields (playtime, last-saved timestamp) | Existing `SceneName` + `BossProgress.Count` | CONTEXT.md explicitly defers new fields; not needed to satisfy D-01~D-07 |

**Key insight:** Nothing in this phase requires new infrastructure. Every piece (per-slot file
I/O, "active slot" state, progress summarization) is a small extension of a pattern the codebase
already has (Phase 11's `SaveLoadManager`, or `PauseMenu`/`GameSettingsPanel`'s panel-toggle UI
convention). The temptation to build a general "SaveSlotManager" abstraction layer, an event bus
for slot changes, or a reusable dialog system should be resisted — none of it is asked for.

## Common Pitfalls

### Pitfall 1: `MainMenuUI.cs` and `PlayerSpawner.cs` are CP949-encoded, not UTF-8

**What goes wrong:** Reading these files through a standard UTF-8 text pipeline already shows
mojibake in their existing Korean comments/log strings (e.g. `MainMenuUI.cs` line 29:
`"���� ���� ��ư Ŭ����"`, `PlayerSpawner.cs`'s Korean comments are similarly garbled when viewed
as UTF-8). This confirms these two files are saved on disk as CP949 (Korean legacy encoding), not
UTF-8. STATE.md documents (Phase 11 Plan 3) that a standard Read→Edit UTF-8 round-trip on a
CP949 file **silently corrupts every non-ASCII byte in the entire file** to U+FFFD — not just the
lines being edited — and that a `grep -cP "[^\x00-\x7F]"` line-count gate does not catch this
because the byte count doesn't change, only the byte values do.

**Why it happens:** The file's actual on-disk bytes are CP949; a UTF-8-assuming
read/decode/re-encode/write cycle misinterprets and then mis-re-encodes every high-byte character.

**How to avoid:** `MainMenuUI.cs` is a primary edit target this phase (`OnClickStart`/
`OnClickLoad` rewrite). Follow the Phase 11 Plan 3 mitigation: extract the original bytes via
`git show HEAD:<path>` and perform pure byte-level insertion/edits rather than a naive
Read-tool-then-Edit-tool round trip, OR (simpler, since the new logic is pure C#/English control
flow) write only ASCII-safe new code and avoid touching/re-saving the file's existing Korean
comment lines at all where possible. Verify after editing with `git diff` that no unrelated lines
(especially the existing Korean log strings) changed byte-for-byte.

**Warning signs:** Any `git diff` on `MainMenuUI.cs` (or `PlayerSpawner.cs`, if it needs touching
— current expectation is it does not) showing changes on lines you didn't intend to edit, or
Korean text turning into `?`/`□`/mojibake in the diff.

### Pitfall 2: Confusing `PeekSlotData` with `LoadGame` semantics

**What goes wrong:** If the slot-select screen's card-population code accidentally calls
`LoadGame()` (or any method that assigns into `_data`) instead of a non-mutating peek, opening the
slot-select screen would corrupt/overwrite the in-memory `_data` for whatever slot happens to be
`CurrentSlot`, and `LoadGame()` additionally triggers an async scene load — wrong on a menu screen.

**Why it happens:** `LoadGame()` is the only existing "read a save file into memory" code path;
it's tempting to reuse it directly for "peek 3 slots' worth of data for display."

**How to avoid:** Use the dedicated `PeekSlotData(int slot)` (see Architecture Patterns) which
reads and deserializes without touching `_data`, `CurrentSlot`, or scenes. Verify: after opening
the slot-select screen, `SaveLoadManager.Instance.Data` (the existing read-only accessor) must be
unchanged from before the screen opened.

### Pitfall 3: `NewGame()`'s "memory-only" contract must be preserved when adding `NewGameInSlot`

**What goes wrong:** `NewGame()`'s existing contract (Phase 11 D-06, restated in its own comment)
is that it resets `_data` in memory only — it deliberately does NOT touch the file on disk (the
old file is overwritten lazily on the next `Save()`). If `NewGameInSlot(slot)` is implemented by
writing an empty `SaveData` to disk immediately (instead of just calling the existing `NewGame()`
after `SelectSlot(slot)`), the D-04/D-05 overwrite-confirm flow becomes pointless — the slot would
already be overwritten before the player reaches the checkpoint/first save trigger that normally
performs the write.

**Why it happens:** "New Game in slot N" sounds like it should "create slot N's file now," but
the existing single-slot design intentionally defers the disk write to the first real save event.

**How to avoid:** `NewGameInSlot(slot)` must be exactly `SelectSlot(slot); NewGame();` — no direct
`File.WriteAllText` call. The overwrite only actually happens later, at the next checkpoint/boss
death `Save()` call, exactly matching Phase 11's existing lazy-write behavior for slot 0 today.

### Pitfall 4: `HasSaveFile()` (no-arg) still exists and now means "does `CurrentSlot` have a save"

**What goes wrong:** After adding `HasSaveFile(int slot)`, the original no-arg `HasSaveFile()` is
still present (used by e.g. `LoadGame()`'s own guard) and its meaning silently shifts from "does
the single save file exist" to "does the currently-selected slot's file exist." If any new code
calls the no-arg version before a slot has been explicitly selected in the current session
(`CurrentSlot` defaults to 0), it will check slot 0, not "any slot" — which is correct for
`LoadGame()`'s internal guard but wrong if reused carelessly for UI logic that means "any slot
has data" (see the `anySave` fix in Architecture Patterns, which must use the 3-slot OR, not the
no-arg method).

**How to avoid:** Treat the no-arg `HasSaveFile()`/`Save()`/`LoadGame()`/`NewGame()` as "operate on
`CurrentSlot`" consistently; any new UI code that means "check all 3 slots" must explicitly loop/OR
over `HasSaveFile(0)`, `HasSaveFile(1)`, `HasSaveFile(2)` rather than calling the no-arg overload.

## Runtime State Inventory

This phase is a schema/manager extension, not a rename/rebrand/refactor of an existing
identifier — the Runtime State Inventory trigger (rename/refactor/migration phases) does not
strictly apply. It is included below only for the one narrow item that resembles a migration
concern (D-07), for completeness.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | Existing players' `Application.persistentDataPath/save.json` (Phase 11 single-slot format). Confirmed this phase's recommended scheme (slot 0 = `save.json`, unchanged filename) requires **no data migration at all** — the existing file becomes slot 0's file simply by virtue of `GetSavePath(0)` resolving to the same path it always has. | None — no code-driven migration, no format change. `SaveData.SaveVersion` field stays `1` (schema itself is unchanged; only which slot maps to which filename is new). |
| Live service config | None — this is a local single-player save file, no external/live service involved. | None |
| OS-registered state | None — plain file I/O under `Application.persistentDataPath`, no OS registration (no scheduled tasks, no registry keys, etc.). | None |
| Secrets/env vars | None — no secrets involved in save file naming or content. | None |
| Build artifacts | None — pure C# script changes, no build-time artifact (e.g. `.egg-info`-style stale metadata) applicable to a Unity script-only change. | None |

**Explicitly confirmed nothing found in every category except "Stored data"**, and even there the
recommended approach requires zero migration code — stated explicitly per the Runtime State
Inventory format's requirement to not leave categories blank.

## Code Examples

### Current `Save()`/`LoadGame()`/`HasSaveFile()` (unchanged bodies — only `SavePath`'s shape changes)

```csharp
// Source: Assets/SaveSystem/Script/SaveLoadManager.cs (current, read in full)
public void Save()
{
    CapturePlayerStats();
    string json = JsonConvert.SerializeObject(_data, JsonSettings);
    File.WriteAllText(SavePath, json);   // SavePath now resolves via GetSavePath(CurrentSlot)
    Debug.Log("[SaveLoadManager] Saved to " + SavePath);
}

public bool HasSaveFile()
{
    return File.Exists(SavePath);        // same: now slot-aware via SavePath
}
```

### Existing panel-toggle + `OnEnable()` refresh convention to mirror for `SlotSelectPanel`

```csharp
// Source: Assets/Player/Script/Menu/GameSettingsPanel.cs (current, read in full)
private void OnEnable()
{
    var s = SaveLoadManager.CurrentSettings;
    _langIndex = s.Language;
    RefreshLanguageText();
    if (screenShakeToggle  != null) screenShakeToggle.isOn  = s.ScreenShake;
    if (tutorialHintToggle != null) tutorialHintToggle.isOn = s.TutorialHint;
}
```
`SlotSelectPanel.OnEnable()` should follow the identical shape: read fresh state
(`PeekSlotData(0/1/2)`) and populate 3 card UIs, every time the panel is (re-)activated.

## Open Questions

1. **Should the "이어하기" button's per-slot rows show a disabled/greyed state for empty slots,
   or hide them entirely?**
   - What we know: D-01 says "이어하기" always opens the slot screen; nothing in CONTEXT.md
     specifies whether empty slots are selectable/visible there (only that "새시작" from an
     all-full state routes there too, per D-03, where empty slots can't exist).
   - What's unclear: the load-intent slot screen could have 0-3 populated slots depending on
     player progress; UI treatment of the empty ones during a *load* visit isn't decided.
   - Recommendation: disable (not hide) empty-slot cards in load-intent mode — consistent with
     the existing `loadGameButton.interactable` disabled-color pattern already used in
     `MainMenuUI.cs` for the single "이어하기" button today. This is a UI-layout detail
     (Claude's Discretion per CONTEXT.md), safe for the planner to decide without further research.

2. **Does the confirm dialog need to distinguish "overwrite via D-03 auto-route" vs. "overwrite
   via explicitly picking an occupied slot in load-intent-turned-new-game flows"?**
   - What we know: D-04 says the confirm dialog fires for "every path that picks an occupied slot
     for a new game," with no distinction by which button (새시작/이어하기) led there.
   - What's unclear: nothing — D-04's wording ("또는 슬롯 화면에서 직접 재시작을 고르는 모든
     경로") already covers this; there is no real ambiguity, listed here only to confirm the
     planner doesn't need a separate research pass.
   - Recommendation: single confirm-dialog code path triggered by "new-game action targeting a
     slot where `HasSaveFile(slot) == true`," regardless of entry point. No further research needed.

## Sources

### Primary (HIGH confidence — direct file reads of current on-disk state, this session)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\SaveSystem\Script\SaveLoadManager.cs` — full read, current state (post quick-task 260827-h5y settings additions, confirmed against STATE.md)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\SaveSystem\Script\SaveData.cs` — full read
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\SaveSystem\Script\SettingsData.cs` — full read
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\Script\MainMenuUI.cs` — full read (confirmed CP949 mojibake, confirmed `OnClickStart()` doesn't call `NewGame()`)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\map\script\PlayerSpawner.cs` — full read (confirmed unrelated to slot logic, confirmed CP949)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\Player\Script\Menu\PauseMenu.cs` — full read (panel-toggle UI pattern source)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\Player\Script\Menu\GameSettingsPanel.cs` — full read (`OnEnable()` refresh pattern source)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\Assets\Player\Script\GameStateManager.cs` — full read (confirmed not relevant — main menu doesn't reference it)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\.planning\phases\50-2-3\50-CONTEXT.md` — full read
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\.planning\STATE.md` — full read (Phase 11 decisions, Phase 11 Plan 3 CP949 pitfall, quick task 260827-h5y status)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\.planning\REQUIREMENTS.md` — full read (confirmed v2.0 scope is boss-fight only, not this phase)
- `C:\Users\MSI\Projeect_A.E\Projeect_A.E\.planning\config.json` — confirmed `nyquist_validation: false` (Validation Architecture section omitted per skip rule)
- Grep across `Assets/` — confirmed zero external callers of `SaveLoadManager.SavePath`/`.Instance.SavePath`; confirmed no existing `Confirm`/`Dialog` UI pattern in gameplay code (only unrelated Editor tooling)

No Context7/WebSearch queries were needed — this phase extends an existing in-repo, project-specific
pattern (no external library research required; Newtonsoft.Json/Unity UI/TMPro are already
integrated dependencies from Phase 11 and are explicitly out of scope to change).

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new packages; extending Newtonsoft.Json + Unity UI/TMPro already verified working in Phase 11.
- Architecture: HIGH — every recommendation is grounded in direct reads of the current, on-disk source files, not assumption.
- Pitfalls: HIGH — CP949 encoding risk is documented precedent from this exact codebase (Phase 11 Plan 3), not speculative; `OnClickStart()` NewGame() gap and PeekSlotData/LoadGame conflation are derived directly from reading the current method bodies.

**Research date:** 2026-08-27
**Valid until:** Until `SaveLoadManager.cs`/`SaveData.cs`/`MainMenuUI.cs` are next modified by another concurrent task (this project has a history of overlapping quick tasks touching `SaveLoadManager.cs` — re-verify current file state immediately before planning if time has passed).
