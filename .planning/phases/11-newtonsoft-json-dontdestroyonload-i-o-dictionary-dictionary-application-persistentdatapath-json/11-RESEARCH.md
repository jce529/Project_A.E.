# Phase 11: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 - Research

**Researched:** 2026-08-09
**Domain:** Unity 6 (6000.3.10f1) C# — singleton manager, Newtonsoft.Json file I/O, async scene loading
**Confidence:** HIGH (all findings verified against actual project source files; only the async-scene-load timing nuance and Newtonsoft settings choice are MEDIUM, cross-checked with web sources)

## Summary

This phase adds a new `SaveLoadManager` singleton that persists a single `save.json` file under `Application.persistentDataPath` using Newtonsoft.Json (already resolved transitively via `com.unity.nuget.newtonsoft-json@3.2.2` in `Library/PackageCache`, not declared directly in `Packages/manifest.json`). The project has **zero `.asmdef` files** anywhere under `Assets/`, and the Newtonsoft package itself ships as a plain DLL with no `.asmdef` — so `using Newtonsoft.Json;` will compile from any script in the default `Assembly-CSharp` assembly with no assembly-reference wiring needed. The main technical risk is not compile-time availability, it's transitive-dependency fragility (see Pitfall 1) and reconciling this phase's stated data model ("씬+좌표") with the *only* working coordinate-restore mechanism in the codebase, which is name-based (`PlayerSpawner.targetSpawnPointName` → object lookup by `GameObject.name`), not raw XY floats.

Critically, the codebase has **two incompatible boss-death architectures**. `TutorialBossController` and `WoodBossController` use a shared `HP` component (`Assets/Script/HP.cs`) that already exposes `public event Action OnDeath`, and both already subscribe to it (`_hp.OnDeath += HandleDeath`). But `SpiritController` (WaterSpirit) and `WaterMonsterController` use `BossStatsSystem` (`Assets/Enemy/NewBoss/Script/BossStatesSystem.cs`), whose `Die()` is a `protected virtual` method with **no public event at all** — `SpiritStats.Die()` and `WaterMonsterStats.Die()` override it and just call `gameObject.SetActive(false)` / cleanup, with nothing to hook. CONTEXT.md's D-01 assumption ("각 보스의 OnDeath 이벤트 핸들러에서 SaveLoadManager.Instance.Save() 직접 호출") only holds as-is for 2 of the 4 bosses; the other 2 need a one-line addition inside their existing `Die()` overrides.

**Primary recommendation:** Build `SaveLoadManager` as a `MonoBehaviour` singleton mirroring `GameManager.cs`'s exact `Awake()` pattern, store save data as `sceneName + spawnPointName` (string) rather than raw coordinates so it can reuse `PlayerSpawner.targetSpawnPointName` → `ApplySpawn()` unmodified, drive scene transitions with a coroutine (`IEnumerator`, not `async`/`await` — the whole codebase exclusively uses `StartCoroutine`/`IEnumerator`), and add the missing `SaveLoadManager.Instance.Save()` call directly inside `SpiritStats.Die()` / `WaterMonsterStats.Die()` (not via a nonexistent event) plus inside `TutorialBossController.HandleDeath()` / `WoodBossController.HandleDeath()` (which do have the event).

## User Constraints (from CONTEXT.md)

<user_constraints>
### Locked Decisions

- **D-01:** `SaveLoadManager`는 저장/로드 핵심 로직만 소유한다. 별도의 이벤트 버스/브로드캐스트 레이어를 신설하지 않고, `Checkpoint.cs`의 체크포인트 활성화 로직과 각 보스의 `OnDeath` 이벤트 핸들러(또는 그에 준하는 사망 처리 지점)에서 `SaveLoadManager.Instance.Save()`를 직접 호출하는 방식으로 최소 변경 통합한다.
- **D-02:** 단일 세이브 슬롯만 지원한다 (`Application.persistentDataPath`에 파일 1개, 예: `save.json`). 다중 슬롯 UI/구조는 범위 밖.
- **D-03:** 보스 진행도 Dictionary와 맵 기믹 상태 Dictionary는 이번 페이즈에서 최소 스텁만 구현한다 (예: `Dictionary<string, bool>`). 실제로 채워 넣는 항목 수는 최소화. 스키마 확장은 후속 페이즈 몫.
- **D-03b:** 아이템 목록도 동일하게 빈 리스트/스텁으로 구현한다 — 프로젝트에 아이템/인벤토리 시스템 자체가 아직 없음.
- **D-03c:** 플레이어 스탯 하위클래스에 담을 정확한 필드 구성은 Claude 재량. `PlayerStats : HP`(`Assets/Player/Script/PlayerStats.cs`)에 실제 존재하는 `health`/`maxHealth`/`maxTotalHealth` 필드가 출발점.
- **D-04:** 이번 페이즈는 `SaveLoadManager`의 공개 API(예: `SaveGame()`, `LoadGame()`, `HasSaveFile()`, `NewGame()`)만 구현한다. `MainMenuUI.cs`에 "이어하기" 버튼 추가/연결, 세이브 파일 유무에 따른 UI 분기는 범위 밖. 비동기 씬 로드 중 별도 로딩 화면 UI도 만들지 않는다.
- **D-05:** 로드 후 좌표 복원은 기존에 실제로 동작 중인 `PlayerSpawner.targetSpawnPointName`(static) → `PlayerSpawner.ApplySpawn()` 경로를 재사용한다. `Portal.cs` → `GameManager.Instance.NextSpawnPointName` 경로는 아무 데서도 읽히지 않는 기존 고아 코드이며, 이번 페이즈에서 되살리거나 수정하지 않는다 (언급만, 삭제/수정 금지).
- **D-06:** 단일 슬롯 구조에서 "새 게임" 시작 시 기존 세이브 파일을 즉시 덮어쓰지 않는다. 메모리 상 데이터만 기본값으로 리셋하고, 실제 파일 덮어쓰기는 다음 `Save()` 트리거 시점에만 발생한다.

