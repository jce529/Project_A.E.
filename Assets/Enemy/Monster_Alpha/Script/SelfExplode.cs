using System.Collections;
using UnityEngine;

public class SelfExplode : MonoBehaviour
{
    [Header("자폭 설정")]
    [Tooltip("플레이어가 이 거리 안으로 들어오면 자폭 카운트다운을 시작합니다.")]
    public float explodeTriggerRange = 1.5f;

    [Tooltip("실제 폭발이 데미지를 주는 반경입니다.")]
    public float explosionRadius = 2.5f;

    [Tooltip("자폭 폭발 데미지")]
    public int damage = 20;

    [Tooltip("자폭 시작 후 터질 때까지 걸리는 시간 (전조 시간)")]
    public float telegraphTime = 1f;

    private Transform player;
    private SpriteRenderer sr;
    private MonoBehaviour chaseComponent; // 기존 Chase.cs 스크립트 제어용
    private Rigidbody2D rb;
    private bool isPreparingExplosion = false;

    void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // 기존에 만들어두신 추적 스크립트를 찾아둡니다. (이름이 Chase라고 가정)
        chaseComponent = GetComponent("Chase") as MonoBehaviour;
    }

    void Update()
    {
        if (player == null || isPreparingExplosion) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어가 자폭 트리거 범위 안에 들어왔을 때
        if (distanceToPlayer <= explodeTriggerRange)
        {
            StartCoroutine(ExplosionRoutine());
        }
    }

    private IEnumerator ExplosionRoutine()
    {
        isPreparingExplosion = true;

        // 1. 쫓아가던 이동 멈추기
        if (chaseComponent != null) chaseComponent.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. 전조 연출 (점점 빨라지는 붉은색 깜빡임)
        Color originalColor = sr.color;
        float elapsed = 0f;
        float baseBlinkSpeed = 4f;

        while (elapsed < telegraphTime)
        {
            elapsed += Time.deltaTime;

            // 시간이 지날수록 깜빡임 속도가 빨라지는 수식
            float currentBlinkSpeed = baseBlinkSpeed + (elapsed * 15f);
            float lerp = Mathf.PingPong(elapsed * currentBlinkSpeed, 1f);
            sr.color = Color.Lerp(originalColor, Color.red, lerp);

            yield return null; // 다음 프레임까지 대기
        }

        // 3. 폭발 및 데미지 판정
        // 폭발 반경(explosionRadius) 안의 모든 콜라이더를 검사합니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D coll in colliders)
        {
            if (coll.CompareTag("Player"))
            {
                // 플레이어 체력 스크립트에 데미지 전달
                PlayerStats playerStats = coll.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damage);
                }
            }
        }

        // TODO: (선택) 폭발 파티클 이펙트나 사운드 프리팹을 Instantiate 하시면 타격감이 더 좋습니다.
        Debug.Log(gameObject.name + " 펑! 자폭했습니다.");

        // 4. 터졌으므로 몬스터 자기 자신 파괴
        Destroy(gameObject);
    }

    // 유니티 에디터에서 범위를 시각적으로 보여주는 편의 기능 (기즈모)
    private void OnDrawGizmosSelected()
    {
        // 자폭 트리거 범위 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explodeTriggerRange);

        // 실제 폭발 데미지 범위 (빨간색 원)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}