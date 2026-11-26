using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Puddle : MonoBehaviour
{
    [Header("장판 설정")]
    [Tooltip("장판이 생성되기 전 경고(전조) 시간")]
    public float telegraphTime = 1f;
    [Tooltip("실제 독 장판이 유지되는 시간")]
    public float activeTime = 4f;

    [Header("데미지 설정")]
    [Tooltip("틱당 데미지")]
    public int tickDamage = 5;
    [Tooltip("데미지가 들어가는 주기 (예: 0.5초마다 틱 데미지)")]
    public float tickRate = 0.5f;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isActive = false;
    private float nextTickTime = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // 처음엔 데미지를 주지 않도록 콜라이더를 꺼둡니다.
        col.enabled = false;

        StartCoroutine(PuddleRoutine());
    }

    private IEnumerator PuddleRoutine()
    {
        // 1. 전조 상태 (Telegraph) - 플레이어에게 피할 시간 제공
        Color originalColor = sr.color;
        // 경고 느낌을 위해 약간 투명한 붉은색/주황색으로 시작
        sr.color = new Color(1f, 0.5f, 0f, 0.5f);

        yield return new WaitForSeconds(telegraphTime);

        // 2. 장판 활성화 (Active)
        isActive = true;
        col.enabled = true; // 데미지 판정 시작
        sr.color = originalColor; // 원래의 독(장판) 색상으로 변경

        yield return new WaitForSeconds(activeTime);

        // 3. 수명 다함
        Destroy(gameObject);
    }

    // 트리거 안에 머물고 있을 때 지속적으로 호출됨
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            // 틱 주기마다 데미지 적용
            if (Time.time >= nextTickTime)
            {
                PlayerStats playerStats = other.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(tickDamage);
                }
                // 다음 데미지가 들어갈 시간 설정
                nextTickTime = Time.time + tickRate;
            }
        }
    }
}