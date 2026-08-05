using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ���ΰ�
    public float smoothing = 5f;

    // [����] �������� ���ο��� ������� �ʰ�, �⺻���� ���⼭ ���ع����ϴ�.
    // (0, 0, -10)�� 2D ������ ���� ��ġ�Դϴ�. (X,Y�� ���߾�, Z�� �ڷ� 10��ŭ)
    public Vector3 offset = new Vector3(0f, 0f, 10f);

    [Header("Stage Zoom")]
    // Orthographic size used on normal stages (D-04 / D-05).
    public float normalZoom = 5f;
    // Orthographic size used while the player is inside a BossZoomTrigger zone (D-04 / D-05).
    public float bossZoom = 7f;
    // Lerp rate for the zoom transition ONLY. Kept separate from 'smoothing' on purpose (D-07).
    public float zoomSmoothing = 3f;

    [Header("Camera X Bounds")]
    // World-space X limits of the visible area (D-09 / D-10).
    // These two are the STAGE BASE bounds and are never written at runtime (260805-q2u / Q2-01):
    // SetXBounds feeds _targetMinX / _targetMaxX instead, so this Inspector pair stays the
    // fallback the camera falls back to whenever the player is inside no zone at all.
    // Defaults are intentionally wide so existing scenes keep their current behaviour
    // until minX / maxX are tuned per scene in the Inspector.
    // Y is NOT clamped in this phase by design (D-09).
    public float minX = -1000f;
    public float maxX = 1000f;
    // Lerp rate for the bounds transition ONLY - the exact role zoomSmoothing plays for the
    // zoom, and kept separate from both 'smoothing' and 'zoomSmoothing' on purpose (Q2-02).
    public float boundsSmoothing = 3f;

    [Header("Deadzone (normal stages only)")]
    // Deadzone box size in WORLD units. Fixed size, never scaled by zoom (D-01 / D-02).
    // Both fields below are real hard cut gates: the width field gates camera X and the
    // height field gates camera Y. This supersedes locked assumption A1 of 10-01-PLAN,
    // which had the height field as Gizmo / Inspector only (quick task 260804-q6h).
    public float deadzoneWidth = 3f;
    public float deadzoneHeight = 2f;

    [Header("Dynamic Offset (normal stages only)")]
    // How far the deadzone box shifts opposite the movement direction, in world units (D-05).
    // The camera ends up leading the player by this much while an edge is being pushed.
    public float maxOffsetDistance = 1.5f;
    // SmoothDamp time for the offset transition (D-07). Larger = lazier look ahead.
    public float offsetSmoothTime = 0.35f;
    // Seconds the offset is held after the push stops, before it eases back to 0 (D-06).
    public float offsetHoldDuration = 0.4f;

    [Header("Peeking (normal stages only)")]
    // Seconds the vertical input must be held while idle before peeking starts (D-13, user value).
    public float peekThreshold = 0.5f;
    // World units the camera shifts vertically at full input deflection (D-13).
    public float peekDistance = 3f;
    // SmoothDamp time while peeking out, and the faster one used for the snap back (D-13).
    public float peekSmoothTime = 0.35f;
    public float peekReturnSmoothTime = 0.12f;
    // Target speed (world units per second) at or below which the target counts as stopped.
    // This is the Velocity == 0 proxy from the frame delta of target.position (D-10).
    public float idleSpeedThreshold = 0.05f;
    // Target speed that instantly cancels peeking. Dash / knockback proxy - the cause is NOT
    // distinguished (D-11). PlayerController dashSpeed is 20 and runSpeed is 7, so 12 sits
    // between them: running never trips it, dashing and knockback always do.
    public float peekCancelSpeed = 12f;

    // Scene-local singleton. BossZoomTrigger calls in through this (D-01).
    // Not persisted across scene loads on purpose: every stage scene owns its own camera.
    public static CameraController Instance { get; private set; }

    private Camera _cam;
    private float _targetZoom;

    // Bounds the camera is easing TOWARD. Written by SetXBounds only, seeded from minX / maxX
    // in Start. Mirrors the _targetZoom / zoomSmoothing pair exactly (Q2-03).
    private float _targetMinX;
    private float _targetMaxX;
    // Live bounds actually consumed by ApplyXClamp. Lerped toward the target pair every frame,
    // which is what turns a zone handoff into a slide instead of a snap (Q2-03).
    private float _currentMinX;
    private float _currentMaxX;

    // True while the player is inside a BossZoomTrigger zone (D-15). SetBossZoom stores it
    // so LateUpdate can bypass the whole deadzone pipeline and keep the Phase 9 behaviour.
    private bool _isBossZone;

    // Resting X of the deadzone box center. The camera parks here instead of chasing the
    // target every frame - this is what makes the box feel "sticky" (D-14 hard cut).
    private float _deadzoneCenterX;
    // Resting Y of the deadzone box center - the Y axis mirror of _deadzoneCenterX
    // (quick task 260804-q6h). Kept separate from transform.position.y so the peek offset
    // layered on top of it cannot feed back into the box.
    private float _deadzoneCenterY;
    // Deadzone box offset, computed with the user formula -(pushDir * maxOffsetDistance).
    // The camera sits at _deadzoneCenterX MINUS this value, so a negative offset (running
    // right) pushes the camera right and opens up the view ahead (assumption A2 in 10-02-PLAN).
    private float _currentBoxOffsetX;
    // SmoothDamp state. MUST be a persistent field, never a local (Unity SmoothDamp contract).
    private float _offsetVelocityX;
    // Counts down after the push stops so the offset lingers before easing back (D-06).
    private float _offsetHoldTimer;
    // -1 while pushing the left edge, +1 while pushing the right edge, 0 while resting (D-05).
    private float _deadzonePushSign;
    // Last non-zero _deadzonePushSign, retained after the push stops. The hard-cut box (D-14)
    // never re-centers on its own, so once idle this is what lets the offset settle back onto
    // the player's exact X instead of leaving the camera stuck halfW behind them (checkpoint fix).
    private float _lastPushSign;

    // Latest raw move input from the InputHandler bus (D-08). Cached as-is: the movementLocked
    // guard is applied where it is consumed, exactly like PlayerController.OnMove does (D-09).
    private Vector2 _lastMoveInput;
    // True once the OnMoveEvent subscription is live, so it is bound and released exactly once.
    private bool _moveEventBound;
    // Cached once so LateUpdate never calls GetComponent (10-RESEARCH Pitfall 5).
    private PlayerController _playerRef;
    // Target position of the previous frame. Single shared movement signal used for the
    // idle check (D-10) and the dash / knockback spike proxy (D-11).
    private Vector3 _lastTargetPos;
    // Current vertical peek offset added on top of _deadzoneCenterY.
    private float _currentPeekY;
    // SmoothDamp state. MUST be a persistent field, never a local.
    private float _peekVelocityY;
    // How long all peek conditions have held continuously (D-13 threshold timer).
    private float _peekTimer;

    void Awake()
    {
        Instance = this;
        _cam = GetComponent<Camera>();
        _targetZoom = normalZoom;
    }

    // InputHandler is DontDestroyOnLoad while this camera is scene local, so the subscription
    // MUST be released on disable or the delegate list grows on every scene load (D-08 / D-09).
    void OnEnable()
    {
        TryBindMoveEvent();
    }

    void OnDisable()
    {
        if (!_moveEventBound) return;
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnMoveEvent -= HandleMoveInput;
        _moveEventBound = false;
    }

    // OnEnable can run before InputHandler.Awake in the very first scene, so Start retries.
    private void TryBindMoveEvent()
    {
        if (_moveEventBound || InputHandler.Instance == null) return;
        InputHandler.Instance.OnMoveEvent += HandleMoveInput;
        _moveEventBound = true;
    }

    // Dumb latest-value cache, mirroring PlayerController.OnMove which also stores input
    // unconditionally. All gating happens in UpdatePeekOffset (D-09).
    private void HandleMoveInput(Vector2 input)
    {
        _lastMoveInput = input;
    }

    // Called by BossZoomTrigger: true on enter, false on exit (D-01 / D-03).
    // Idempotent, last call wins - repeated calls simply overwrite the target size.
    public void SetBossZoom(bool isBossStage)
    {
        _targetZoom = isBossStage ? bossZoom : normalZoom;
        // Store the zone state itself, not just the zoom value - LateUpdate branches on it (D-15).
        _isBossZone = isBossStage;
    }

    // Called by CameraBoundsTrigger on enter and on exit: hands the camera the X limits of the
    // zone the player is currently scoped to, so a single scene can hard clamp room by room
    // (260805-m41, retargeted by 260805-q2u). On exit the trigger simply feeds back this
    // controller's own minX / maxX, so no bounds history is kept anywhere (Q2-06).
    // Idempotent, last call wins - same contract as SetBossZoom above.
    // Plain field assignment on purpose: LateUpdate eases _currentMinX / _currentMaxX toward
    // this pair every frame and ApplyXClamp consumes the eased values, so a bounds swap needs
    // no extra logic here. Do NOT clamp, validate or interpolate in this method.
    // A zone narrower than the camera view collapses the clamp onto a single X - that is the
    // designer's responsibility when authoring the zone, it is not guarded here.
    public void SetXBounds(float min, float max)
    {
        _targetMinX = min;
        _targetMaxX = max;
    }

    // Clamps X so the visible left/right edges never pass minX / maxX (D-09 / D-11).
    // Uses the CURRENT orthographicSize so the bound stays correct mid zoom transition.
    private void ApplyXClamp()
    {
        float halfWidth = _cam.orthographicSize * _cam.aspect;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _currentMinX + halfWidth, _currentMaxX - halfWidth);
        transform.position = pos;
    }

    // Hard cut deadzone (D-14). The camera does not move at all while the target stays inside
    // the box; when the target pushes an edge the box center snaps by exactly the overrun.
    // X axis only - the Y axis has its own mirror below, UpdateDeadzoneCenterY (260804-q6h).
    // Do NOT Lerp this: smoothing it would open a gap between the box edge and the camera,
    // which defeats the "completely still inside the box" goal (D-14).
    private void UpdateDeadzoneCenter()
    {
        float halfW = deadzoneWidth * 0.5f;
        float px = target.position.x;
        _deadzonePushSign = 0f;
        if (px < _deadzoneCenterX - halfW)
        {
            _deadzoneCenterX = px + halfW;
            _deadzonePushSign = -1f;
        }
        else if (px > _deadzoneCenterX + halfW)
        {
            _deadzoneCenterX = px - halfW;
            _deadzonePushSign = 1f;
        }
        if (_deadzonePushSign != 0f) _lastPushSign = _deadzonePushSign;
    }

    // Y axis mirror of UpdateDeadzoneCenter (quick task 260804-q6h). Same hard cut family:
    // the camera does not move vertically while the target stays inside the box, and the box
    // center snaps by exactly the overrun once an edge is crossed.
    // Deliberately independent of the dynamic offset and of peeking - it writes no push sign
    // and has no grounded / airborne branch, so it behaves identically in the air.
    // Do NOT Lerp or SmoothDamp this, for the same reason as the X axis (D-14).
    private void UpdateDeadzoneCenterY()
    {
        float halfH = deadzoneHeight * 0.5f;
        float py = target.position.y;
        if (py < _deadzoneCenterY - halfH)
        {
            _deadzoneCenterY = py + halfH;
        }
        else if (py > _deadzoneCenterY + halfH)
        {
            _deadzoneCenterY = py - halfH;
        }
    }

    // Normal stage camera composition. Runs AFTER the legacy follow Lerp in LateUpdate and
    // overwrites its X and Y, which is why that legacy line stays byte identical for the
    // boss path (D-15). Z is still handled by the legacy line above.
    private void ApplyNormalStageCamera()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float targetSpeed = (target.position - _lastTargetPos).magnitude / dt;
        UpdateDeadzoneCenter();
        UpdateDeadzoneCenterY();
        UpdateDynamicOffset();
        UpdatePeekOffset(targetSpeed);
        Vector3 p = transform.position;
        p.x = _deadzoneCenterX - _currentBoxOffsetX;
        p.y = _deadzoneCenterY + _currentPeekY;
        transform.position = p;
        // Consumed by next frame's speed proxy. Updated AFTER all three helpers have read it.
        _lastTargetPos = target.position;
    }

    // Boss zones bypass the deadzone pipeline entirely (D-15). Syncing the anchors to the
    // legacy camera position every frame means returning to a normal stage does not jump.
    // Also used once from Start to seed the anchors.
    private void ResetNormalStageState()
    {
        _deadzoneCenterX = transform.position.x;
        _deadzoneCenterY = transform.position.y;
        _currentBoxOffsetX = 0f;
        _offsetVelocityX = 0f;
        _offsetHoldTimer = 0f;
        _deadzonePushSign = 0f;
        _lastPushSign = 0f;
        _currentPeekY = 0f;
        _peekVelocityY = 0f;
        _peekTimer = 0f;
        _lastTargetPos = target.position;
    }

    // Dynamic asymmetrical deadzone (D-05 / D-06 / D-07). The offset only builds while the
    // target actually pushes a deadzone edge - there is no separate speed threshold (D-05).
    // After the push stops the offset is held for offsetHoldDuration, then eases back (D-06).
    // The idle target is halfW (not 0): the hard-cut box (D-14) never re-centers on its own,
    // so easing the offset all the way to 0 would leave the camera parked halfW behind the
    // player - narrower on the side they just walked toward. Settling at halfW instead cancels
    // that lag exactly, so a fully-idle camera always sits on the player's X (checkpoint fix,
    // Phase 10 UAT).
    private void UpdateDynamicOffset()
    {
        float targetOffsetX;
        if (_deadzonePushSign != 0f)
        {
            targetOffsetX = -(_deadzonePushSign * maxOffsetDistance);
            _offsetHoldTimer = offsetHoldDuration;
        }
        else if (_offsetHoldTimer > 0f)
        {
            _offsetHoldTimer -= Time.deltaTime;
            targetOffsetX = _currentBoxOffsetX;
        }
        else
        {
            targetOffsetX = -(_lastPushSign * (deadzoneWidth * 0.5f));
        }
        _currentBoxOffsetX = Mathf.SmoothDamp(_currentBoxOffsetX, targetOffsetX, ref _offsetVelocityX, offsetSmoothTime);
    }

    // Input based peeking (D-08 - D-13). All four conditions must hold together:
    // input not locked (D-09), grounded (D-12), target effectively stopped (D-10),
    // and a held vertical input. A speed spike cancels it immediately (D-11).
    private void UpdatePeekOffset(float targetSpeed)
    {
        bool inputUsable = _playerRef != null && !_playerRef.movementLocked;
        bool grounded = _playerRef != null && _playerRef.IsGrounded();
        bool idle = targetSpeed <= idleSpeedThreshold;
        bool cancelled = targetSpeed >= peekCancelSpeed;
        bool holding = inputUsable && grounded && idle && !cancelled && Mathf.Abs(_lastMoveInput.y) > 0.1f;

        float targetPeek = 0f;
        if (holding)
        {
            _peekTimer += Time.deltaTime;
            if (_peekTimer > peekThreshold)
                targetPeek = _lastMoveInput.y * peekDistance;
        }
        else
        {
            _peekTimer = 0f;
        }

        // Returning to 0 uses the faster smooth time so the view snaps back on cancel.
        float smoothTime = (targetPeek == 0f) ? peekReturnSmoothTime : peekSmoothTime;
        _currentPeekY = Mathf.SmoothDamp(_currentPeekY, targetPeek, ref _peekVelocityY, smoothTime);
    }

    // Editor only deadzone visualization (D-03). Zero runtime cost in a build.
    // In play mode the box is drawn at its actual resting center; in edit mode it falls back
    // to the camera transform so the size can still be eyeballed before pressing Play.
    private void OnDrawGizmos()
    {
        float centerX = Application.isPlaying ? _deadzoneCenterX : transform.position.x;
        float centerY = Application.isPlaying ? _deadzoneCenterY : transform.position.y;
        Vector3 center = new Vector3(centerX, centerY, 0f);
        Vector3 size = new Vector3(deadzoneWidth, deadzoneHeight, 0f);
        Gizmos.color = new Color(1f, 1f, 0f, 0.12f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);

        // X bound visualization (D-09 / D-10) - lets minX/maxX be eyeballed without Play mode.
        // In play mode the LIVE eased bounds are drawn so a zone handoff is visible on screen;
        // in edit mode it falls back to the Inspector base pair, exactly like the deadzone box
        // above falls back to transform.position (Q2-05).
        Gizmos.color = Color.red;
        const float boundLineHeight = 20f;
        float halfLine = boundLineHeight / 2f;
        float drawMinX = Application.isPlaying ? _currentMinX : minX;
        float drawMaxX = Application.isPlaying ? _currentMaxX : maxX;
        Gizmos.DrawLine(new Vector3(drawMinX, transform.position.y - halfLine, 0f), new Vector3(drawMinX, transform.position.y + halfLine, 0f));
        Gizmos.DrawLine(new Vector3(drawMaxX, transform.position.y - halfLine, 0f), new Vector3(drawMaxX, transform.position.y + halfLine, 0f));
    }

    void Start()
    {
        // [�߿�] ���� ī�޶� ��� �ֵ� �������,
        // ���� �������ڸ��� ������ Ÿ���� '�̻����� ��ġ(������ ����)'�� �����̵���ŵ�ϴ�.
        transform.position = target.position + offset;
        // Start already at the normal-stage zoom so frame 1 does not play a transition.
        _cam.orthographicSize = normalZoom;
        // Seed both bounds pairs from the Inspector base values BEFORE the first clamp:
        // ApplyXClamp now reads _currentMinX / _currentMaxX, so leaving them at their default
        // 0 would clamp the camera onto x = 0 on frame 1 (Q2-03).
        _targetMinX = minX;
        _targetMaxX = maxX;
        _currentMinX = minX;
        _currentMaxX = maxX;
        ApplyXClamp();
        // Park the deadzone box and the follow baseline on the camera's start position.
        ResetNormalStageState();
        // Retry in case InputHandler.Instance was not ready yet during OnEnable (D-08).
        TryBindMoveEvent();
        _playerRef = target.GetComponent<PlayerController>();
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        // Normal stages overwrite the legacy follow result with the deadzone pipeline (D-14).
        // Boss zones leave the legacy Lerp above untouched, exactly as in Phase 9 (D-15).
        if (_isBossZone) ResetNormalStageState(); else ApplyNormalStageCamera();
        // Zoom Lerp toward the current stage target size (D-06 / D-07).
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, zoomSmoothing * Time.deltaTime);
        // Bounds Lerp, mirroring the zoom Lerp directly above (Q2-04). Sits before the clamp so
        // the clamp always consumes this frame's freshly eased bounds.
        _currentMinX = Mathf.Lerp(_currentMinX, _targetMinX, boundsSmoothing * Time.deltaTime);
        _currentMaxX = Mathf.Lerp(_currentMaxX, _targetMaxX, boundsSmoothing * Time.deltaTime);
        // X clamp LAST so it uses this frame's freshly updated orthographicSize.
        ApplyXClamp();
        // Re-anchor on the clamped position so the camera responds immediately when the
        // target walks back from a clamped map edge instead of eating dead travel (D-17).
        if (!_isBossZone) _deadzoneCenterX = transform.position.x + _currentBoxOffsetX;
    }
}