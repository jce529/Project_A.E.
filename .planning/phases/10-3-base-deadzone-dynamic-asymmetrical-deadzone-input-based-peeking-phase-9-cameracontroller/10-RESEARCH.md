# Phase 10: 카메라 데드존 3종 기법 - Research

**Researched:** 2026-08-04
**Domain:** Unity C# 2D camera systems (deadzone box / dynamic offset / input-based peeking) layered onto an existing hand-rolled `CameraController` singleton
**Confidence:** HIGH (architecture/integration), MEDIUM (default tuning values, Y-axis scope — see Open Questions)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**데드존 박스 정의**
- **D-01:** 박스 크기는 월드 유닛 고정 Inspector float. 가로/세로 별도 필드(`deadzoneWidth`, `deadzoneHeight`) — 메트로배니아 특성상 좌우/상하 이동 폭이 다르므로 분리.
- **D-02:** 보스 줌(7) 전환 중에도 데드존 박스는 고정 월드 크기를 유지한다. 줌 비율에 따라 스케일하지 않는다 (다만 D-15에 따라 보스 구역에서는 애초에 데드존 자체가 꺼진다).
- **D-03:** `OnDrawGizmos`로 박스 윤곽을 표시한다 (에디터 전용, 런타임 성능 영향 없음). Play 모드 튜닝 시 Scene 뷰에서 바로 확인 가능해야 한다.

**이동방향 감지 (동적 비대칭 오프셋)**
- **D-04:** `PlayerController.cs`는 수정하지 않는다. 카메라가 `target.position`의 프레임 간 델타로 이동방향을 추정한다 (moveInput/rigid는 private이라 직접 접근 불가, Phase 9의 "카메라 스크립트만 수정" 원칙 유지).
- **D-05:** 오프셋 발동 기준은 별도 속도 임계값이 아니라 **데드존 경계를 실제로 밀고 있을 때만**이다 (D-01 박스와 연동).
- **D-06:** 정지 후에도 오프셋은 즉시 줄어들지 않고, 일정 시간 유지된 뒤 서서히 복귀한다 (유지시간 파라미터 별도 필요, 플레이테스트로 조정할 초기값).
- **D-07:** 오프셋 자체 전환은 `SmoothDamp`로 부드럽게 처리한다 (D-14의 데드존 하드컷과 대비되는 레이어).

**피킹 입력 및 취소 조건**
- **D-08:** 수직 입력은 카메라가 `InputHandler.Instance.OnMoveEvent`를 **직접 구독**해서 읽는다 (`PlayerController.cs` 미수정). `InputHandler`는 원래 "외부 스크립트가 구독하는 이벤트 버스"로 설계되어 있어 이 경로가 정석.
- **D-09:** 입력 잠금 가드 — `target.GetComponent<PlayerController>().movementLocked`(이미 public)를 확인해, 잠금 중(컷신/사망/구속 등)에는 `OnMoveEvent`가 흘려보내는 원시 입력을 무시한다. `OnMoveEvent`는 `movementLocked` 여부와 무관하게 항상 발화하므로 이 가드가 필수. `InputHandler`는 `DontDestroyOnLoad`, `CameraController`는 씬 로컬(파괴됨)이므로 `OnEnable`에서 구독, `OnDisable`에서 반드시 해제해 구독 누수를 막는다.
- **D-10:** 정지 판단(`Velocity==0`)은 `target.position` 프레임 델타가 거의 0인지로 근사한다 (D-04와 동일 방식 재사용, `PlayerController.cs` 미수정).
- **D-11:** 피킹 즉시 취소 조건(대시/피격 등)도 원인을 구분하지 않고 **이동량 급증**만으로 프록시 감지한다 (`isDashing`/`isKnockedBack`은 private, 새 접근자 추가하지 않음).
- **D-12:** `IsGrounded()` 조건을 원안 그대로 포함한다 (이미 public 메서드, 그대로 재사용).
- **D-13:** `Threshold`(0.5초)와 `PeekDistance`는 사용자 제시값을 초기값으로 Inspector에 노출한다 (Phase 9의 `normalZoom`/`bossZoom` 패턴과 동일하게 플레이테스트 튜닝 대상).