### Claude's Discretion

- 정확한 `PlayerStats` 저장 필드 구성 (D-03c).
- 비동기 씬 로드 중 로딩 화면 UI 유무 — UI가 범위 밖(D-04)이므로 로딩 화면 없이 코루틴/async 흐름만 구현하는 것이 기본 방향.
- 보스 진행도/맵 기믹 Dictionary의 정확한 키 네이밍 규칙(보스 ID, 기믹 ID 문자열 등).
- Newtonsoft.Json 직렬화 세부 설정(들여쓰기, null 처리, 타입 핸들링 등).

### Deferred Ideas (OUT OF SCOPE)

- 메인 메뉴 "이어하기" 버튼 UI 연동, 세이브 파일 존재 여부에 따른 버튼 활성화/비활성화 — 후속 페이즈.
- 다중 세이브 슬롯 지원 — 필요 시 후속 페이즈.
- `GameManager.NextSpawnPointName` 고아 코드 정리 — 이번 페이즈 범위 아님, 언급만 (건드리지 않음).
</user_constraints>

## Phase Requirements

No requirement IDs mapped in `.planning/REQUIREMENTS.md` (that file tracks the WaterSpirit boss milestone, unrelated to this phase). `11-CONTEXT.md` decisions D-01 through D-06 are the authoritative spec for this phase.

## Project Constraints (from CLAUDE.md)

- **Plan before code:** confirm a `.planning/phases/11-.../11-PLAN.md` exists before writing implementation. This RESEARCH.md feeds that plan.
- **Minimal footprint:** only touch files directly required by D-01–D-06. Do not refactor `GameManager.cs`, `Portal.cs`, or `WoodBossStatsSystem.cs` — they are explicitly out of scope / orphaned.
- **Surgical changes:** every changed line must trace to a plan task. Existing dead code (`Portal.cs`, `WoodBossStatsSystem.cs`) must be mentioned, never deleted, in this phase.
- **Verifiable goals:** each integration point (`Checkpoint.cs` save call, each boss death save call, `LoadGame()` coordinate restore) needs an explicit verification step, not just "should work."
- **YOLO mode awareness:** `.planning/config.json` currently has no `"mode"` key at the top level (only `workflow.nyquist_validation: false` and `_auto_chain_active: false`) — treat as standard confirmation flow unless the executor's config says otherwise.

## Standard Stack

### Core
| Library | Version (verified) | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `com.unity.nuget.newtonsoft-json` | **3.2.2** (resolved in `Packages/packages-lock.json`, cached at `Library/PackageCache/com.unity.nuget.newtonsoft-json@4dfd81071c64`) | JSON serialize/deserialize of the save-data object graph, including `Dictionary<string,T>` | Only JSON library in the project ecosystem that natively serializes `Dictionary<TKey,TValue>`; Unity's built-in `JsonUtility` cannot serialize dictionaries or top-level collections at all |
| `Newtonsoft.Json.dll` (bundled inside the package, `Runtime/Newtonsoft.Json.dll`) | ships as of 3.2.2 | The actual managed assembly `using Newtonsoft.Json;` resolves to | Package has **no `.asmdef`** — the DLL is a plain plugin, globally visible to `Assembly-CSharp` |

**Note on CONTEXT.md's stated version:** CONTEXT.md's code_context says "버전 3.2.1". The actual resolved/cached version on disk is **3.2.2** (both `packages-lock.json`'s `com.unity.nuget.newtonsoft-json` entry and the package's own `package.json` say 3.2.2; the 3.2.1 string in `packages-lock.json` is a *different* package, `com.unity.nuget.newtonsoft-json.tests`, unrelated). Use 3.2.2 as the actual current version when documenting/planning.

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `System.IO` (BCL, no package) | n/a | `File.WriteAllText` / `File.ReadAllText` / `File.Exists` against `Application.persistentDataPath` | Standard Unity save-file I/O; no third-party file library needed |
| `UnityEngine.SceneManagement` | bundled with Unity 6000.3.10f1 | `SceneManager.LoadSceneAsync`, `SceneManager.GetActiveScene()` | Required for the phase's mandated async scene load |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Newtonsoft.Json | Unity `JsonUtility` | Rejected by roadmap itself — `JsonUtility` cannot serialize `Dictionary<string,T>` (boss progress / map gimmick fields), which this phase explicitly requires |
| `File.WriteAllText` | `System.IO.StreamWriter` / async file APIs | No async file API exists in old .NET Standard 2.0 profile Unity targets by default reliably; sync `File.WriteAllText` is simpler and matches "저장은 체크포인트/보스 격파 시점에만" (infrequent, not perf-critical) |
| Coroutine-based async scene load | `async`/`await Task` + `SceneManager.LoadSceneAsync` awaiter extension | Rejected — **zero** precedent for `async`/`Task` anywhere in the codebase (verified via grep across `Assets/`); every existing async-ish operation uses `IEnumerator`/`StartCoroutine`. Introducing `async void` here would be the only occurrence in the project and breaks "정밀한 변경" / matching existing conventions |

