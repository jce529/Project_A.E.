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

    // Scene-local singleton. BossZoomTrigger calls in through this (D-01).
    // Not persisted across scene loads on purpose: every stage scene owns its own camera.
    public static CameraController Instance { get; private set; }

    private Camera _cam;
    private float _targetZoom;

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
    }

    void Start()
    {
        // [�߿�] ���� ī�޶� ��� �ֵ� �������,
        // ���� �������ڸ��� ������ Ÿ���� '�̻����� ��ġ(������ ����)'�� �����̵���ŵ�ϴ�.
        transform.position = target.position + offset;
        // Start already at the normal-stage zoom so frame 1 does not play a transition.
        _cam.orthographicSize = normalZoom;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        // Zoom Lerp toward the current stage target size (D-06 / D-07).
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, zoomSmoothing * Time.deltaTime);
    }
}