# Phase 12: 피격 시 카메라 흔들림 (Camera Shake on Hit) - Research

**Researched:** 2026-08-11
**Domain:** Unity 6000.3.10f1, C#, single-file MonoBehaviour camera pipeline extension
**Confidence:** HIGH

## Summary

This phase adds a small, self-contained "hit shake" layer to `CameraController.cs`. All the hard
design questions are already locked in `12-CONTEXT.md` (trigger point, magnitude model, decay
style, refresh-not-stack semantics, boss-zone always-on, apply-after-clamp ordering, two Inspector
fields only). The remaining work is almost entirely mechanical: two new Inspector fields, one
public `Shake()` method, one private per-frame decay/apply helper, one call site in
`PlayerStats.TakeDamage`, and one call site at the very end of `CameraController.LateUpdate()`
(after the existing re-anchor block, unconditionally — i.e. outside the `if (!_isBossZone)` guard
so it also runs in boss zones per D-07).

The one open technical risk flagged in the phase brief — CP949 byte corruption risk when editing
`CameraController.cs` — was investigated directly and downgraded. The file's few remaining
non-ASCII bytes (5 lines, all far from the insertion points) are **already permanently converted
to literal U+FFFD replacement-character UTF-8 sequences** by an earlier, pre-byte-safe-protocol
edit (visible back to at least Phase 9/10 commits). The file is presently valid, stable UTF-8. This
means the CP949-misdecoding failure mode that motivated the `git show HEAD` + pure-byte-script
protocol in Phase 11 is **not currently live** for this specific file — standard Read/Edit is safe
here, though new code should still stay ASCII-only as a defensive habit matching the rest of the
file's post-corruption comments.

**Primary recommendation:** Add `[Header("Hit Shake")] public float shakeMagnitude` /
`shakeDuration` near the other tunable Inspector groups, a `private float _shakeTimer` state field,
`public void Shake() => _shakeTimer = shakeDuration;`, a `private void ApplyHitShake()` helper using
linear decay (`Random.insideUnitCircle * shakeMagnitude * (_shakeTimer / shakeDuration)`,
`Time.deltaTime`-based countdown), called as the last line of `LateUpdate()` unconditionally. Wire
`PlayerStats.TakeDamage` to call `CameraController.Instance.Shake()` immediately after
`base.TakeDamage(dmg)`, with no null guard (matches existing codebase convention).

<phase_requirements>
## Phase Requirements

No formal REQ-IDs are mapped to this phase (small scoped enhancement, not part of the v2.0
milestone REQUIREMENTS.md traceability table). Scope is fully defined by `12-CONTEXT.md` decisions
D-01 through D-09. This research supports implementing those decisions directly; see Code Examples
and Architecture Patterns below for the D-ID-to-code mapping.
</phase_requirements>

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**트리거 조건**
- D-01: 흔들림은 플레이어가 피격당할 때만 발동한다. 보스 등 다른 엔티티가 맞을 때는 흔들리지 않는다 (범위 밖).
- D-02: 연결 지점은 `PlayerStats.TakeDamage(float dmg)` 오버라이드 (`Assets/Player/Script/PlayerStats.cs:54`)다. `base.TakeDamage(dmg)` 호출 뒤 `CameraController.Instance.Shake()`를 호출한다. `HP.cs`는 무수정이며 공용 `OnHit` 이벤트도 신설하지 않는다.
- D-03: 사망(`Die()`)으로 이어지는 마지막 피격에도 흔들림이 함께 발동한다 — `TakeDamage` 내부에서 호출하므로 별도 분기 없이 자연스럽게 포함된다.

**흔들림 강도/결**
- D-04: 강도는 고정값이다. 데미지량에 비례하지 않는다.
- D-05: 움직임 방식은 랜덤 오프셋(예: `Random.insideUnitCircle * 현재강도`)이며, 시간에 따라 감쇠(decay)한다. 규칙적인 진동(Sine wave)은 사용하지 않는다.
- D-06: 연속 피격으로 이전 흔들림이 아직 감쇠 중일 때 새 타격이 들어오면, 지속시간 타이머를 최대치로 리프레시한다 (가산/누적 방식 아님).