**Installation:**

Newtonsoft.Json is already available; no `npm`-equivalent install step is required. If the planner decides to make the dependency explicit and safe (see Pitfall 1), the only action is adding one line to `Packages/manifest.json`:

```json
"com.unity.nuget.newtonsoft-json": "3.2.2"
```

**Version verification performed:** Checked `Packages/packages-lock.json` (resolved version `3.2.2`, depth 1, registry source) and `Library/PackageCache/com.unity.nuget.newtonsoft-json@4dfd81071c64/package.json` (`"version": "3.2.2"`) directly on disk — both agree. This is the authoritative current-state check for a Unity project (there is no npm registry involved).

## Architecture Patterns

### Recommended Project Structure

Follow existing project conventions — flat `Script`/`script` folders per feature area, no new top-level folder needed:

```
Assets/
├── SaveSystem/                      # NEW folder (or reuse Assets/Script/ — see note)
│   └── Script/
│       ├── SaveLoadManager.cs       # singleton, public API, file I/O, scene-load coroutine
│       └── SaveData.cs              # POCO data classes (SaveData, PlayerStatsSaveData, etc.)
```

Note: the project does not have a single consistent top-level convention (`Assets/map/script/`, `Assets/Player/Script/`, `Assets/Script/` all coexist with inconsistent casing). Either `Assets/Script/` (already holds `GameStateManager`-adjacent global scripts like `MainMenuUI.cs`, `HP.cs`, `AudioManager.cs`) or a new `Assets/SaveSystem/Script/` folder are both consistent with precedent. Planner's discretion; **do not** invent a deeper nested structure than the rest of the project uses.

### Pattern 1: DontDestroyOnLoad Singleton (exact project convention)

**What:** Every existing singleton in this project (`GameManager`, `GameStateManager`, `InputHandler`) uses the identical `Awake()` shape. `SaveLoadManager` must match it exactly — this is D-01's "minimal integration" spirit applied to the manager itself.

**Verified source (`Assets/map/script/GameManager.cs`, lines 10-22):**
```csharp
private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

`GameStateManager.cs` (`Assets/Player/Script/GameStateManager.cs`, lines 28-41) and `InputHandler.cs` (`Assets/Player/Script/InputHandler.cs`, lines 64-73) use the byte-for-byte same shape. `SaveLoadManager` should copy this pattern with `public static SaveLoadManager Instance;` (matching `GameManager`'s field style) or `{ get; private set; }` (matching `GameStateManager`'s style) — either is established precedent; prefer matching `GameStateManager`'s `{ get; private set; }` since it's the safer/more recent pattern and `SaveLoadManager` is conceptually closer to `GameStateManager` (project-lifetime service) than `GameManager`.

### Pattern 2: Coordinate Restore via Named Spawn Point (NOT raw XY floats)

**What:** The only working scene-transition + coordinate-restore path in the codebase is name-based lookup, not literal position serialization.

**Verified source (`Assets/map/script/PlayerSpawner.cs`):**
```csharp
public static string targetSpawnPointName = "";

void Start() { ApplySpawn(); }

