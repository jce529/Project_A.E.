# Phase 9: 카메라 줌 전환 & X축 이동 제한 - Research

**Researched:** 2026-07-30
**Domain:** Unity 2D Camera scripting (orthographic zoom Lerp, trigger-based state, position clamping)
**Confidence:** HIGH

## Summary

This phase extends `Assets/Camera/Script/CameraController.cs` (currently a pure position-follow script — `Transform target` + `Vector3.Lerp` in `LateUpdate`) with two independent features: (1) a Lerp-based `orthographicSize` zoom that a separately-placed trigger collider script toggles on boss-stage entry/exit, and (2) a hard X-axis clamp on the camera's final position that accounts for the camera's current half-width (`orthographicSize * aspect`).

The codebase has a strong, consistent, pre-existing convention for exactly this kind of cross-object communication: a `public static X Instance { get; private set; }` singleton set in `Awake()`, used by `GameManager`, `PlayerStats`, `AudioManager`, and `GameStateManager`. There is no Cinemachine, no `Camera.main` caching pattern for controller scripts, and no existing zoom code anywhere in the project — this is genuinely new ground, but the surrounding conventions (singleton access, `OnTriggerEnter2D`/`OnTriggerExit2D` + `CompareTag("Player")`, `[SerializeField] private` fields, `[Header(...)]` grouping, no namespace on top-level gameplay scripts) are unambiguous and should be followed exactly. Since `CameraController.cs` already lives on the Camera GameObject itself (it directly sets `transform.position`), the idiomatic reference to the `Camera` component is `GetComponent<Camera>()` in `Awake()`, not `Camera.main` (which the project reserves for one-off raycasting scripts like `PlayerAttack.cs` and `InteractivePrompt.cs` that live on other objects).

**Primary recommendation:** Add a singleton (`CameraController.Instance`) + a public method (e.g. `SetBossZoom(bool)`) to `CameraController.cs`, keep the trigger detection in a brand-new separate MonoBehaviour (e.g. `BossZoomTrigger.cs`) that calls that method on `OnTriggerEnter2D`/`OnTriggerExit2D` with a `CompareTag("Player")` guard — matching the existing `Portal.cs` trigger pattern exactly. Apply the X-axis clamp as the last step in `LateUpdate`, after both the position-follow Lerp and the zoom Lerp have computed their frame's values, using `cam.orthographicSize * cam.aspect` for the half-width.

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** 보스 스테이지 진입은 씬 로드가 아니라 **트리거 콜라이더**(`BoxCollider2D`, Is Trigger) 기반으로 감지한다.
- **D-02:** 트리거 콜라이더는 보스마다 **사용자가 에디터에서 수동으로 배치**한다. 보스 GameObject에 자동 부착하지 않는다.
- **D-03:** `OnTriggerExit2D`로 트리거 영역을 벗어나면 **즉시 자동으로 일반 스테이지 줌 값으로 복귀**한다. 보스 처치 등 별도 이벤트는 필요 없다.
- **D-04:** 목표 줌 값(orthographic size)은 **일반 = 5, 보스 = 7**. 확정값이 아니라 플레이테스트로 조정될 초기값.
- **D-05:** 줌 값은 **Inspector 필드로 노출**한다 (하드코딩 금지).
- **D-06:** 줌 전환은 **부드럽게(Lerp)** 처리한다. 즉시 전환 아님.
- **D-07:** 줌 전환 속도는 기존 `smoothing`(5f) 필드와 **별도의 속도 필드로 분리**한다 (예: `zoomSmoothing`).
- **D-08:** 이번 Phase 범위는 **트리거 스크립트 + 카메라 줌 로직 구현까지만**. 실제 보스 씬에 트리거 콜라이더를 배치하는 에디터 작업은 범위 밖.
- **D-09:** 카메라 **X축 이동 범위를 min/max 값으로 제한**한다. **Y축은 이번 Phase에서 제한하지 않는다.**
- **D-10:** 경계값은 `BoxCollider2D` 참조 방식(Phase 4 `mapBounds` 패턴)이 아니라 **`minX`/`maxX` float Inspector 필드**로 직접 지정한다.
- **D-11:** 클램프 계산 시 카메라 중심점만이 아니라 **화면 반폭(`orthographicSize * camera.aspect`)까지 감안**한다.