**기존 파이프라인 통합**
- **D-14:** 일반 스테이지에서는 데드존을 **하드컷**으로 즉시 계산한다 (Lerp 없이, 카메라가 데드존이 허용하는 경계 위치로 즉시 스냅). 그 위에 동적 비대칭 오프셋(SmoothDamp)과 피킹 오프셋(SmoothDamp)만 부드럽게 가산한다.
- **D-15:** 보스 구역 진입 중(`SetBossZoom(true)` 활성 상태)에는 데드존/동적오프셋/피킹을 **전부 비활성화**하고, 기존 Phase 9의 `Vector3.Lerp(transform.position, target.position + offset, smoothing)` 레거시 로직으로 완전히 복귀한다.
- **D-16:** 줌 Lerp(`normalZoom`/`bossZoom`/`zoomSmoothing`)는 두 경로(일반/보스) 모두에서 기존 그대로 동작한다 — 변경 없음.
- **D-17:** X축 클램프(`minX`/`maxX`, `ApplyXClamp()`)는 데드존+오프셋+피킹이 모두 적용된 최종 위치(또는 보스 구역 레거시 위치)에 대해 **마지막으로** 적용된다.

### Claude's Discretion
- 새 필드/메서드의 정확한 이름, `LateUpdate` 내부를 헬퍼 메서드로 어떻게 분리할지
- `SmoothDamp` velocity 임시 변수 관리 방식
- Gizmo 색상/스타일
- "이동량 급증" 프록시의 정확한 임계값, 오프셋 유지시간 파라미터의 기본 수치 (전부 Inspector 노출 필수, 플레이테스트로 조정될 초기값)

### Deferred Ideas (OUT OF SCOPE)
- Y축 데드존/오프셋/피킹 확장 여부 — 이번 논의에서 다루지 않음. 사용자 수식은 X축(및 수직 피킹만) 중심으로 제시되었으므로, Y축 데드존이 필요한지는 이번 Phase 범위 밖. 실행 중 애매하면 재논의 필요.
- Phase 9에서 이미 이월된 Y축 카메라 이동 범위 제한(minY/maxY) — 여전히 범위 밖.

</user_constraints>

## Project Constraints (from CLAUDE.md)

- **기획 우선:** 계획(PLAN.md) 없이 코드부터 작성하지 않는다. 불확실하면 임의로 하나를 조용히 선택하지 말고 사용자/플래너에게 제시한다 — 아래 Open Questions가 이 원칙에 해당.
- **단순성 우선 / 범위 준수:** 요청받은 것 이상을 추가하지 않는다. 이번 Phase(10) 범위를 벗어나는 코드(예: Y축 데드존 전체 확장, Cinemachine 도입)는 작성하지 않는다.
- **정밀한 변경(Surgical Changes):** 반드시 필요한 부분만 건드린다. 인접 코드/주석/포맷팅을 임의로 "개선"하지 않는다. 기존 CP949 한글 주석은 삽입 전용 편집으로 보존한다 (Phase 9 정밀 전례, 아래 Pitfall 1 참조). 자신의 변경으로 고립된 요소만 정리하고 기존 데드코드는 언급만 한다.
- **추적 가능성:** 변경된 모든 줄은 실행 중인 플랜과 직접 연결되어야 한다.
- **목표 주도적 실행:** 검증 가능한 성공 기준으로 변환한다 (예: "부드럽게 전환" → "Play 모드에서 Size/좌표가 급격히 튀지 않고 N프레임에 걸쳐 보간되는지 확인"). `.planning/config.json`에 `mode: yolo`가 없으므로 사용자 확인 없이 독단적으로 다음 단계로 진행하지 않는다.
- **읽기 전용 참조 파일:** `PlayerController.cs`, `InputHandler.cs`는 절대 수정하지 않는다 (D-04, D-08, D-09, D-10, D-11 — 컨텍스트에서 반복적으로 명시).

## Summary

Phase 10 is a pure C#/Unity extension of the existing `CameraController.cs` singleton (no new packages, no Cinemachine, no external dependencies). All required math is already specified verbatim by the user in `10-CONTEXT.md`'s `<specifics>` section — this research does not re-derive the formulas, it focuses on how to wire them into Unity's `MonoBehaviour` lifecycle and the existing `LateUpdate` pipeline without breaking Phase 9's guarantees (zoom Lerp, X-clamp, CP949-encoded file integrity).