public void ApplySpawn()
{
    if (!string.IsNullOrEmpty(targetSpawnPointName))
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        GameObject targetPoint = null;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == targetSpawnPointName) { targetPoint = obj; break; }
        }
        if (targetPoint != null)
        {
            transform.position = targetPoint.transform.position;
            PlayerRespawn respawn = GetComponent<PlayerRespawn>();
            if (respawn != null) respawn.SyncStartPosition(targetPoint.transform);
            targetSpawnPointName = ""; // consumed, self-clearing
        }
    }
}
```

And the real, currently-used caller (`Assets/map/script/SignpostPortal.cs`, lines 14-19):
```csharp
if (playerInRange && Input.GetKeyDown(KeyCode.W))
{
    PlayerSpawner.targetSpawnPointName = spawnPointName;
    SceneManager.LoadScene(nextSceneName);
}
```

**When to use:** This is the pattern `SaveLoadManager.LoadGame()` must mirror. **Implication for the save-data schema:** the "씬+좌표" field the roadmap calls for should be modeled as `string sceneName` + `string spawnPointName` (a GameObject name findable in the target scene), **not** `float x, y`. Rationale:
1. It's the only respawn mechanism that actually works end-to-end today (`Portal.cs`'s coordinate-adjacent field, `GameManager.NextSpawnPointName`, is dead/orphaned — D-05 explicitly forbids touching it).
2. `Checkpoint.cs` objects are named GameObjects already placed in scenes — a checkpoint's own `gameObject.name` is naturally usable as `spawnPointName` with zero new scene setup, since `Checkpoint.OnTriggerEnter2D`/S-key activation happens on the checkpoint object itself.
3. Storing raw floats would require **inventing a second respawn code path** that bypasses `PlayerSpawner`/`PlayerRespawn` entirely — larger surface area, contradicts "surgical changes" and D-05.

**Timing caveat (async load):** `PlayerSpawner` is attached to the `Player` prefab (confirmed via `Assets/Player.prefab` and its placement in `Assets/Scenes/1 stage.unity`), and the Player object is **not** `DontDestroyOnLoad` — a fresh `Player` instance is created by each scene, and its own `Start()` calls `ApplySpawn()` automatically. This means: as long as `SaveLoadManager.LoadGame()` sets `PlayerSpawner.targetSpawnPointName` **before** starting the scene load (exactly mirroring `SignpostPortal.cs`), the freshly-instantiated Player in the new scene will self-apply its position via its own `Start()` — **no explicit post-load "move player" call is needed** for position. This significantly simplifies the async-load coroutine: it only needs to (a) set the static field, (b) `yield return SceneManager.LoadSceneAsync(sceneName)`, and (c) after `isDone`, separately restore `PlayerStats` fields (see Pattern 4) since those are *not* self-restoring the way `PlayerSpawner` is.

### Pattern 3: Boss Death Hook — Two Different Architectures (must handle both)

**What:** D-01 assumes a uniform "보스의 OnDeath 이벤트" exists to subscribe to. In reality there are two incompatible boss stat hierarchies in this codebase:

**Group A — has a working `OnDeath` event already wired up (TutorialBoss, WoodBoss):**

Both use `HP` (`Assets/Script/HP.cs`), which has:
```csharp
public event Action OnDeath;   // HP.cs line 26
public bool ManualDeath = false;
public virtual void Die() { OnDeath?.Invoke(); if (ManualDeath) return; Destroy(gameObject); }
```

And both controllers already subscribe:
```csharp
// TutorialBossController.cs line 150, WoodBossController.cs line 42
_hp.OnDeath += HandleDeath;
```

For these two, the plan should add `SaveLoadManager.Instance.Save();` **inside the existing `HandleDeath()` method body** (`TutorialBossController.cs` line 299, `WoodBossController.cs` line 48) — a true one-line addition, exactly matching D-01's stated integration style.

**Group B — no event exists at all (WaterSpirit, WaterMonster):**

`BossStatsSystem` (`Assets/Enemy/NewBoss/Script/BossStatesSystem.cs`, the shared base class for `SpiritStats` and `WaterMonsterStats`) declares only:
```csharp
protected virtual void Die() { /* 사망 처리 */ }   // line 105, EMPTY body, no event
```

`SpiritStats.Die()` (override) and `WaterMonsterStats.Die()` (override) **do not call `base.Die()`** and have no event to fire:
```csharp
// SpiritStats.cs lines 57-69
protected override void Die()
{
    Debug.Log("[SpiritStats] 사망 처리!");
    var spiritController = GetComponent<SpiritController>();
    if (spiritController != null) spiritController.CleanupClones();
    gameObject.SetActive(false);
}