### Claude's Discretion

- 새 트리거/줌 컴포넌트의 클래스/파일 이름
- 트리거 스크립트가 `CameraController`를 참조하는 방식 (싱글톤 vs `FindObjectOfType` vs Inspector 직접 참조 등)
- 줌 전환·클램프 로직을 `LateUpdate` 내 어느 순서로 적용할지

### Deferred Ideas (OUT OF SCOPE)

- 실제 보스 씬(WaterMonster/WaterSpirit/TutorialBoss)에 `BossZoomTrigger` 콜라이더를 배치하는 에디터 작업 — 사용자가 기능 완성 후 직접 진행 (D-08).
- Y축 이동 범위 제한 — 이번 Phase 범위 아님.

## Project Constraints (from CLAUDE.md)

- **Surgical changes only:** Only touch `CameraController.cs` and add net-new files; do not refactor unrelated code, do not "improve" existing comments/formatting in `CameraController.cs` beyond what the new fields/methods require.
- **No premature abstraction / no over-engineering:** Don't add generic "camera state machine" or event-bus infrastructure for a two-value zoom toggle — a bool-driven Lerp target is sufficient (matches D-06/D-07 as literally stated).
- **Phase isolation:** Do not place trigger colliders in boss scenes (that's D-08, explicitly deferred) and do not implement Y-axis clamping (D-09 explicitly excludes it).
- **Traceability:** Every changed/added line must map to a decision in CONTEXT.md (D-01 through D-11) or a technical concern proven in this research (encoding, singleton pattern, clamp math).
- **Verification via test-and-pass framing:** Where GSD's `verify-work` step later checks this phase, it will look for: zoom transitions on trigger enter/exit, revert-on-exit behavior, Inspector-tunable zoom/smoothing/clamp values, and camera never showing outside `[minX, maxX]` at either zoom level.

## Current CameraController.cs (exact structure to extend)

Read in full — `Assets/Camera/Script/CameraController.cs`, 25 lines:

```csharp
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 목표물
    public float smoothing = 5f;

    // [설명] 오프셋은 내부에서 계산하지 않고, 기본값은 여기서 정해둡니다.
    // (0, 0, -10)는 2D 게임의 표준 배치입니다. (X,Y는 안맞춤, Z는 뒤로 10만큼)
    public Vector3 offset = new Vector3(0f, 0f, 10f);

    void Start()
    {
        // [중요] 시작 카메라 흔들림 없도록,
        // 최초 프레임부터는 타겟의 '이상적인 위치(오프셋 적용)'로 순간이동시킵니다.
        transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}
```

**Encoding note:** The Read tool renders the Korean comments in this file as mojibake (`������` garbage) regardless of read attempt — the file is almost certainly saved in a non-UTF8 codepage (likely CP949/EUC-KR, common for Korean Unity projects edited in older Visual Studio/MonoDevelop setups). This is a **pre-existing condition of the file**, not a new issue introduced by this research. **Implication for the planner/executor:** when editing this file, use a tool path that preserves the existing byte encoding of untouched lines (e.g., targeted string replacement rather than a full UTF-8 rewrite of the whole file) to avoid corrupting the existing Korean comments further. If the edit tool used cannot guarantee encoding preservation, flag this explicitly as a risk in the plan and verify the file still opens correctly in Unity/VS after the edit. New comments added by this phase should use the same style (Korean, `[설명]`/`[중요]`-style headers) for consistency, but be aware of the encoding risk when writing them.

No `Camera` component reference, no `orthographicSize`/zoom field, no X-clamp logic exists — 100% net-new additions confirmed by full-file read (nothing to search for and remove).

**Fields/methods confirmed:**
- `public Transform target`
- `public float smoothing = 5f`
- `public Vector3 offset = new Vector3(0f, 0f, 10f)`
- `void Start()` — snaps to `target.position + offset` on frame 1
- `void LateUpdate()` — the only place per-frame logic runs; new zoom Lerp and X-clamp logic must be added here, after the existing position-follow line

## Standard Stack

### Core

| API | Version | Purpose | Why Standard |
|-----|---------|---------|---------------|
| `UnityEngine.Camera` (built-in component) | Unity 6000.3.10f1 (confirmed via `ProjectSettings/ProjectVersion.txt`) | Exposes `orthographicSize` and `aspect` | No Cinemachine package is used anywhere in this project (confirmed — CONTEXT.md states this and no `Cinemachine` references found in `Assets/`); plain `Camera` API is the only option and matches project convention |
| `Vector3.Lerp` / `Mathf.Lerp` | Built-in | Smooth interpolation | Already the established pattern in `CameraController.LateUpdate` for position; `Mathf.Lerp` is the scalar equivalent for `orthographicSize` (a `float`) |
| `Mathf.Clamp` | Built-in | X-axis hard clamp | Standard Unity idiom for bounding a scalar between min/max |

No external packages need installing — this is 100% built-in `UnityEngine` API, no `npm install` / no `Packages/manifest.json` changes.

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Plain `Camera` + manual Lerp | Cinemachine (`CinemachineVirtualCamera` + `CinemachineConfiner2D` + zoom via Lens) | Cinemachine is the modern Unity-recommended approach for exactly this (zoom transitions, bounds confining) and would eliminate hand-rolled Lerp/clamp code — but it is a separate package not currently installed, would require scene rework (camera becomes Cinemachine Brain + virtual cams), and directly violates CLAUDE.md's "surgical changes" + "no over-engineering for out-of-scope needs" principle for a 2-value zoom + 1-axis clamp. **Not recommended for this phase** — flagged only so the planner is aware it exists as a future option if camera requirements grow (e.g., multiple boss zoom levels, screen shake, split-screen).
| Singleton (`CameraController.Instance`) | `FindObjectOfType<CameraController>()` per trigger, or direct Inspector drag-in reference | See Architecture Patterns section below — singleton chosen to match 4-for-4 existing codebase convention. |
| `minX`/`maxX` float fields | `BoxCollider2D`-referenced bounds (Phase 4 `WeatherController.mapBounds` pattern) | Explicitly rejected by user in D-10 — noted here only for completeness, not to be revisited. |

## Architecture Patterns

### Recommended Project Structure

No new folders needed — both new files go in the existing camera script folder:

```
Assets/Camera/Script/
├── CameraController.cs      # EXTEND: add zoom fields, Instance singleton, SetBossZoom(), zoom Lerp + X-clamp in LateUpdate
├── CameraController.cs.meta # unchanged (existing GUID must be preserved — do not regenerate)
├── BossZoomTrigger.cs        # NEW: trigger collider script, placed on user's future boss-zone GameObjects
└── BossZoomTrigger.cs.meta   # NEW (Unity auto-generates on first import; if hand-authoring, let Unity generate it — do not fabricate a GUID)
```

**Naming rationale:** `BossZoomTrigger` directly names its single responsibility (trigger boss zoom), matches the project's existing zone-trigger naming style (`SpeedUpZone`, `SlowDownZone`, `FallZone`), and avoids ambiguity with the already-existing `Portal.cs` trigger (which handles scene transitions, an explicitly separate concern per CONTEXT.md's Integration Points section).