**카메라 파이프라인과의 합성**
- D-07: 보스 구역(`_isBossZone` true, 줌 확대 상태)에서도 흔들림은 항상 적용된다. Phase 9의 D-15는 흔들림에는 적용되지 않는다.
- D-08: 흔들림 오프셋은 `ApplyBoundsClamp()`(X/Y 경계 클램프) 이후에 최종 적용되며, 클램프를 약간 뚫고 나가는 것을 허용한다. 흔들림 적용 후 다시 클램프하지 않는다.

**튜닝 파라미터 노출 범위**
- D-09: Inspector 노출 필드는 `shakeMagnitude`(강도, 월드 유닛)와 `shakeDuration`(지속시간, 초) 2개만이다. 감쇠 곡선을 `AnimationCurve`로 노출하는 것은 범위 밖 — 기존 `zoomSmoothing`/`deadzoneWidth`류의 단순 float 필드 패턴을 그대로 따른다.

### Claude's Discretion
- `Shake()` 메서드 시그니처 — 매개변수 없는 오버로드로 충분 (`public void Shake()`).
- 감쇠 곡선의 정확한 수식 (선형 vs 지수) — 코드 내부에서 결정.
- `Random.insideUnitCircle` vs Perlin noise 중 선택.
- **LateUpdate 내 정확한 삽입 위치 — 반드시 지켜야 할 기술 제약**: 흔들림 오프셋은 데드존 재앵커 블록 이후에 더해져야 한다 — 재앵커보다 먼저 섞이면 흔들림 값 자체가 매 프레임 `_deadzoneCenterX/Y`에 누적되어 카메라 추적 로직이 오염되는 회귀가 발생한다.
- 신규 필드/내부 상태 변수의 정확한 이름.

### Deferred Ideas (OUT OF SCOPE)
- 보스(또는 다른 엔티티) 피격 시에도 카메라 흔들림 — 이번 Phase는 플레이어 피격만 다룬다(D-01). 필요해지면 `HP.cs`에 공용 `OnHit` 이벤트를 추가하는 방식으로 확장 가능하나, 이번 Phase에서는 구현하지 않는다.
- 감쇠 곡선을 `AnimationCurve`로 Inspector 노출 — 범위 밖(D-09), 필요시 후속 Phase에서 논의.
</user_constraints>

## Project Constraints (from CLAUDE.md)

- **Think before coding**: plan is already locked via CONTEXT.md; do not re-litigate D-01~D-09.
- **Phase isolation / minimal scope**: touch only what's needed for shake — do not refactor
  adjacent deadzone/offset/peek/clamp logic even if imperfect.
- **Surgical changes**: only lines traceable to this phase's plan may change in
  `CameraController.cs` and `PlayerStats.cs`. Do not "clean up" unrelated existing comments
  (including the pre-existing mojibake — leave it as-is, it predates this phase and touching it is
  out of scope).
- **Respect existing style**: mirror the existing `[Header(...)]` + public float Inspector field
  pattern (`zoomSmoothing`, `deadzoneWidth`, etc.) exactly for `shakeMagnitude`/`shakeDuration`.
- **Verifiable goals**: define concrete before/after checks (field count, method presence, call
  site presence, line-count deltas) the same way Phase 9-11 plans did (see Common Pitfalls below
  for the literal-string-in-verification-gate trap those phases hit repeatedly).
- **YOLO mode**: `.planning/config.json` does not set `"mode": "yolo"` — no special fast-path
  behavior implied here; normal confirmation flow applies unless a later config/user directive
  says otherwise.

## Standard Stack

Not applicable in the traditional library sense — this is a pure Unity `MonoBehaviour` extension
using only built-in `UnityEngine` APIs already used elsewhere in this file.

