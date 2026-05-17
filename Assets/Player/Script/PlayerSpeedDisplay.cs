using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSpeedDisplay : MonoBehaviour
{
    [Header("표시 설정")]
    public bool show = true;
    public int fontSize = 20;
    public Color textColor = Color.white;
    public Vector2 screenOffset = new Vector2(10, 10); // 좌상단 기준 offset (픽셀)

    private Rigidbody2D rigid;
    private PlayerController playerController;
    private GUIStyle style;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void OnGUI()
    {
        if (!show || rigid == null) return;

        // 스타일은 OnGUI 안에서만 생성 가능
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
            style.fontStyle = FontStyle.Bold;
        }

        // 현재 실제 속도 (Rigidbody2D 기준)
        float currentVx = rigid.linearVelocity.x;
        float currentVy = rigid.linearVelocity.y;
        float currentSpeed = rigid.linearVelocity.magnitude;

        // 계산된 최대속도 (PlayerController가 있을 때만)
        string maxSpeedLine = "";
        if (playerController != null)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float baseSpeed = isRunning ? playerController.runSpeed : playerController.defaultSpeed;
            // currentSpeedModifier는 private이라 접근 불가 → 실제속도와 baseSpeed 차이로 추정 표시 대신 baseSpeed만 표기
            string mode = isRunning ? "Run" : "Walk";
            maxSpeedLine = $"\nMode  : {mode} (base {baseSpeed:F1})";
        }

        string text =
            $"Speed : {currentSpeed:F2}" +
            $"\nVx    : {currentVx:F2}" +
            $"\nVy    : {currentVy:F2}" +
            maxSpeedLine;

        // 배경 박스 (가독성용)
        Rect bgRect = new Rect(screenOffset.x - 5, screenOffset.y - 5, 230, 110);
        GUI.Box(bgRect, GUIContent.none);

        Rect rect = new Rect(screenOffset.x, screenOffset.y, 220, 100);
        GUI.Label(rect, text, style);
    }
}