### Pattern 1: Singleton Instance for Manager-Style MonoBehaviours

**What:** `public static CameraController Instance { get; private set; }`, assigned in `Awake()`.

**When to use:** Any script that (a) exists exactly once per scene, and (b) needs to be called into from other, unrelated scripts without an Inspector-wired reference.

**Why this fits `CameraController`:** There is exactly one camera per scene (confirmed: `CameraController.cs` is a `MonoBehaviour` that directly manipulates `transform.position`, implying it lives on the single Camera GameObject). This is architecturally identical to `GameManager`, `PlayerStats`, `AudioManager`, and `GameStateManager` — all of which use this exact singleton shape in this codebase. Confidence: HIGH — verified by reading all four files' `Instance` declarations directly.

**Example (verified from `Assets/Player/Script/GameStateManager.cs` and `Assets/Script/AudioManager.cs`, both use this identical shape):**
```csharp
// Source: Assets/Script/AudioManager.cs (existing project code)
public static AudioManager Instance { get; private set; }

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
    else
    {
        Destroy(gameObject);
    }
}
```

**Applied to CameraController — recommended shape (not yet written, this is guidance for the planner):**
```csharp
public static CameraController Instance { get; private set; }

private Camera cam;

void Awake()
{
    Instance = this;
    cam = GetComponent<Camera>();
}
```