| API | Purpose | Already used in this file? |
|-----|---------|------------------------------|
| `Random.insideUnitCircle` | Random 2D offset direction/magnitude for shake (D-05) | No (new) |
| `Time.deltaTime` | Frame-scaled countdown, matches every other timer in this file | Yes (`smoothing`, `zoomSmoothing`, `SmoothDamp` calls, `offsetHoldTimer`, `_peekTimer`) |
| `Mathf.Clamp01` / `Mathf.Max` | Safe division guard for the decay fraction | Yes (`Mathf.Max(Time.deltaTime, 0.0001f)` at line 260 is the exact precedent to mirror) |

No package installs needed. No version concerns — this is core `UnityEngine`, stable across all
6000.x releases.

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `Random.insideUnitCircle` | `Mathf.PerlinNoise` | Smoother, less jittery shake, but D-05 explicitly says "규칙적인 진동(Sine wave)은 사용하지 않는다" and Perlin is deterministic/wave-like in character; `insideUnitCircle` is the more literal reading of D-05's own example and is Claude's-discretion-approved without further justification needed |
| Linear decay (`remaining/duration`) | Exponential decay (`Mathf.Exp(-k*t)`) or `AnimationCurve` | Linear is simplest, matches D-09's explicit "no AnimationCurve this phase," and mirrors the file's existing preference for simple float math over curves |

## Architecture Patterns

### Recommended structure (all changes inside `Assets/Camera/Script/CameraController.cs` + one line in `Assets/Player/Script/PlayerStats.cs`)

No new files needed. This phase is a pure extension of two existing classes.

### Pattern 1: Inspector field group (mirrors existing `[Header(...)]` groups)
**What:** Two new public float fields in their own header block, placed after the existing
`[Header("Peeking (normal stages only)")]` block (ends at line 67) and before the
`Instance` singleton declaration (line 69-71).
**When to use:** Any new tunable parameter in this file, per D-09's explicit instruction to reuse
the `zoomSmoothing`/`deadzoneWidth` pattern.
**Example:**
```csharp
// Source: existing CameraController.cs pattern (lines 12-18, 36-42)
[Header("Hit Shake")]
// Peak random offset magnitude in world units on a fresh hit (D-04, D-05, D-09).
public float shakeMagnitude = 0.3f;
// Seconds the shake takes to decay from full magnitude to zero; refreshed to this
// value (not added to) on every new hit while still decaying (D-06, D-09).
public float shakeDuration = 0.25f;
```

### Pattern 2: Public trigger method + private per-frame decay/apply (mirrors `SetZoomZone`/`SetXBounds` + `UpdateDynamicOffset`/`UpdatePeekOffset` split)
**What:** A thin public setter-style method (`Shake()`) that only arms a timer, and a private
`LateUpdate`-driven helper that does the actual per-frame math and writes to `transform.position`.
This mirrors the existing `SetZoomZone`/`SetXBounds` (public, idempotent, last-call-wins) vs.
`UpdateDynamicOffset`/`UpdatePeekOffset` (private, called once per `LateUpdate`, own internal state)
split already used throughout the file.
**When to use:** Exactly this phase's `Shake()` trigger + decay pair.
**Example:**
```csharp
// Public trigger - called externally (PlayerStats.TakeDamage). Mirrors SetZoomZone's
// "idempotent, last call wins" contract (D-06: refresh to max, not additive).
public void Shake()
{
    _shakeTimer = shakeDuration;
}

// Applied as the LAST step of LateUpdate, after ApplyBoundsClamp() and the deadzone
// re-anchor block, and UNCONDITIONALLY (not inside `if (!_isBossZone)`) so it also
// fires in boss zones per D-07. Not re-clamped afterward per D-08.
private void ApplyHitShake()
{
    if (_shakeTimer <= 0f) return;
    _shakeTimer -= Time.deltaTime;
    float t = Mathf.Clamp01(_shakeTimer / Mathf.Max(shakeDuration, 0.0001f));
    Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * t;
    Vector3 pos = transform.position;
    pos.x += shakeOffset.x;
    pos.y += shakeOffset.y;
    transform.position = pos;
}
```

