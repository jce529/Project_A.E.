using UnityEngine;

public class boss : MonoBehaviour
{
    [Header("참조 컴포넌트")]
    public HP hp;
    public Chase chase;   // 👈 플레이어 추적 담당 스크립트
    public Hand hand;     // 👈 공격 담당 스크립트

    // 탐지 관련 이벤트
    public delegate void BossDetectionEvent(bool detected);
    public event BossDetectionEvent OnPlayerDetectionChanged;

    private bool playerDetected = false;

    void Start()
    {
        if (hp == null) hp = GetComponent<HP>();
        if (chase == null) chase = GetComponent<Chase>();
        if (hand == null) hand = GetComponent<Hand>();
    }

    void Update()
    {
        if (hp == null || hp.Health <= 0) return;

        // 👇 Chase의 detectionRange를 그대로 활용
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= chaseDetectionRange();

        if (inRange && !playerDetected)
        {
            playerDetected = true;
            OnPlayerDetectionChanged?.Invoke(true);   // 체력바 표시
        }
        else if (!inRange && playerDetected)
        {
            playerDetected = false;
            OnPlayerDetectionChanged?.Invoke(false);  // 체력바 숨김
        }
    }

    float chaseDetectionRange()
    {
        // Chase 클래스의 private detectionRange 접근용
        var field = typeof(Chase).GetField("detectionRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (float)field.GetValue(chase) : 8f;
    }
}