// WaterMonsterStats.cs lines 74-77
protected override void Die()
{
    gameObject.SetActive(false);
}
```

Neither `SpiritController` nor `WaterMonsterController` (the `BossController` subclasses) contain any death-handling code at all (`grep -i "dead|die|death"` returns zero matches in both files) — `BossController`'s base `Start()` only wires `Stats.OnWaterDepleted` and `Stats.OnDamageTaken`, never anything death-related (`BossController.cs` lines 52-56).

**For these two, the plan cannot "subscribe to an OnDeath event" — there isn't one.** The minimal-diff option consistent with D-01's spirit is to add `SaveLoadManager.Instance.Save();` as a **direct call inside the existing `Die()` override body** of `SpiritStats.cs` and `WaterMonsterStats.cs` (2 files, 1 line each) — functionally identical outcome to D-01 (boss death triggers save) without inventing a new event/observer layer, and without touching the shared `BossStatsSystem` base class (which would risk ripple effects across `NewBoss`, `WaterMonster`, `WaterSpirit` simultaneously).

**Anti-pattern to avoid:** Do NOT add a new `public event Action OnDeath` to `BossStatsSystem` just to "unify" the two architectures. That touches a shared base class used by at least 3 boss types (`BossStatesSystem.cs` is the base for `NewBoss`, `SpiritStats`, `WaterMonsterStats`), is out of this phase's minimal-integration scope (D-01), and both subclasses already override `Die()` without calling `base.Die()`, so the new event would silently never fire anyway unless those overrides are also touched — same amount of surgery as just calling `SaveLoadManager.Instance.Save()` directly, but with a bigger blast radius.

### Pattern 4: PlayerStats Restore Needs a New Public Method (fields are `protected`)

**What:** `HP.cs` declares `health` and `maxHealth` as `protected float` (`[SerializeField] protected float health;` / `maxHealth`, lines 14-17). `PlayerStats.cs` (subclass) adds `private float maxTotalHealth` with only a public **getter** (`public float MaxTotalHealth { get; }`, line 28) — no setter exists anywhere.

**Implication:** `SaveLoadManager` cannot do `PlayerStats.Instance.health = savedHealth;` from outside — it's `protected`, and `PlayerStats` doesn't currently expose a way to set it either. The plan needs a small, additive method on `PlayerStats` (which *does* have protected access to `health`/`maxHealth` via inheritance from `HP`) such as:

```csharp
// New method on PlayerStats.cs — additive, does not touch existing methods
public void RestoreStats(float savedHealth, float savedMaxHealth, float savedMaxTotalHealth)
{
    maxHealth = savedMaxHealth;
    maxTotalHealth = savedMaxTotalHealth;
    health = savedHealth;
    ClampHealth();
}
```

This is a **new public method**, not a modification of `Heal()`/`TakeDamage()`/`AddHealth()` — satisfies "정밀한 변경" (don't touch existing method bodies).

**`PlayerStats.Instance` timing:** `PlayerStats.Instance` is a lazy `FindAnyObjectByType<PlayerStats>()` (not cached in `Awake`, not `DontDestroyOnLoad` — re-resolves on every access since `instance` is only cached after first successful find and reset to null happens naturally when the object is destroyed on scene unload... **actually it does NOT reset**: `instance` is a `private static PlayerStats instance` that is only set once and never cleared. After a scene reload destroys the old Player object, `instance` still points to a destroyed (null-equivalent in Unity's overridden `==`) object, but the `Instance` getter's `if (instance == null)` check uses Unity's overloaded null check, which correctly detects the destroyed object as null and re-finds the new one. This works correctly across scene loads — confirmed by reading the getter logic, no fix needed, just be aware when calling `PlayerStats.Instance` after a scene load that Unity's `==` override is what makes this safe.**

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Dictionary/collection JSON serialization | Custom key-value list serializer for `Dictionary<string,bool>` | `JsonConvert.SerializeObject` / `DeserializeObject` (Newtonsoft) | Newtonsoft.Json natively round-trips `Dictionary<TKey,TValue>` (keys as JSON object property names when `TKey` is `string`) — zero custom code needed |
| Waiting for async scene load completion | Manual polling loop with `Time.deltaTime` timers or fixed-delay `WaitForSeconds` | `yield return asyncOp;` (or `while (!asyncOp.isDone) yield return null;`) inside a coroutine | `AsyncOperation` is directly yieldable in a Unity coroutine; both patterns are official/idiomatic (verified via Unity scripting docs and community sources) |
| Path construction for the save file | String concatenation with hardcoded `/` | `System.IO.Path.Combine(Application.persistentDataPath, "save.json")` | Cross-platform path separator correctness; `Application.persistentDataPath` already returns a platform-correct OS path (Windows: `%userprofile%/AppData/LocalLow/<company>/<product>/`) |

**Key insight:** This phase has very little "temptation to hand-roll" risk — Newtonsoft.Json and Unity's own scene/file APIs cover the entire technical surface. The actual risk in this phase is architectural (matching the two different boss-death shapes, matching the name-based spawn system) rather than "reinventing a wheel."

## Common Pitfalls

### Pitfall 1: Newtonsoft.Json Is a Transitive Dependency, Not a Direct One
**What goes wrong:** `Packages/manifest.json` has no `com.unity.nuget.newtonsoft-json` entry. It currently resolves only because `com.unity.ai.assistant` (2.9.0-pre.2) and `com.unity.ai.inference` (2.6.1) both declare it as their own dependency (confirmed in `packages-lock.json`, both list `"com.unity.nuget.newtonsoft-json": "3.2.1"` as a sub-dependency — note this is the *dependency version constraint*, while the actually resolved/installed version is 3.2.2 per the lockfile's own top-level entry and the cached package's `package.json`). If either of those AI packages is later removed (e.g. a future cleanup phase, a Unity upgrade that drops them, or a package cache reset that re-resolves without one of them), Newtonsoft.Json silently disappears and every `using Newtonsoft.Json;` in `SaveLoadManager.cs` fails to compile.
**Why it happens:** Unity Package Manager only guarantees a transitive package stays resolved as long as *something* still depends on it. There is no "pin this transitive package directly" without editing `manifest.json`.
**How to avoid:** Add `"com.unity.nuget.newtonsoft-json": "3.2.2"` directly to `Packages/manifest.json`'s `dependencies` object as part of this phase's implementation (a one-line, low-risk, in-scope change — it does not add new functionality, it just makes an already-used dependency explicit and safe). This is a reasonable, minimal action directly required to make the phase's core technology (Newtonsoft.Json) durable, not scope creep.
**Warning signs:** If `manifest.json` is left untouched and a later, unrelated phase removes `com.unity.ai.assistant` or `com.unity.ai.inference` (e.g. to slim the project), `SaveLoadManager.cs` would fail to compile with no obvious connection to the actual cause.

### Pitfall 2: Two Different Boss-Death Architectures — a Uniform "Subscribe to OnDeath" Plan Will Half-Fail
**What goes wrong:** A plan written assuming "every boss has an `OnDeath` event, just subscribe `SaveLoadManager.Instance.Save` to it" will work for `TutorialBossController`/`WoodBossController` (backed by `HP.OnDeath`) but silently do nothing for `SpiritController`/`WaterMonsterController` (backed by `BossStatsSystem`, which has no such event) — the water boss defeats simply won't trigger a save, and this will not throw any compile or runtime error, making it a silent functional gap.
**Why it happens:** The codebase evolved two parallel boss-stats hierarchies (`HP`-based for Tutorial/WoodBoss, `BossStatsSystem`-based for NewBoss/WaterSpirit/WaterMonster) that happen to share naming conventions (`Die()`, `OnDeath`-ish semantics) but not actual event wiring.
**How to avoid:** Treat this as 4 distinct, individually-verified integration points, not 1 event subscription: (1) `TutorialBossController.HandleDeath()` — add call, (2) `WoodBossController.HandleDeath()` — add call, (3) `SpiritStats.Die()` — add call, (4) `WaterMonsterStats.Die()` — add call. See Pattern 3 above for exact insertion points.
**Warning signs:** Grep for `OnDeath` before writing the plan's boss-integration tasks — if a boss doesn't show up owning/subscribing to an `OnDeath` event, its `Die()`/death method must be located and the save call added there directly instead.

### Pitfall 3: `WoodBossStatsSystem.cs` Looks Relevant But Is Dead Code
**What goes wrong:** `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs` defines `class WoodBossStatsSystem : MonoBehaviour` with its own `public event Action OnDeath;` (separate from `HP.OnDeath`) — a planner or implementer skimming file names might reasonably assume this is WoodBoss's actual stats/death system and wire the save call there.
**Why it happens:** The filename strongly suggests it's the active system, and it independently reimplements a health/OnDeath pattern that looks legitimate.
**How to avoid:** `grep -r "WoodBossStatsSystem"` across `Assets/` shows it is referenced **only inside its own file** — `WoodBossController.cs` actually uses `GetComponent<HP>()`, not `GetComponent<WoodBossStatsSystem>()`. This class is orphaned/unused, same category as `GameManager.NextSpawnPointName`. Do not wire anything to it; mention only, per "기존 코드 존중" principle.
**Warning signs:** Any GameObject in the WoodBoss prefab that has a `WoodBossStatsSystem` component attached but where `WoodBossController._hp` resolves via `GetComponent<HP>()` instead confirms the component (if present) is inert for this purpose.

### Pitfall 4: Storing Raw Coordinates Instead of `sceneName` + `spawnPointName` Breaks the Only Working Respawn Path
**What goes wrong:** If the save schema stores `Vector2 position` / raw `x, y` floats and `LoadGame()` tries to `transform.position = savedPosition` directly, it must build a whole parallel restore path that bypasses `PlayerSpawner`, since `PlayerSpawner.ApplySpawn()` only understands named-object lookup. This means either (a) writing new position-setting code that runs after the new Player object exists (extra complexity, another place that must get post-scene-load timing right) or (b) a race where `PlayerSpawner.Start()` runs and does nothing (because `targetSpawnPointName` is empty), leaving the player at the prefab's default position while a separate late call repositions it — fragile ordering.
**Why it happens:** The phase description's "씬+좌표" wording naturally reads as "store X/Y coordinates," but the codebase's only functioning coordinate-restore mechanism is name-based, not float-based.
**How to avoid:** Model the save schema's location field as `string sceneName` + `string spawnPointName`, and have `LoadGame()` do exactly what `SignpostPortal.cs` does: set the static field, then load the scene. See Pattern 2.
**Warning signs:** If implementation ends up needing to search for "how do I move the player after LoadSceneAsync completes" as new code (rather than reusing `PlayerSpawner`), that's a sign the schema chose raw coordinates and reinvented the existing mechanism.

### Pitfall 5: `allowSceneActivation` Default and Coroutine Completion Ordering
**What goes wrong:** `SceneManager.LoadSceneAsync` defaults to `allowSceneActivation = true`, meaning the scene activates automatically as soon as loading reaches ~90% progress, without any deliberate blocking step. If a coroutine does `yield return SceneManager.LoadSceneAsync(sceneName);` and, on the very next line, immediately calls something like `PlayerStats.Instance.RestoreStats(...)`, that call fires on the **same frame** the scene became active — Unity guarantees `Awake()`/`OnEnable()`/`Start()` for the newly-activated scene's objects have already run by the time `yield return op;` resumes (this is standard, well-documented coroutine/AsyncOperation behavior), so `PlayerStats.Instance` (lazy `FindAnyObjectByType`) will correctly find the new scene's `PlayerStats` component. No extra "wait one more frame" is required, but this ordering fact should be explicitly verified with a `Debug.Log` in the plan's verification step rather than assumed, since D-04 stipulates no loading-screen UI to visually confirm the wait.
**Why it happens:** Async operation completion semantics interacting with lazy singleton lookups are a common source of "works most of the time, sometimes finds null" bugs if assumptions aren't verified.
**How to avoid:** Plan's verification step for the load flow should include an explicit runtime check (e.g. temporary `Debug.Log` or Play-mode manual check per project convention — see `Assets/Camera/Check.md` precedent for this project's verification style) confirming `PlayerStats.Instance` is non-null and has the restored values immediately after the load coroutine completes.
**Warning signs:** `NullReferenceException` on `PlayerStats.Instance` immediately after a load, or restored stats not applying (silently finding the *old, destroyed* `PlayerStats` object due to Unity's fake-null check edge cases if `DontDestroyOnLoad` were mistakenly added to `PlayerStats` in some future change — currently it is not, so this is a latent risk to note but not fix now).

## Code Examples

### Async Scene Load via Coroutine (project-consistent pattern)
```csharp
// Source: Unity official docs (https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html)
// + cross-verified against community sources; adapted to this project's IEnumerator/StartCoroutine
// convention (no async/await anywhere else in the codebase — see BossController.cs's
// StartCoroutine(HeavyAttackRoutine(cooldown)) for the established idiom)
private IEnumerator LoadSceneAndRestoreRoutine(string sceneName, string spawnPointName)
{
    PlayerSpawner.targetSpawnPointName = spawnPointName; // consumed by the new Player's own Start()

    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
    yield return op; // AsyncOperation is directly yieldable

    // At this point the new scene's Awake()/Start() have already executed,
    // including PlayerSpawner.Start() -> ApplySpawn() (position already restored).
    ApplyPlayerStatsFromSave();
}
```

### Newtonsoft.Json Save/Load with Dictionary Support
```csharp
// Source: Newtonsoft.Json official docs (https://www.newtonsoft.com/json/help/html/DefaultSettings.htm)
// + cross-verified pattern used broadly in Unity + Newtonsoft integration guides
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