Three techniques compose additively in normal stages: a hard-cut deadzone box (X-axis push, D-14), a `SmoothDamp`-based dynamic asymmetrical offset that shifts opposite the player's movement direction (D-05/D-06/D-07), and a `SmoothDamp`-based vertical peek offset driven by held directional input while idle+grounded (D-08–D-13). In boss zones, all three are bypassed entirely in favor of Phase 9's legacy `Vector3.Lerp` follow (D-15). The X-clamp always runs last, unchanged (D-17).

Two integration gaps not covered by CONTEXT.md's decisions were found during research and must be resolved by the plan: (1) `CameraController` currently has **no stored boolean** for "is a boss zone active" — `SetBossZoom(bool)` only writes `_targetZoom`, so a new private field is required to branch `LateUpdate` per D-15; (2) the intended behavior of the **Y-axis** during the normal-stage hard-cut path is genuinely ambiguous in CONTEXT.md (see Open Questions) and should not be silently decided by the planner.

**Primary recommendation:** Extend `CameraController.cs` via `Edit` (never `Write`) with ASCII-only new comments, decompose `LateUpdate`'s normal-stage path into three private helper methods (`ApplyBaseDeadzone`, `ApplyDynamicOffset`, `ApplyPeeking`) that each consume a single per-frame-cached `target.position` delta, add a `_isBossZone` bool set inside `SetBossZoom`, and gate the entire new pipeline behind that flag — leaving the boss-zone branch byte-for-byte identical to Phase 9's existing Lerp line.

## Standard Stack

No new packages, no new Unity modules, no Cinemachine. This phase is 100% hand-rolled `UnityEngine` scripting on top of the existing `Assets/Camera/Script/CameraController.cs`, consistent with Phase 9's explicit choice ("Cinemachine 미사용 — 순수 `UnityEngine.Camera` + 수동 스크립트 구조를 그대로 따른다").