**Important deviation from `GameManager`/`AudioManager`:** Those managers use `DontDestroyOnLoad(gameObject)` because they persist across scene loads. `CameraController` is scene-local (a new Camera exists in each scene per the project's per-stage-scene structure noted in CONTEXT.md's Integration Points) — **do NOT add `DontDestroyOnLoad`** to `CameraController.Awake()`. This is a case where the pattern must be adapted, not copied verbatim; blindly copying `DontDestroyOnLoad` would cause duplicate/orphaned camera issues across scene transitions. Flag this explicitly in the plan so the executor doesn't over-apply the pattern.

Also note: `Start()` currently does `transform.position = target.position + offset;` — if `Awake()` is added for the singleton, ensure `Awake()` still runs before `Start()` (Unity's default lifecycle guarantees this within the same GameObject, so no explicit ordering work is needed).

### Pattern 2: Trigger Zone Talks to Target via Direct Call, Guarded by Tag/Layer

**What:** A trigger-collider `MonoBehaviour` implements `OnTriggerEnter2D`/`OnTriggerExit2D`, checks `CompareTag("Player")` (or layer, both patterns exist in-project), then calls directly into the target system.

**When to use:** Any "zone that changes some other system's state when the player enters/exits" — exactly this phase's boss-zoom trigger.

**Verified precedent 1 — `Assets/map/script/portal.cs` (tag-based, closest analog: also `OnTriggerEnter2D`/`OnTriggerExit2D` pair with immediate revert on exit):**
```csharp
// Source: Assets/map/script/portal.cs (existing project code)
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player")) isPlayerNearby = true;
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player")) isPlayerNearby = false;
}
```

**Verified precedent 2 — `Assets/Enemy/WaterMonster/Script/Phase4/SpeedUpZone.cs` (layer-based, shows the "call directly into another component" idiom with an inline comment tracing back to a REQ-ID):**
```csharp
// Source: Assets/Enemy/WaterMonster/Script/Phase4/SpeedUpZone.cs (existing project code)
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
    var pc = other.GetComponentInParent<PlayerController>();
    if (pc != null) pc.currentSpeedModifier = speedMultiplier;
}
```

**Recommendation for `BossZoomTrigger.cs`:** Use the `CompareTag("Player")` style (matches `Portal.cs`, the closer analog since both are OnTriggerEnter2D/OnTriggerExit2D *pairs* with revert-on-exit semantics — `SpeedUpZone` is also a pair but the tag-vs-layer choice is a wash stylistically; tag matches the more directly analogous script). Call `CameraController.Instance.SetBossZoom(true)` on enter, `CameraController.Instance.SetBossZoom(false)` on exit — no per-trigger config needed since D-04's zoom values live on `CameraController`, not the trigger (keeps `BossZoomTrigger` a dumb, reusable, zero-field trigger that any boss scene can drop in per D-02).

```csharp
// Recommended shape (net-new file, not yet written)
using UnityEngine;

public class BossZoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetBossZoom(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetBossZoom(false);
    }
}
```

**Null-safety consideration:** Unlike `SpeedUpZone`/`FallZone` which null-check the `GetComponent` result (because the target component might not exist on the colliding object), `CameraController.Instance` is a scene-wide singleton assumed to always exist. Following `Portal.cs`'s precedent (`GameManager.Instance.NextSpawnPointName` is called with zero null-check), a null-check is **not** required for consistency with codebase style — but if `Instance` is null (e.g., trigger fires before `CameraController.Awake()` runs, or the Camera object was deleted from the scene), it will throw a `NullReferenceException`. This is an accepted/established risk pattern in this codebase; no change needed, just noting it as a known-and-intentional inconsistency with defensive coding elsewhere.

### Pattern 3: LateUpdate Ordering — Follow → Zoom Lerp → Clamp

**What:** Within a single `LateUpdate()`, compute position-follow first, then zoom, then clamp the position using that frame's (possibly just-changed) zoom value.

**Why this order matters:** D-11 requires the clamp's half-width term (`orthographicSize * aspect`) to reflect the *current* zoom, including mid-transition. If the clamp ran before the zoom Lerp updated `cam.orthographicSize`, it would clamp against last frame's zoom value — a one-frame lag that's harmless in isolation but conceptually wrong and would compound into visible jitter during the ~1-2 second zoom transition (5→7 or 7→5) when the camera is also near a clamped edge. Correct order avoids this by construction.

**Recommended `LateUpdate` shape (illustrative — exact code is the planner/executor's job, this establishes the required sequence and math only):**
```csharp
void LateUpdate()
{
    if (target == null) return;

    // 1. Existing position-follow (unchanged logic)
    Vector3 targetCamPos = target.position + offset;
    transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);

    // 2. NEW: zoom Lerp toward current target size
    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSmoothing * Time.deltaTime);

    // 3. NEW: X-axis clamp, using THIS frame's post-Lerp orthographicSize
    float halfWidth = cam.orthographicSize * cam.aspect;
    Vector3 pos = transform.position;
    pos.x = Mathf.Clamp(pos.x, minX + halfWidth, maxX - halfWidth);
    transform.position = pos;
}
```

**Confirms D-11's math precisely:** `Camera.aspect` (Unity built-in, `Camera.aspect = pixelWidth / pixelHeight`) times `orthographicSize` (which represents *half* the vertical view height in world units) gives half the *horizontal* view width in world units for an orthographic camera. This is the standard, well-known Unity formula for "how much world-space X does this ortho camera see on each side of center" — HIGH confidence, this is basic orthographic camera geometry, not something requiring external verification, and matches D-11's own stated formula verbatim.

### Anti-Patterns to Avoid

- **Sharing `smoothing` for both position-follow and zoom:** Explicitly rejected by D-07. They are conceptually different rates (position tracks a moving target continuously; zoom transitions between two fixed states occasionally) and tuning one will fight the other if shared.
- **Implementing the trigger as `OnTriggerEnter2D`/`Exit2D` directly on `CameraController` itself:** Would require the trigger collider to be a child of/or the same object as the Camera, which contradicts D-02 (boss-specific manual placement, separate from the camera). Keeping trigger and camera-control logic in separate files/GameObjects is required by the decisions as stated, not just a style preference.
- **Using `BoxCollider2D` bounds for the X clamp:** Explicitly rejected by D-10 in favor of raw `minX`/`maxX` floats — do not "improve" this into the Phase 4 `mapBounds` pattern even though that pattern exists and works elsewhere in the codebase.
- **Adding `DontDestroyOnLoad` to `CameraController`'s new `Awake()`:** Would break the per-scene camera model this project uses (each stage scene has its own Camera per CONTEXT.md's Integration Points note about scenes already being stage-separated). See Pattern 1 above.
- **Clamping before the zoom Lerp runs each frame:** Produces a stale half-width, see Pattern 3.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Smooth scalar transition between two float values over time | A custom coroutine/timer/easing-curve system for `orthographicSize` | `Mathf.Lerp(current, target, rate * Time.deltaTime)` | This is the exact pattern already proven in this file for `Vector3.Lerp` on position — one line, no new abstraction, directly consistent with D-06/D-07 and CLAUDE.md's anti-over-engineering rule. |
| Bounding a value between two limits | Manual `if (x < min) x = min; else if (x > max) x = max;` | `Mathf.Clamp(x, min, max)` | Built-in, one call, no reason to hand-roll — this is precisely the kind of "don't build what the framework already gives you" case. |
| Cross-object reference to a known-singleton manager | Inspector drag-and-drop reference per trigger instance, or a `FindObjectOfType<CameraController>()` call in each trigger's `Start()` | `CameraController.Instance` singleton, set once in `Awake()` | 4-for-4 precedent in this exact codebase (`GameManager`, `PlayerStats`, `AudioManager`, `GameStateManager`) — inventing a different access pattern here would be inconsistent with zero benefit. `FindObjectOfType` is also strictly worse (runtime search cost every call unless cached, and the codebase's own idiom already solves this via the static `Instance` field). |

