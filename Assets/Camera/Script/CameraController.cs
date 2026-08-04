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
    // Defaults are intentionally wide so existing scenes keep their current behaviour
    // until minX / maxX are tuned per scene in the Inspector.
    // Y is NOT clamped in this phase by design (D-09).
    public float minX = -1000f;
    public float maxX = 1000f;

    [Header("Deadzone (normal stages only)")]
    // Deadzone box size in WORLD units. Fixed size, never scaled by zoom (D-01 / D-02).
    // deadzoneWidth gates camera X movement. The height field below is Gizmo / Inspector only
    // in this phase - the hard cut deadzone is X axis only (locked assumption A1 in 10-01-PLAN).
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

    // Scene-local singleton. BossZoomTrigger calls in through this (D-01).
    // Not persisted across scene loads on purpose: every stage scene owns its own camera.
    public static CameraController Instance { get; private set; }

    private Camera _cam;
    private float _targetZoom;

    // True while the player is inside a BossZoomTrigger zone (D-15). SetBossZoom stores it
    // so LateUpdate can bypass the whole deadzone pipeline and keep the Phase 9 behaviour.
    private bool _isBossZone;

    // Resting X of the deadzone box center. The camera parks here instead of chasing the
    // target every frame - this is what makes the box feel "sticky" (D-14 hard cut).
    private float _deadzoneCenterX;
    // Un-peeked Y baseline the camera follows with the legacy 'smoothing' rate.
    // Kept separate from transform.position.y so later offset layers cannot feed back into it.
    private float _followBaseY;
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

    void Awake()
    {
        Instance = this;
        _cam = GetComponent<Camera>();
        _targetZoom = normalZoom;
    }

    // Called by BossZoomTrigger: true on enter, false on exit (D-01 / D-03).
    // Idempotent, last call wins - repeated calls simply overwrite the target size.
    public void SetBossZoom(bool isBossStage)
    {
        _targetZoom = isBossStage ? bossZoom : normalZoom;
        // Store the zone state itself, not just the zoom value - LateUpdate branches on it (D-15).
        _isBossZone = isBossStage;
    }

    // Clamps X so the visible left/right edges never pass minX / maxX (D-09 / D-11).
    // Uses the CURRENT orthographicSize so the bound stays correct mid zoom transition.
    private void ApplyXClamp()
    {
        float halfWidth = _cam.orthographicSize * _cam.aspect;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX + halfWidth, maxX - halfWidth);
        transform.position = pos;
    }

    // Hard cut deadzone (D-14). The camera does not move at all while the target stays inside
    // the box; when the target pushes an edge the box center snaps by exactly the overrun.
    // X axis only - camera Y keeps following the target (locked assumption A1 in 10-01-PLAN).
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
    }

    // Normal stage camera composition. Runs AFTER the legacy follow Lerp in LateUpdate and
    // overwrites its X and Y, which is why that legacy line stays byte identical for the
    // boss path (D-15). Z is still handled by the legacy line above.
    private void ApplyNormalStageCamera()
    {
        UpdateDeadzoneCenter();
        UpdateDynamicOffset();
        _followBaseY = Mathf.Lerp(_followBaseY, target.position.y + offset.y, smoothing * Time.deltaTime);
        Vector3 p = transform.position;
        p.x = _deadzoneCenterX - _currentBoxOffsetX;
        p.y = _followBaseY;
        transform.position = p;
    }

    // Boss zones bypass the deadzone pipeline entirely (D-15). Syncing the anchors to the
    // legacy camera position every frame means returning to a normal stage does not jump.
    // Also used once from Start to seed the anchors.
    private void ResetNormalStageState()
    {
        _deadzoneCenterX = transform.position.x;
        _followBaseY = transform.position.y;
        _currentBoxOffsetX = 0f;
        _offsetVelocityX = 0f;
        _offsetHoldTimer = 0f;
        _deadzonePushSign = 0f;
    }

    // Dynamic asymmetrical deadzone (D-05 / D-06 / D-07). The offset only builds while the
    // target actually pushes a deadzone edge - there is no separate speed threshold (D-05).
    // After the push stops the offset is held for offsetHoldDuration, then eases back to 0 (D-06).
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
            targetOffsetX = 0f;
        }
        _currentBoxOffsetX = Mathf.SmoothDamp(_currentBoxOffsetX, targetOffsetX, ref _offsetVelocityX, offsetSmoothTime);
    }

    // Editor only deadzone visualization (D-03). Zero runtime cost in a build.
    // In play mode the box is drawn at its actual resting center; in edit mode it falls back
    // to the camera transform so the size can still be eyeballed before pressing Play.
    private void OnDrawGizmos()
    {
        float centerX = Application.isPlaying ? _deadzoneCenterX : transform.position.x;
        Vector3 center = new Vector3(centerX, transform.position.y, 0f);
        Vector3 size = new Vector3(deadzoneWidth, deadzoneHeight, 0f);
        Gizmos.color = new Color(1f, 1f, 0f, 0.12f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }

    void Start()
    {
        // [�߿�] ���� ī�޶� ��� �ֵ� �������,
        // ���� �������ڸ��� ������ Ÿ���� '�̻����� ��ġ(������ ����)'�� �����̵���ŵ�ϴ�.
        transform.position = target.position + offset;
        // Start already at the normal-stage zoom so frame 1 does not play a transition.
        _cam.orthographicSize = normalZoom;
        ApplyXClamp();
        // Park the deadzone box and the follow baseline on the camera's start position.
        ResetNormalStageState();
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
        // X clamp LAST so it uses this frame's freshly updated orthographicSize.
        ApplyXClamp();
        // Re-anchor on the clamped position so the camera responds immediately when the
        // target walks back from a clamped map edge instead of eating dead travel (D-17).
        if (!_isBossZone) _deadzoneCenterX = transform.position.x + _currentBoxOffsetX;
    }
}