### Pattern 3: LateUpdate call site placement
**What:** `ApplyHitShake();` must be the last statement in `LateUpdate()`, after the existing
re-anchor `if (!_isBossZone) { ... }` block, and outside of it.
**Why this order matters:** The re-anchor block (lines 423-427) reads `transform.position` back
into `_deadzoneCenterX`/`_deadzoneCenterY` for next-frame deadzone math. If shake were applied
before that block, the shake jitter would bleed into the deadzone anchor and accumulate frame over
frame, corrupting camera tracking — this is explicitly called out in CONTEXT.md as "the same class
of bug as D-08's after-clamp reasoning." Placing `ApplyHitShake()` after (not inside) the
`if (!_isBossZone)` block also automatically satisfies D-07 (shake fires in boss zones too),
because it is no longer gated by that conditional at all.
**Example (final LateUpdate shape):**
```csharp
void LateUpdate()
{
    if (target == null) return;
    Vector3 targetCamPos = target.position + offset;
    transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    if (_isBossZone) ResetNormalStageState(); else ApplyNormalStageCamera();
    _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, zoomSmoothing * Time.deltaTime);
    ApplyBoundsClamp();
    if (!_isBossZone)
    {
        _deadzoneCenterX = transform.position.x + _currentBoxOffsetX;
        _deadzoneCenterY = transform.position.y - _currentPeekY;
    }
    ApplyHitShake(); // NEW - last layer, unconditional (D-07), after clamp/re-anchor (D-08)
}
```

### Anti-Patterns to Avoid
- **Adding shake reset into `ResetNormalStageState()`:** This method runs every frame while
  `_isBossZone` is true (line 412: `if (_isBossZone) ResetNormalStageState();`). If `_shakeTimer`
  reset were added there, shake would be silently killed every frame in boss zones, directly
  violating D-07 ("shake ALWAYS applies even in boss zones"). Do not touch this method at all.
- **Re-clamping after shake:** D-08 explicitly allows the shake offset to poke outside
  `minX/maxX/minY/maxY`. Do not call `ApplyBoundsClamp()` again after `ApplyHitShake()`.
- **Damage-proportional magnitude:** D-04 locks this to a fixed magnitude. Do not thread `dmg`
  through to `Shake()` even though `TakeDamage(float dmg)` has the value available.
- **Sine-wave/periodic shake:** D-05 explicitly excludes this. Do not use `Mathf.Sin(Time.time * freq)`.
- **Additive/stacking shake on repeated hits:** D-06 requires refresh-to-max, not summing
  magnitudes or duration. `Shake()` must simply reassign `_shakeTimer = shakeDuration`, never
  `+=`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Random 2D offset | Custom RNG / two `Random.Range` calls composed into a vector | `Random.insideUnitCircle` | Built-in, uniform distribution over a disc, exactly matches the `Random.insideUnitCircle * 현재강도` example already given in D-05 |
| Decay curve | Custom `AnimationCurve` asset wiring | Plain `float` division (`remaining/duration`) | D-09 explicitly excludes `AnimationCurve` exposure this phase; a hand-rolled curve field would violate the 2-fields-only constraint |
| Singleton null-safety wrapper | New helper/extension method for safe `CameraController.Instance` access | Direct `CameraController.Instance.Shake()` call, no guard | Matches the established, unguarded convention already used by `CameraZoomTrigger` and `CameraBoundsTrigger` (see Common Pitfalls for the associated risk note) |