**Key insight:** Every piece of new logic in this phase (Lerp-based scalar transition, min/max clamp, singleton manager reference, tag-guarded trigger pair) already has a proven, minimal, built-in-or-established-pattern solution directly visible in this same codebase or in bog-standard `UnityEngine` API. There is no case in this phase where a custom solution is justified.

## Common Pitfalls

### Pitfall 1: Orthographic half-width formula direction confusion

**What goes wrong:** Someone computes `halfWidth = orthographicSize / aspect` instead of `* aspect`, inverting the ratio and producing wildly wrong clamp bounds (over-restrictive on wide screens, under-restrictive on narrow ones).

**Why it happens:** `orthographicSize` is a *vertical* half-height; multiplying by `aspect` (width/height) converts it to a *horizontal* half-width. It's easy to misremember which direction the multiplication goes.

**How to avoid:** Use exactly `cam.orthographicSize * cam.aspect` as D-11 states verbatim — this is already the user-specified formula, don't second-guess or "correct" it.

**Warning signs:** In Play mode, camera view clips through map edges on wide aspect ratios but stops far short of edges on narrow ones (or vice versa) — a sign the multiplication is inverted.

### Pitfall 2: `minX`/`maxX` narrower than the current view width

**What goes wrong:** If `(maxX - minX) < 2 * halfWidth` (i.e., the allowed camera-center range is narrower than the camera's current view), `Mathf.Clamp(pos.x, minX + halfWidth, maxX - halfWidth)` receives a `min` argument greater than its `max` argument. `Mathf.Clamp` in this situation returns the `min` value unconditionally (per Unity's implementation, it does not throw), which still produces a defined (if visually odd — camera slightly overshoots one edge) result rather than a crash. This can silently occur when zoom is 7 (wider view) if `minX`/`maxX` were tuned while testing at zoom 5.