private static readonly JsonSerializerSettings SaveSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,       // human-readable/hand-editable, small file, not perf-critical
    NullValueHandling = NullValueHandling.Include, // explicit over implicit for a hand-editable save file
};

public void Save()
{
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    string json = JsonConvert.SerializeObject(_currentSaveData, SaveSettings);
    File.WriteAllText(path, json);
}

public bool HasSaveFile()
{
    return File.Exists(Path.Combine(Application.persistentDataPath, "save.json"));
}

private SaveData LoadFromDisk()
{
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    string json = File.ReadAllText(path);
    return JsonConvert.DeserializeObject<SaveData>(json, SaveSettings);
}
```

### Dictionary Stub Fields (Newtonsoft handles natively — no custom converter needed)
```csharp
// Source: Newtonsoft.Json official docs — Dictionary<string,TValue> serializes as a plain
// JSON object with keys as property names, natively, no [JsonConverter] attribute required.
public class SaveData
{
    public string SceneName;
    public string SpawnPointName;
    public PlayerStatsSaveData PlayerStats = new PlayerStatsSaveData();
    public Dictionary<string, bool> BossProgress = new Dictionary<string, bool>();     // D-03 stub
    public Dictionary<string, bool> MapGimmickState = new Dictionary<string, bool>();  // D-03 stub
    public List<string> Items = new List<string>();                                    // D-03b stub
}

