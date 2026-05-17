using System.Collections;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public float destroytime = 5;
    public int damage = 1;
    private Animator animator;
    public int handspeed;
    private float initialY;
   void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Appear");
        Invoke("Attack", 0.5f);
        Destroy(gameObject, destroytime);
        initialY = transform.position.y;

    [Header("공격 설정")]
    public float warningDuration = 1.0f; // 경고 유지 시간
    public float attackLifetime = 2.0f;  // 손 존재 시간
    public float handRiseSpeed = 2f;     // 상승 속도
    public float handRiseHeight = 2f;    // 상승 높이
    public int damage = 10;              // 데미지
    public float attackInterval = 4f;    // 반복 공격 간격
    public float detectionRadius = 12f;  // 플레이어 탐지 범위

    private Transform player;

    void Start()
    {
        // 🎯 플레이어 자동 탐색
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            StartCoroutine(AttackLoop());
        }
        else
        {
            Debug.LogWarning("Player not found. Hand system disabled.");
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return StartCoroutine(AttackSequence());
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator AttackSequence()
    {
        // 플레이어가 탐지 범위 밖이면 스킵
        if (player == null || Vector2.Distance(transform.position, player.position) > detectionRadius)
            yield break;

        // 1️⃣ 플레이어 아래 위치 계산
        Vector2 playerPos = player.position;
        Vector2 targetPos = GetGroundPosition(playerPos);

        // 2️⃣ 경고 표시
        GameObject warning = Instantiate(warningPrefab, targetPos, Quaternion.identity);
        yield return new WaitForSeconds(warningDuration);
        Destroy(warning);

        // 3️⃣ 손 생성
        GameObject hand = Instantiate(handVisualPrefab, targetPos, Quaternion.identity);

        float startY = hand.transform.position.y;
        bool reachedTop = false;

        float elapsed = 0f;
        while (elapsed < attackLifetime)
        {
            // 🧩 손이 삭제되었으면 즉시 루프 중단
            if (hand == null)
                yield break;

            if (!reachedTop)
            {
                hand.transform.Translate(Vector3.up * handRiseSpeed * Time.deltaTime);
                if (hand.transform.position.y >= startY + handRiseHeight)
                    reachedTop = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }


        Destroy(hand);
    }

    private Vector2 GetGroundPosition(Vector2 playerPosition)
    {
        int groundMask = LayerMask.GetMask("Ground", "Platform");
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, Vector2.down, 10f, groundMask);
        if (hit.collider != null)
            return hit.point;
        else
            return new Vector2(playerPosition.x, playerPosition.y - 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HP hp = other.GetComponent<HP>();
            if (hp != null)
                hp.TakeDamage(damage);
        }
    }
}