**Why it happens:** D-04's boss zoom (7) is wider than normal zoom (5), so the same map's `minX`/`maxX` pair must be wide enough to accommodate the *larger* of the two view widths, not just the normal one. This is easy to miss if `minX`/`maxX` are tuned only while standing in the normal-zoom part of a level.

**How to avoid:** When documenting/tuning `minX`/`maxX` values (a per-scene, manual Inspector task outside this phase's code scope per D-08, but worth flagging for whoever eventually places these values), verify the clamp doesn't collapse at zoom=7, not just zoom=5. This is a tuning/testing note, not something the code needs to defensively guard against (no requirement in CONTEXT.md asks for defensive handling of misconfigured min/max), but should be called out so the plan's verification step checks both zoom levels against the same bounds.

### Pitfall 3: File encoding corruption on edit

**What goes wrong:** `CameraController.cs` appears to be saved in a non-UTF-8 encoding (Korean comments render as mojibake when read via UTF-8-assuming tools). A naive full-file rewrite using a UTF-8-assuming write tool could silently corrupt the existing Korean comments into garbage bytes that no longer display correctly in Unity/Visual Studio, even though the code still compiles (comments don't affect compilation).

**Why it happens:** Older Korean-locale Windows/Unity/MonoDevelop setups commonly default to CP949/EUC-KR for new C# files instead of UTF-8; mixing an EUC-KR file with UTF-8-only tooling causes round-trip corruption.

**How to avoid:** Prefer a targeted/surgical text edit tool that only touches the specific lines being changed/added (preserving byte-for-byte encoding on untouched lines) rather than a full-file read-modify-write-as-UTF8 cycle. If the executor's tooling can only write UTF-8, this is an acceptable and likely unavoidable tradeoff (functionally harmless — only comment *display* is affected, not code behavior) but should be flagged in the plan so it's a conscious decision, not a silent side effect discovered later.

**Warning signs:** After editing, existing Korean comments in the diff show as different mojibake patterns than before, or the file's line count/comment content shifts unexpectedly.

## Code Examples

### Existing position-follow Lerp (verified, to be preserved as-is)
```csharp
// Source: Assets/Camera/Script/CameraController.cs (current file, unmodified)
void LateUpdate()
{
    if (target == null) return;
    Vector3 targetCamPos = target.position + offset;
    transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
}
```

### Existing singleton shape to replicate (verified)
```csharp
// Source: Assets/Player/Script/GameStateManager.cs (existing project code)
public static GameStateManager Instance { get; private set; }

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        // (GameStateManager also does DontDestroyOnLoad here — CameraController should NOT copy this part, see Pattern 1)
    }
    else
    {
        Destroy(gameObject);
    }
}
```

### Existing trigger-pair-with-revert-on-exit shape to replicate (verified)
```csharp
// Source: Assets/map/script/portal.cs (existing project code)
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player")) isPlayerNearby = true;
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player")) isPlayerNearby = false;
}
```

## State of the Art

Not applicable in the traditional sense (no library versioning drift here — this is built-in `UnityEngine` API that has been stable across Unity versions for this exact use case for many years). One relevant note:

| Old Approach | Current Approach | When Changed | Impact |
|--------------|-------------------|---------------|--------|
| `Object.FindObjectOfType<T>()` | `Object.FindFirstObjectByType<T>()` / `FindAnyObjectByType<T>()` | Unity 2023.1+ deprecated the old overload-less `FindObjectOfType` (still works, but flagged obsolete in newer Unity versions) | Not directly relevant to this phase since the recommended approach is a singleton, not `FindObjectOfType` — but if the planner ever considers `FindObjectOfType` as an alternative reference strategy, note that this project's Unity version (6000.3.10f1, i.e. Unity 6) will show an obsolete-API warning for the parameterless `FindObjectOfType<T>()` call. This reinforces the singleton recommendation (Pattern 1) as also being the more forward-compatible choice, not just the stylistically consistent one.

## Open Questions

1. **Is `CameraController.cs` guaranteed to be attached directly to the Camera GameObject (for `GetComponent<Camera>()` to succeed)?**
   - What we know: The script directly sets `transform.position` in `Start()`/`LateUpdate()` with no `GetComponent` calls currently, and the class name/behavior strongly implies it's the camera-rig script sitting on the actual Camera object (or a parent rig object).
   - What's unclear: Scene file inspection (`.unity` YAML) wasn't performed to 100% confirm the GameObject hierarchy — it's possible `CameraController` sits on a parent "CameraRig" empty with the actual `Camera` component on a child object, in which case `GetComponent<Camera>()` would return null and `GetComponentInChildren<Camera>()` would be needed instead.
   - Recommendation: The plan/executor should verify this at implementation time by checking the actual GameObject in one scene's Inspector (e.g., `1 stage.unity` or `Tutorial Map.unity`) before finalizing whether to use `GetComponent<Camera>()` or `GetComponentInChildren<Camera>()`. This is a 30-second Unity Editor check, flagged here so it isn't missed. Given the `offset = (0,0,-10)` field is described as the camera's own Z-depth-from-target offset (per the file's own comment), the single-object model is far more likely, but should still be confirmed.

2. **Should `targetZoom`/`SetBossZoom` state survive re-entering the same trigger while already in boss zoom, or overlapping triggers from two different bosses?**
   - What we know: D-01/D-02/D-03 describe a single trigger's enter/exit behavior. Nothing in CONTEXT.md addresses what happens if two boss trigger zones overlap (e.g., player standing in the overlap of two boss areas) or if `OnTriggerEnter2D` fires again while already zoomed in.
   - What's unclear: Whether `SetBossZoom(true)` should be idempotent (safe to call repeatedly) or whether overlapping zones need reference-counting (don't revert to normal until *all* overlapping boss triggers have been exited).
   - Recommendation: Given D-08 defers actual scene placement to the user and no multi-boss-overlap scenario is described as a requirement, treat `SetBossZoom(bool)` as a simple idempotent boolean setter (last-call-wins) — this is the minimal implementation consistent with CLAUDE.md's anti-over-engineering principle. If the user later places overlapping triggers and observes a bug (premature revert-to-normal when exiting only one of two overlapping zones), that would be a new, separate bug report/phase, not something to defensively engineer against now.