**Key insight:** This phase has zero real "hand-roll risk" — everything needed is either a plain
float field or a one-line call to a built-in Unity API. The main risk in this domain is
over-building (adding curve exposure, event buses, or generalized "shake systems" that exceed
D-09's explicit two-field scope).

## Runtime State Inventory

Not applicable — this is a greenfield feature addition (new fields/methods), not a rename, refactor,
or migration. No stored data, live service config, OS-registered state, secrets, or build artifacts
are affected.

## Common Pitfalls

### Pitfall 1: Assuming CP949 byte corruption risk applies uniformly to the whole file
**What goes wrong:** Blindly applying the "extract bytes via `git show HEAD:<path>` + pure-byte
insertion script" protocol from Phase 11 to every edit in `CameraController.cs`, when it is no
longer necessary for this specific file.
**Why it happens:** Phase 9-11 established (correctly, at the time) that Read/Edit round-trips on
CP949 files silently mangle non-ASCII bytes to U+FFFD. That mangling already happened to this
file's remaining 5 comment lines in an earlier phase and is now baked into git history as literal,
valid UTF-8 U+FFFD sequences (confirmed via `git show HEAD:Assets/Camera/Script/CameraController.cs
| xxd` showing `ef bf bd` triplets, and cross-checked against the file's first commit `e7126b6`
which had genuine two-byte CP949 sequences like `c5 b8 b0 d9` at the same location — proving the
corruption happened somewhere in between, not at authoring time).
**How to avoid:** Standard Read/Edit tool round-trips are safe for this file now (verified: the
file decodes as valid UTF-8 today, `grep -nP "[^\x00-\x7F]"` finds only 5 lines, all already
U+FFFD, none near the insertion points at lines ~12-71 or ~199-428). Still write all NEW code/comments
in this file as pure ASCII (matching every phase-9/10 addition already in the file) as a zero-cost
defensive habit — do not introduce fresh Korean text that could be a future corruption target if the
file's true encoding is ever reconciled.
**Warning signs:** If a future `grep -nP "[^\x00-\x7F]"` on this file returns MORE than 5 lines
after this phase's edits, something went wrong (either new non-ASCII was introduced, or a tool
round-trip touched previously-clean bytes).

### Pitfall 2: Verification-gate literal-string collisions
**What goes wrong:** Phase 9, 10, and 11 all independently hit the same failure: a plan's own
insertion-comment text happened to contain the literal string its own verification gate was
grepping for, making the gate fail (or falsely pass) regardless of correctness (e.g.
"DontDestroyOnLoad" in Phase 9 Plan 1, "deadzoneHeight" in Phase 10 Plan 1).
**Why it happens:** Verification gates in this project are frequently literal substring/line
counts (`git diff` line counts, `grep -c "some literal"`), and phase-relevant vocabulary
(`shakeMagnitude`, `shakeDuration`, `_isBossZone`, `ApplyBoundsClamp`) is exactly the kind of term
a natural-language planning comment would also use.
**How to avoid:** When the planner writes verification gates that count occurrences of
`shakeMagnitude`/`shakeDuration`/`_isBossZone`/etc., phrase insertion-comment prose to avoid
repeating the exact gated term where the count matters, or use `git diff` against the correct
baseline commit (see Pitfall 3) instead of raw literal counts.