### Core APIs used (all part of `UnityEngine`, no install needed)
| API | Purpose | Why standard |
|---|---|---|
| `Mathf.SmoothDamp(float, float, ref float, float, ...)` | Dynamic offset (X) and peek offset (Y) smoothing (D-07) | Official spring-damper smoothing primitive, unchanged API since Unity 5; verified current signature via [docs.unity3d.com](https://docs.unity3d.com/ScriptReference/Mathf.SmoothDamp.html) |
| `Mathf.Clamp` | Already used by `ApplyXClamp()` — no change needed | Existing Phase 9 code |
| `MonoBehaviour.OnDrawGizmos()` | Editor-only deadzone box visualization (D-03) | Zero runtime cost, standard Unity editor-visualization hook |
| `Gizmos.DrawWireCube` / `Gizmos.color` | Draw the deadzone box outline | Matches existing project pattern (see Code Examples — `WeatherController.cs`) |
| `Action<Vector2>` event subscription (`InputHandler.Instance.OnMoveEvent`) | Read vertical input for peeking (D-08) | Existing project-wide input bus pattern already used by `PlayerController.cs` |

### Alternatives Considered
| Instead of | Could use | Tradeoff |
|---|---|---|
| Hand-rolled deadzone/offset/peek (locked) | Cinemachine `CinemachineFramingTransposer` (Dead Zone Width/Height, Soft Zone, Look Ahead Time) | Cinemachine ships equivalent features out of the box, but Phase 9 already locked out Cinemachine for this project ("순수 `UnityEngine.Camera`"), and CONTEXT.md gives exact hand-rolled formulas — not revisited here, just noted for awareness. |

**Installation:** None. No `npm`/`nuget`/Unity Package Manager changes required.

**Version verification:** Not applicable — no package versions to pin. Unity project's engine version was not part of this phase's file set to inspect (not required, since only `UnityEngine` core scripting APIs — stable across Unity 2019–6000.x — are used).

## Architecture Patterns

### Recommended structural change to `CameraController.cs`

```
LateUpdate()
├── if (target == null) return;
├── compute per-frame target-position delta ONCE (shared by all 3 systems)
├── if (_isBossZone)                         // D-15 — NEW private bool, set inside SetBossZoom
│     transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothing * Time.deltaTime);
│     // <- byte-identical to Phase 9's existing line, just moved under a branch
├── else
│     Vector3 basePos   = ApplyBaseDeadzone(delta);   // D-01/D-02/D-14 — hard cut, X only (see Open Questions)
│     Vector3 dynOffset = ApplyDynamicOffset(delta);  // D-05/D-06/D-07 — SmoothDamp, sign of push direction
│     Vector3 peekOff   = ApplyPeeking(delta);        // D-08–D-13 — SmoothDamp, vertical
│     transform.position = basePos + dynOffset + peekOff;
├── _cam.orthographicSize = Mathf.Lerp(...)   // D-16 — UNCHANGED, runs in both branches
└── ApplyXClamp();                            // D-17 — UNCHANGED, runs LAST in both branches
```

### Pattern 1: Single shared per-frame delta (avoid triple-computing "is the player moving")
**What:** Compute `Vector3 frameDelta = target.position - _lastTargetPos;` once at the top of the non-boss branch of `LateUpdate`, then update `_lastTargetPos = target.position;` at the very end of the frame (after it's been read by all three helpers). Pass `frameDelta` into `ApplyDynamicOffset` (direction sign) and `ApplyPeeking` (near-zero magnitude check, D-10) instead of recomputing it independently in each.
**When to use:** Any time multiple sibling systems in the same `LateUpdate` need "is the target moving / which way" — recomputing a `transform.position` diff 2-3 times per frame is wasteful and risks the two computations drifting if one runs before/after a mutation.
**Why it matters here:** D-04 and D-10 independently describe "frame delta of target.position" as the proxy for both movement-direction (offset) and velocity-is-zero (peeking). Because both decisions describe the *same underlying signal*, D-11's "sudden movement spike" cancel condition for peeking does not need a second detection mechanism — a magnitude threshold on the same `frameDelta` used for D-10 already catches dashes and knockback (both produce an unusually large single-frame position delta), since `PlayerController.isDashing`/`isKnockedBack` are private and D-11 explicitly forbids adding accessors for them.

### Pattern 2: Deadzone hard-cut box relative to the camera's own current position
**What:** The deadzone box is not centered on `target.position` — it is centered on the camera's *own current* `transform.position.x` (i.e., "does the player's absolute X exceed the box edges that the camera is currently sitting on"). Camera doesn't move at all while the player is inside; when the player exits, the camera snaps forward by exactly the overrun distance, effectively re-centering the box edge on the player.
```csharp
// Source: derived directly from CONTEXT.md <specifics> section 1 (Base Deadzone), no external reference needed
private Vector3 ApplyBaseDeadzone()
{
    float halfW = deadzoneWidth * 0.5f;
    float camX = transform.position.x; // box is anchored to the camera's current resting position
    float newX = camX;

    if (target.position.x < camX - halfW)
        newX = target.position.x + halfW;      // player pushed the left edge -> snap left edge to player
    else if (target.position.x > camX + halfW)
        newX = target.position.x - halfW;      // player pushed the right edge -> snap right edge to player
    // else: inside the box, newX stays == camX (D-14 "정지 유지")

    return new Vector3(newX, transform.position.y, transform.position.z);
}
```
**When to use:** This is the D-14-mandated hard-cut layer; it must run BEFORE the two `SmoothDamp` offset layers are added on top (they add to its result, they do not replace it).

### Pattern 3: Sustain-then-decay state for the dynamic offset (D-06)
**What:** D-06 requires the offset to *not* snap back to zero the instant the player stops pushing the deadzone edge — it must hold the last non-zero target offset for a duration, then let `TargetOffset` fall back to 0 (still smoothed by the same `SmoothDamp`, D-07). This needs one additional timer field beyond the `SmoothDamp` velocity ref:
```csharp
// Illustrative shape only — exact names/defaults are Claude's Discretion per CONTEXT.md
private float _offsetHoldTimer;
public float offsetHoldDuration = 0.4f; // playtest-tunable, Inspector-exposed per Claude's Discretion note

private Vector3 ApplyDynamicOffset(bool isPushingDeadzone, float pushDirectionSign)
{
    float targetOffsetX;
    if (isPushingDeadzone)             // D-05: only while actively pushing the deadzone boundary
    {
        targetOffsetX = -(pushDirectionSign * maxOffsetDistance);
        _offsetHoldTimer = offsetHoldDuration; // refresh hold window
    }
    else if (_offsetHoldTimer > 0f)
    {
        _offsetHoldTimer -= Time.deltaTime;
        targetOffsetX = _currentOffsetX;       // hold: don't change the SmoothDamp target yet
    }
    else
    {
        targetOffsetX = 0f;                    // hold expired: let SmoothDamp ease back to 0
    }

    _currentOffsetX = Mathf.SmoothDamp(_currentOffsetX, targetOffsetX, ref _offsetVelocityX, offsetSmoothTime);
    return new Vector3(_currentOffsetX, 0f, 0f);
}
```
**Why:** Without the hold timer, `TargetOffset` would immediately jump back to 0 the frame the player stops pushing, and `SmoothDamp` would start easing back before the user's intended "look ahead lingers briefly" effect (D-06) is achieved.

### Pattern 4: Peeking as an event-driven cached-input read, guarded at consumption time
**What:** Subscribe once in `OnEnable`/unsubscribe in `OnDisable` (D-09 lifecycle requirement, matching `InputHandler`'s `DontDestroyOnLoad` vs. `CameraController`'s scene-local lifetime). The handler should stay a "dumb" latest-value cache — mirroring the existing project convention in `PlayerController.OnMove()`, which unconditionally stores `moveInput` regardless of `movementLocked` and lets *readers* decide whether to act on it. Apply the `movementLocked` guard (D-09) at the point `LateUpdate` reads the cached input, not inside the event handler itself — this keeps the handler symmetric with the existing codebase style and avoids stale-value edge cases.
```csharp
// Source: pattern mirrors PlayerController.cs Start()/OnDestroy() (lines 72-86) subscribe/unsubscribe symmetry
void OnEnable()
{
    if (InputHandler.Instance != null)
        InputHandler.Instance.OnMoveEvent += HandleMoveInput;
}

void OnDisable()
{
    if (InputHandler.Instance != null)
        InputHandler.Instance.OnMoveEvent -= HandleMoveInput;
}

private void HandleMoveInput(Vector2 input) => _lastRawInput = input; // no gating here (see above)
```
Then in the peeking helper (consumption site), gate on `movementLocked`, `IsGrounded()`, near-zero `frameDelta`, and `_lastRawInput.y != 0` together — all four are locked conditions (D-09, D-10, D-12, and the peeking formula in `<specifics>`).

**Caching note:** Both `movementLocked` and `IsGrounded()` require a `PlayerController` reference (`target.GetComponent<PlayerController>()`). Cache this once (e.g., in `Start()`, alongside the existing `_cam = GetComponent<Camera>()` pattern already in the file) rather than calling `GetComponent` every frame.

### Anti-Patterns to Avoid
- **Lerping the deadzone box itself:** D-14 explicitly forbids this — "데드존 자체를 Lerp로 쫓아가면... '박스 안에서는 완전 정지'라는 취지가 깨진다." Only the two offset layers (dynamic + peek) may use `SmoothDamp`; the base deadzone position must be a hard, un-smoothed assignment.
- **Branching `LateUpdate` on `_targetZoom == bossZoom` instead of a dedicated bool:** `_targetZoom` is a tunable float that could coincidentally equal another tunable float, and comparing floats for equality is fragile. Add an explicit `_isBossZone` bool inside `SetBossZoom(bool)` instead (see Common Pitfalls).
- **Re-deriving `isDashing`/`isKnockedBack` via new public accessors on `PlayerController`:** Explicitly forbidden by D-11 and the "read-only reference, do not modify" constraint on `PlayerController.cs`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---|---|---|---|
| Frame-rate-independent smoothing toward a moving target | A custom exponential-decay or manual velocity-integration smoother | `Mathf.SmoothDamp` (already the project's established primitive via `Mathf.Lerp` elsewhere, and explicitly named in D-07) | Battle-tested, handles overshoot/critical damping correctly; reinventing it risks subtle frame-rate-dependent bugs that `SmoothDamp` already solves. |
| Full-featured camera deadzone/soft-zone/look-ahead system | A generalized, reusable "camera rules" framework | The three purpose-built helper methods described above | This is explicitly a one-off, project-specific pipeline (CLAUDE.md: no unrequested abstraction/flexibility for a single use site). Cinemachine already provides a generalized version of this (`CinemachineFramingTransposer`), but that door was closed in Phase 9's context — not reopened here. |

**Key insight:** Every formula in this phase is already fully specified by the user (`<specifics>` in CONTEXT.md). The engineering risk is not "what's the right math" — it's "how do these three additive layers compose inside one `MonoBehaviour.LateUpdate` without duplicating the X-clamp or breaking the boss-zone legacy path," which is why this research is architecture-focused rather than algorithm-focused.

## Common Pitfalls

### Pitfall 1: CP949/EUC-KR file encoding corruption (carried over from Phase 9, HIGH severity)
**What goes wrong:** `Assets/Camera/Script/CameraController.cs` is saved in a non-UTF-8 encoding (Korean comments render as mojibake — e.g. `���ΰ�` — when read via UTF-8-assuming tools, confirmed directly in this research's file read). Using the `Write` tool to rewrite the whole file, or including a Korean comment line as `old_string` in an `Edit` call, silently corrupts the existing 5 non-ASCII comment lines.
**Why it happens:** The file predates UTF-8-only tooling; Phase 9 already hit and solved this exact problem.
**How to avoid:** Reuse Phase 9's exact protocol (see `.planning/phases/09-camera-zoom-stage-transition/09-01-PLAN.md` §1 "파일 인코딩"): edit only via `Edit` with ASCII-only anchor lines and ASCII-only new comments; never `Write` the full file; verify post-edit with a non-printable-byte-count gate (Phase 9 used `LC_ALL=C grep -c '[^[:print:][:space:]]' Assets/Camera/Script/CameraController.cs` == baseline count, and confirmed zero deleted lines via `git diff <baseline> -- <file> | grep '^-' | grep -vc '^---'` == 0).
**Warning signs:** Any diff touching a `-` (deleted) line in this file, or a post-edit non-ASCII line count that differs from the pre-edit baseline (5, per Phase 9's `09-VERIFICATION.md`), is a signal of encoding corruption.

### Pitfall 2: No existing boolean state for "is boss zone active"
**What goes wrong:** `CameraController.SetBossZoom(bool isBossStage)` currently ONLY writes `_targetZoom = isBossStage ? bossZoom : normalZoom;` — it discards the boolean itself. D-15 requires `LateUpdate` to branch every frame on "is a boss zone currently active," but there is nothing to branch on yet.
**Why it happens:** Phase 9 only needed the zoom *value*, not a persisted zone-state flag, so it was never stored.
**How to avoid:** Add a new private field (e.g. `private bool _isBossZone;`) and set it inside `SetBossZoom`. This is a small, additive, in-scope change to `SetBossZoom`'s body (not its signature — `BossZoomTrigger.cs` calls it unchanged) and should be called out explicitly in the plan so it isn't missed as "should already exist."
**Warning signs:** If the plan's task list never mentions touching `SetBossZoom`'s body, this gap will surface as a compile-time "no such field" or a design that awkwardly re-derives boss-zone state by comparing `_targetZoom == bossZoom` (fragile float equality, see Anti-Patterns).

### Pitfall 3: Event subscription leak from `InputHandler.OnMoveEvent`
**What goes wrong:** `InputHandler` is `DontDestroyOnLoad`; `CameraController` is scene-local and destroyed on every scene transition (per its own comment, confirmed in the file: "Not persisted across scene loads on purpose: every stage scene owns its own camera"). If `CameraController` subscribes to `InputHandler.Instance.OnMoveEvent` without unsubscribing in `OnDisable`, each scene load leaves a dangling subscription referencing a destroyed `CameraController` instance, and `InputHandler.OnMoveEvent?.Invoke(...)` will still try to call into it (Unity nullifies destroyed `MonoBehaviour` references gracefully in most cases, but the delegate list itself grows unboundedly across scene loads, which is a memory/perf leak and a source of "did this fire twice?" bugs).
**Why it happens:** Copy-pasting `PlayerController`'s `Start()`/`OnDestroy()` subscribe pattern without considering `CameraController`'s different singleton lifetime.
**How to avoid:** Subscribe in `OnEnable`, unsubscribe in `OnDisable` (not `Start`/`OnDestroy` — CONTEXT.md D-09 explicitly calls out `OnEnable`/`OnDisable` for this reason). Guard both with `if (InputHandler.Instance != null)`.
**Warning signs:** Console warnings/errors on scene transition referencing a destroyed `CameraController`, or peeking behavior "sticking" from the previous scene for one frame after a scene load.

### Pitfall 4: Ambiguous Y-axis behavior in the non-boss hard-cut path
**What goes wrong:** See Open Questions below — implementing *something* for Y without checking with the user/planner risks building against an assumption CONTEXT.md itself flags as unresolved ("실행 중 애매하면 재논의 필요").
**How to avoid:** Surface the ambiguity explicitly in the plan rather than silently picking an interpretation (per CLAUDE.md "가정 명확화").

### Pitfall 5: Redundant `GetComponent` calls
**What goes wrong:** Calling `target.GetComponent<PlayerController>()` inside `LateUpdate` (for `movementLocked` and `IsGrounded()`) every frame is a minor but avoidable per-frame allocation-free-but-not-free lookup.
**How to avoid:** Cache the reference once (`Start()` or `Awake()`, mirroring the existing `_cam = GetComponent<Camera>()` pattern already in the file), matching the file's own established style.

## Code Examples

### Deadzone gizmo (matches existing project convention)
```csharp
// Source: pattern copied from Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs:44-56
// (existing project convention for translucent-fill + wire-outline gizmo boxes)
private void OnDrawGizmos()
{
    Gizmos.color = new Color(1f, 1f, 0f, 0.15f); // translucent fill, color is Claude's Discretion
    Gizmos.DrawCube(transform.position, new Vector3(deadzoneWidth, deadzoneHeight, 0f));
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneWidth, deadzoneHeight, 0f));
}
```

### `Mathf.SmoothDamp` float overload — verified current signature
```csharp
// Source: https://docs.unity3d.com/ScriptReference/Mathf.SmoothDamp.html (verified live, unchanged for years)
public static float SmoothDamp(
    float current, float target, ref float currentVelocity,
    float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = Time.deltaTime);
```
`ref currentVelocity` must be a **persistent field**, not a local variable re-declared each frame — otherwise the damping state resets every call and the value snaps instead of easing.

## State of the Art

| Old Approach (Phase 9 and earlier) | New Approach (this phase) | Impact |
|---|---|---|
| Camera continuously `Vector3.Lerp`s toward `target.position + offset` every frame, everywhere | Normal stages: hard-cut deadzone box + two additive `SmoothDamp` offset layers; boss zones: unchanged legacy Lerp | Normal-stage camera becomes "sticky" (Mario-style) instead of always trailing the player; boss-zone feel is explicitly preserved byte-for-byte |
| No concept of "is a boss zone currently active" as stored state | New `_isBossZone` bool required (see Pitfall 2) | Enables the D-15 branch; without it, D-15 cannot be implemented cleanly |

**Deprecated/outdated:** Nothing in this phase deprecates prior Phase 9 work — D-16 and D-17 explicitly require the zoom Lerp and X-clamp to keep functioning unchanged in both branches.

## Open Questions

1. **Does the Base Deadzone / hard-cut layer touch the camera's Y position at all in normal stages?**
   - What we know: D-01 mandates a separate `deadzoneHeight` Inspector field ("가로/세로 별도 필드... 메트로배니아 특성상 좌우/상하 이동 폭이 다르므로 분리"), implying vertical extent matters. All three `<specifics>` formulas (Base Deadzone, Dynamic Offset, Peeking) are written exclusively in terms of X, except Peeking which is explicitly vertical. The `<deferred>` section separately states "Y축 데드존... 확장 여부... 이번 Phase 범위 밖... 실행 중 애매하면 재논의 필요."
   - What's unclear: Whether `deadzoneHeight` is (a) purely a Gizmo-visualization field this phase (box drawn full-size, but only the X edges actually gate camera movement — Y stays exactly wherever it was left, meaning Peeking becomes the *only* Y-axis movement mechanism in normal stages), or (b) meant to gate a symmetric Y push exactly like X (i.e., the deadzone box is a true 2D box on both axes), with only the worked example given in X.
   - Recommendation: Default to interpretation (a) — Y untouched by the hard-cut layer, `deadzoneHeight` exists for Inspector/Gizmo completeness per D-01 but is functionally inert for camera-movement purposes this phase — because it is the more literal reading of "사용자 수식은 X축(및 수직 피킹만) 중심" and does not require inventing an unspecified symmetric-Y formula. However, per CLAUDE.md's "가정 명확화" and CONTEXT.md's own "재논의 필요" flag, the plan should surface this explicitly as a confirmable assumption (e.g., a plan task note or a check-in point) rather than treat it as silently resolved. Note the behavioral consequence either way: under interpretation (a), a player jumping high enough in a normal stage would move off-screen vertically with zero peeking active — worth confirming this is acceptable for the current stage designs.

2. **Default tuning values for the new Inspector fields not given an initial number by the user**
   - What we know: `Threshold` (0.5s) and presumably `PeekDistance` have explicit example values in `<specifics>`. `deadzoneWidth`/`deadzoneHeight`, `maxOffsetDistance`, `offsetSmoothTime`/peek `SmoothTime`, `offsetHoldDuration`, and the "sudden movement" spike threshold do not.
   - What's unclear: Exact starting numbers.
   - Recommendation: Per CONTEXT.md's Claude's Discretion note, these are explicitly playtest-tunable Inspector defaults — the plan should pick reasonable placeholder values (following the Phase 9 precedent of picking a plausible number and noting "플레이테스트로 조정될 초기값" in a comment) rather than blocking on exact numbers.

3. **Does the peek offset use the same `SmoothTime` for activation and cancellation, or a faster one for the cancel/"복귀"?**
   - What we know: `<specifics>` says on cancel, reset `t=0`/`TargetPeeking=0` "후 빠른 SmoothDamp로 복귀" (return via a *fast* SmoothDamp) — the word "빠른" (fast) suggests the return might intentionally use a shorter smooth time than the slower onset easing.
   - What's unclear: Whether this means literally a second, faster `smoothTime` constant just for the return phase, or is just descriptive language for "SmoothDamp will naturally move quickly once the target snaps to 0."
   - Recommendation: Simplest compliant reading is a single `SmoothDamp` call with one `smoothTime`; if a two-speed cancel is desired, expose a second Inspector field (e.g., `peekReturnSmoothTime`). Flag as Claude's Discretion territory, consistent with "SmoothDamp velocity 임시 변수 관리 방식" being listed as discretionary.

## Sources

### Primary (HIGH confidence)
- `Assets/Camera/Script/CameraController.cs` — direct read, confirms current `LateUpdate` order and the missing `_isBossZone` state gap
- `Assets/Camera/Script/BossZoomTrigger.cs` — direct read, confirms `SetBossZoom(bool)` call sites and signature stability requirement
- `Assets/Player/Script/InputHandler.cs`, `Assets/Player/Script/PlayerController.cs` — direct read, confirms `OnMoveEvent` shape, `movementLocked`/`IsGrounded()` public surface, and existing subscribe/unsubscribe convention
- `Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs:44-56` — direct read, existing project `OnDrawGizmos` convention (fill + wire cube) reused for the deadzone gizmo example
- `.planning/phases/09-camera-zoom-stage-transition/09-01-PLAN.md`, `09-VERIFICATION.md`, `09-01-SUMMARY.md` — direct read, CP949 insert-only-edit protocol and verification gates (Pitfall 1)
- [Unity Scripting API: Mathf.SmoothDamp](https://docs.unity3d.com/ScriptReference/Mathf.SmoothDamp.html) — official docs, verified current signature live via WebSearch

### Secondary (MEDIUM confidence)
- General Unity 2D camera deadzone terminology (hard vs. soft deadzone, look-ahead) cross-referenced against multiple community sources (Unity Discussions threads, tutorial articles) — used only to confirm the hand-rolled design here matches well-established industry patterns (conceptually equivalent to Cinemachine's Framing Transposer Dead Zone/Look Ahead), not to introduce any new implementation detail beyond what CONTEXT.md already locks.

### Tertiary (LOW confidence)
None — all architectural claims in this document are grounded in direct file reads of this repository or official Unity documentation.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new dependencies, pure `UnityEngine` core API confirmed against live official docs
- Architecture: HIGH — derived directly from reading the actual existing `CameraController.cs`/`BossZoomTrigger.cs`/`InputHandler.cs`/`PlayerController.cs` source and Phase 9's precedent plans/verification
- Pitfalls: HIGH for encoding/subscription/GetComponent-caching (all directly observed in this repo's code and Phase 9 history); MEDIUM for the Y-axis scope pitfall (genuinely ambiguous per the user's own CONTEXT.md, not a research gap)

**Research date:** 2026-08-04
**Valid until:** No expiry driver — this is a closed, hand-rolled, dependency-free phase; findings remain valid until `CameraController.cs` itself changes again (next camera-related phase should re-read the file fresh rather than rely on line numbers cited here).