## Sources

### Primary (HIGH confidence)
- `Assets/Camera/Script/CameraController.cs` — full file read, current implementation
- `Assets/map/script/portal.cs` — full file read, trigger pattern precedent
- `Assets/map/script/GameManager.cs` — full file read, singleton pattern precedent
- `Assets/Script/AudioManager.cs` — singleton declaration/Awake read
- `Assets/Player/Script/GameStateManager.cs` — singleton declaration/Awake read
- `Assets/Player/Script/PlayerStats.cs` — singleton declaration confirmed via grep
- `Assets/Enemy/WaterMonster/Script/Phase4/SpeedUpZone.cs` — full file read, trigger + cross-reference pattern precedent
- `Assets/map/script/FallZone.cs` — full file read, `CompareTag` trigger pattern precedent
- `Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs` — `mapBounds` `BoxCollider2D` pattern (confirmed as the pattern D-10 explicitly rejects)
- `ProjectSettings/ProjectVersion.txt` — confirmed Unity 6000.3.10f1 (Unity 6)
- `.planning/phases/09-camera-zoom-stage-transition/09-CONTEXT.md` — locked decisions D-01 through D-11
- `.planning/config.json` — confirmed `nyquist_validation: false` (Validation Architecture section correctly omitted from this document)
- Codebase-wide grep for `Camera.main`, `FindObjectOfType<Camera`, `static.*Instance`, `RequireComponent`, `OnTriggerEnter2D`/`OnTriggerExit2D` — used to establish conventions by exhaustive precedent rather than isolated example

### Secondary (MEDIUM confidence)
- None used — all findings in this document are grounded directly in this project's own source files or well-established, stable `UnityEngine` API behavior (orthographic camera geometry, `Mathf.Clamp`/`Mathf.Lerp` semantics) that does not require external verification.

### Tertiary (LOW confidence)
- None.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no external packages involved, pure built-in `UnityEngine` API confirmed against the actual installed Unity version.
- Architecture: HIGH — every recommended pattern (singleton, trigger-pair, LateUpdate ordering) is directly evidenced by multiple existing files in this exact codebase, not inferred from generic Unity best practice.
- Pitfalls: HIGH for encoding/math pitfalls (directly observed/derivable), MEDIUM for the min/max-narrower-than-view edge case (logically sound but not something reproducible without the actual scene's map dimensions, which are out of this phase's scope per D-08).

**Research date:** 2026-07-30
**Valid until:** Stable — this research is grounded in project-internal conventions and basic Unity orthographic camera geometry that does not drift with Unity version updates. No expiry pressure; re-research only if `CameraController.cs` is restructured by an unrelated future phase before this one executes.