### Pitfall 3: Wrong baseline commit for `git diff` verification
**What goes wrong:** Phase 10 Plan 2/3 both discovered that `git diff <old-baseline>` returned 0
lines changed when the plan itself expected 2-3, because the chosen baseline commit was not the
immediately-preceding commit — work from an intervening plan had already changed the file, making
the diff against a stale baseline nonsensical.
**How to avoid:** Any verification step using `git diff <commit>` should diff against `HEAD`
(i.e., the immediately preceding commit within this phase's plan sequence), not a fixed
phase-start commit, especially since this phase will likely be split into 1-2 plans touching the
same file sequentially.

### Pitfall 4: `_shakeTimer <= 0f` early-return still needs decay to reach exactly/near 0
**What goes wrong:** If the early-return guard is `if (_shakeTimer <= 0f) return;` but the
countdown line `_shakeTimer -= Time.deltaTime;` runs before the guard, or the guard uses `< 0f`
instead of `<= 0f`, shake can either never fully stop (looping tiny negative-fraction jitter) or
apply one extra zero-magnitude frame. The example in Pattern 2 above orders guard-then-decrement
correctly; keep that order.
**Warning signs:** Camera "buzzing" faintly forever after a hit, or `Mathf.Clamp01` silently
masking an out-of-range fraction that should have been treated as "shake over."

### Pitfall 5: Division by zero if `shakeDuration` is set to 0 in the Inspector
**What goes wrong:** `_shakeTimer / shakeDuration` divides by zero if a designer sets
`shakeDuration = 0` while tuning in the Inspector, producing `NaN` that propagates into
`transform.position` and can freeze/break the camera for the rest of the session.
**How to avoid:** Guard with `Mathf.Max(shakeDuration, 0.0001f)` in the denominator, exactly
mirroring the existing `float dt = Mathf.Max(Time.deltaTime, 0.0001f);` pattern already present at
line 260 in this same file (`ApplyNormalStageCamera`).

## Code Examples

### Field/state additions (private state block, near line 126, alongside `_peekTimer`)
```csharp
// Source: pattern mirrors existing private state fields in this file (lines 89-126)
// Counts down from shakeDuration to 0 after a hit (D-06: Shake() resets this to max,
// never adds to it, so repeated hits refresh rather than stack).
private float _shakeTimer;
```

### `PlayerStats.TakeDamage` call site (exact current code, `Assets/Player/Script/PlayerStats.cs:54-59`)
```csharp
// Current (unmodified):
public override void TakeDamage(float dmg)
{

    base.TakeDamage(dmg); // 부모 클래스(HP)의 원래 데미지 처리 로직 호출
    Debug.Log("Player has taken damage!");
}

// After this phase's change (D-02: call after base.TakeDamage, no null guard needed
// per established codebase convention - see Pitfall/Open Question below):
public override void TakeDamage(float dmg)
{

    base.TakeDamage(dmg); // 부모 클래스(HP)의 원래 데미지 처리 로직 호출
    CameraController.Instance.Shake();
    Debug.Log("Player has taken damage!");
}
```
Note: `PlayerStats.cs` is genuinely UTF-8 encoded (BOM present, Korean comments render correctly —
confirmed via `grep -nP "[^\x00-\x7F]"` showing readable Korean, not mojibake). No byte-safety
concern for this file at all; standard Edit tool is fully safe here.

## State of the Art

Not applicable — no ecosystem/library versioning questions in scope. This is pure hand-written
`MonoBehaviour` code consistent with the rest of the project (no Cinemachine, confirmed absent from
this project per Phase 9 research: "Cinemachine 미사용 — 순수 UnityEngine.Camera + 수동 스크립트
구조").

## Open Questions

1. **Should `CameraController.Instance.Shake()` be null-guarded in `PlayerStats.TakeDamage`?**
   - What we know: Every existing external caller of `CameraController.Instance`
     (`CameraZoomTrigger.cs` lines 20/27, `CameraBoundsTrigger.cs` lines 44/45/67/68) calls it
     directly with zero null checks. `CameraController` is a scene-local singleton (no
     `DontDestroyOnLoad`), so `Instance` could theoretically be null very briefly during a scene
     transition if the player were to take damage in that exact window.
   - What's unclear: Whether that window is reachable in practice (player likely can't take damage
     while a new scene's `Awake()` hasn't run yet, since gameplay is usually paused/loading then).
   - Recommendation: Follow the established codebase convention (no guard) for consistency, since
     CONTEXT.md's discretion section doesn't ask for defensive null-checking and every sibling
     trigger script already accepts this same risk profile without incident across 4 prior phases.
     If a plan author wants extra safety, `CameraController.Instance?.Shake();` is a zero-risk,
     one-character addition that would still satisfy all locked decisions — either choice is
     planning-safe.

2. **Exact default values for `shakeMagnitude` / `shakeDuration`.**
   - What we know: D-09 requires these as Inspector-tunable floats with no other constraint on
     defaults; they are explicitly "initial values to be playtested" in the same spirit as
     `normalZoom=5`/`bossZoom=7` in Phase 9 and `peekThreshold=0.5`/`peekDistance=3` in Phase 10.
   - What's unclear: No numeric guidance was given by the user in CONTEXT.md or the discussion log.
   - Recommendation: `shakeMagnitude = 0.3f` (world units — small relative to `deadzoneWidth=3f`/
     `deadzoneHeight=2f` so it reads as a jolt, not a full re-frame) and `shakeDuration = 0.25f`
     (seconds — short enough to read as an impact, not a sustained wobble) are reasonable
     playtest-starting defaults consistent with this file's existing "ship a sane default, let
     designer tune in Inspector" pattern. Not a locked decision either way.

## Environment Availability

Skipped — no external dependencies. This phase only uses built-in `UnityEngine` APIs already
present in the project (Unity 6000.3.10f1, confirmed via `ProjectSettings/ProjectVersion.txt`). No
new packages, no CLI tools, no external services.

## Sources

### Primary (HIGH confidence)
- Direct file read: `Assets/Camera/Script/CameraController.cs` (current working tree, 429 lines) — exact line numbers, field names, `[Header]` groups, `LateUpdate` structure cited throughout this document.
- Direct file read: `Assets/Player/Script/PlayerStats.cs` (current working tree, 74 lines) — `TakeDamage` override at lines 54-59, confirmed UTF-8/BOM encoding.
- Direct file read: `Assets/Script/HP.cs` (current working tree, 108 lines) — confirmed `TakeDamage`/`Die` base implementation, confirmed CP949-origin mojibake present but irrelevant (read-only per D-02).
- `git show e7126b6:Assets/Camera/Script/CameraController.cs | xxd` — proved the file's first commit had genuine two-byte CP949 sequences at the exact location that is now U+FFFD, establishing that corruption happened mid-history, not at authoring time.
- `grep -nP "[^\x00-\x7F]"` runs against working-tree `CameraController.cs` and `PlayerStats.cs` — established current non-ASCII byte locations and encoding validity for both files.
- `Assets/Player/Script/GameStateManager.cs`, `Assets/Player/Script/PlayerAttack.cs` — confirmed `Time.timeScale` usage patterns (pause menu, attack hitstop) exist elsewhere in the project, supporting the recommendation to use `Time.deltaTime` (not `unscaledDeltaTime`) for shake decay so it freezes consistently with the rest of the camera pipeline during a pause.
- `Assets/Camera/Script/CameraZoomTrigger.cs`, `Assets/Camera/Script/CameraBoundsTrigger.cs` — confirmed the established no-null-guard convention for `CameraController.Instance` calls.
- `ProjectSettings/ProjectVersion.txt` — confirmed Unity 6000.3.10f1.
- `.planning/config.json` — confirmed `workflow.nyquist_validation: false`.

### Secondary (MEDIUM confidence)
None used — all findings verified directly against the actual repository files rather than
external/web sources, since this phase has no library or ecosystem dependency.

### Tertiary (LOW confidence)
None.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — pure built-in `UnityEngine` API usage, directly verified against existing file conventions, no external dependency.
- Architecture: HIGH — every insertion point, ordering constraint, and naming pattern is either explicitly locked in CONTEXT.md or directly derived from reading the actual current source files.
- Pitfalls: HIGH — all five pitfalls are either directly reproduced from this project's own documented history (STATE.md Key Decisions log for Phases 9-11) or verified first-hand via byte-level inspection in this research session.

**Research date:** 2026-08-11
**Valid until:** Until `CameraController.cs` or `PlayerStats.cs` next change materially (stable domain, no external version drift risk) — practically, valid through this phase's execution.