public class PlayerStatsSaveData
{
    public float Health;
    public float MaxHealth;
    public float MaxTotalHealth;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|---------------|--------|
| Unity `JsonUtility` for save data | Newtonsoft.Json for anything involving dictionaries/polymorphism | N/A — `JsonUtility`'s dictionary limitation has existed since its introduction; this project has never used `JsonUtility` for save data at all (no prior save system existed) | No migration concern — this is a greenfield choice, not a replacement of prior save code |

**Deprecated/outdated:** Not applicable — no prior save/load system exists in this codebase to deprecate.

## Open Questions

1. **What GameObject name should represent the "이어하기" (continue-game) coordinate when the last save was a boss-defeat auto-save, not a checkpoint interaction?**
   - What we know: Checkpoint activation naturally gives a `spawnPointName` (the checkpoint's own `gameObject.name`, or a checkpoint-associated named point). Boss-defeat auto-save has no obvious analogous named point — the boss's own defeat position isn't a valid `PlayerSpawner` target unless a named object exists there.
   - What's unclear: Whether boss-defeat auto-save should reuse "the last activated checkpoint's name" (simplest, matches typical Soulslike-style checkpoint respawn semantics) or something else.
   - Recommendation: Default to "boss defeat auto-save reuses whatever `spawnPointName` the last checkpoint activation stored" (i.e., `Save()` from the boss-death path writes the *previously recorded* `sceneName`/`spawnPointName` from memory cache, not a new one) — this requires zero new spawn-point infrastructure and matches D-03's "minimize what's actually filled in" spirit. Planner should confirm this framing explicitly rather than leaving it implicit.

2. **Exact folder/namespace placement for `SaveLoadManager.cs` and `SaveData.cs`.**
   - What we know: No `.asmdef` exists, so folder placement doesn't affect compilation. Precedent folders `Assets/Script/` (global singletons/utilities like `HP.cs`, `GameStateManager`-adjacent `MainMenuUI.cs`) and `Assets/Player/Script/` both exist.
   - What's unclear: Which is more "canonical" for a new cross-cutting manager — the codebase doesn't have a single consistent convention.
   - Recommendation: `Assets/Script/` (already home to project-wide singletons/utilities like `HP.cs`) is the closest existing precedent for a new cross-cutting manager; planner should decide during plan authoring, not deferred further.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Unity Editor | All implementation/compilation | ✓ (per `ProjectVersion.txt`) | 6000.3.10f1 | — |
| `com.unity.nuget.newtonsoft-json` | Core save/load serialization | ✓ (transitive, cached) | 3.2.2 resolved | Add direct `manifest.json` entry (Pitfall 1) — no code fallback needed, package is already present on disk |
| `.asmdef` / assembly boundaries | Confirms `using Newtonsoft.Json;` compiles from any new script | ✓ — none exist in `Assets/`, none in the Newtonsoft package itself | n/a | — |

**Missing dependencies with no fallback:** None.

**Missing dependencies with fallback:** `com.unity.nuget.newtonsoft-json` direct manifest entry (currently transitive-only) — fallback is "leave as transitive" but not recommended (see Pitfall 1).

*(Validation Architecture section omitted — `.planning/config.json` has `workflow.nyquist_validation: false`.)*

## Sources

### Primary (HIGH confidence — direct file reads from this repo)
- `Assets/map/script/GameManager.cs` — singleton `Awake()` pattern
- `Assets/map/script/PlayerSpawner.cs` — `targetSpawnPointName` / `ApplySpawn()` (the real respawn mechanism)
- `Assets/map/script/Checkpoint.cs` — S-key checkpoint activation logic, integration point for D-01
- `Assets/map/script/SignpostPortal.cs` — the actual working scene-transition caller of `PlayerSpawner`
- `Assets/map/script/portal.cs` (Portal class) — confirmed orphaned `GameManager.Instance.NextSpawnPointName` usage, per D-05
- `Assets/map/script/PlayerRespawn.cs` — `SyncStartPosition`/`UpdateCheckpoint`, consumed by `ApplySpawn()`
- `Assets/Player/Script/PlayerStats.cs`, `Assets/Script/HP.cs` — field access levels (`protected health/maxHealth`, `private maxTotalHealth` with getter-only), `OnDeath` event definition, singleton `Instance` getter behavior
- `Assets/Player/Script/GameStateManager.cs`, `Assets/Player/Script/InputHandler.cs` — additional singleton `DontDestroyOnLoad` precedent confirming the pattern
- `Assets/Script/MainMenuUI.cs` — confirms D-04 scope boundary (`OnClickStart` untouched)
- `Assets/Enemy/NewBoss/Script/BossController.cs`, `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` — confirmed no `OnDeath` event exists on the `BossStatsSystem` lineage
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs`, `Assets/Enemy/WaterSpirit/Script/SpiritController.cs` — confirmed `Die()` override has no event, no death-handling in controller
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs`, `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — same confirmation for WaterMonster
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs`, `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs` — confirmed working `HP.OnDeath` subscription (`HandleDeath`)
- `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs` — confirmed orphaned (`grep` shows zero external references)
- `Packages/manifest.json` — confirmed no direct `com.unity.nuget.newtonsoft-json` entry
- `Packages/packages-lock.json` — confirmed transitive resolution (via `com.unity.ai.assistant`, `com.unity.ai.inference`) and resolved version 3.2.2
- `Library/PackageCache/com.unity.nuget.newtonsoft-json@4dfd81071c64/package.json` — confirmed cached version 3.2.2, no `.asmdef` in the package
- `ProjectSettings/ProjectVersion.txt` — confirmed Unity 6000.3.10f1
- Grep across `Assets/` for `DontDestroyOnLoad`, `LoadSceneAsync`, `async|Task|await|IEnumerator`, `OnDeath`, `.asmdef` — confirmed no async/Task usage anywhere, no `.asmdef` anywhere under `Assets/`
- `.planning/config.json` — confirmed `workflow.nyquist_validation: false`

### Secondary (MEDIUM confidence — WebSearch cross-verified against official docs)
- [Unity Scripting API: SceneManager.LoadSceneAsync](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html) — confirms `AsyncOperation` yieldability and `allowSceneActivation` semantics
- [Newtonsoft.Json — Serialize with DefaultSettings](https://www.newtonsoft.com/json/help/html/DefaultSettings.htm) — confirms `Formatting.Indented` / `JsonSerializerSettings` usage pattern
- Community sources (Medium articles on Unity + Newtonsoft.Json, Unity Discussions thread on `LoadSceneAsync` + coroutines) — used only to corroborate the official-docs pattern, not as sole evidence for any claim

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — verified directly against `packages-lock.json` and cached package files on disk, not training-data assumption
- Architecture (singleton pattern, boss-death hooks, spawn-point restore): HIGH — every claim traced to an actual line number in an actual project file
- Async scene-load timing (Awake/Start-before-isDone guarantee): MEDIUM — well-established Unity behavior, cross-verified with official docs + community sources, but not something this project has exercised before (first `LoadSceneAsync` usage in the codebase), so flagged for explicit runtime verification in the plan
- Pitfalls: HIGH — all five pitfalls are grounded in direct code inspection, not speculation

**Research date:** 2026-08-09
**Valid until:** Stable for this project's lifetime unless `Packages/manifest.json`/`packages-lock.json` changes (e.g. an AI package removal) or a boss controller is refactored — recommend re-verifying Pitfall 1 and Pitfall 2/3 findings if either occurs before this phase is implemented.
