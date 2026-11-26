using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Flying : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("비행 이동 속도")]
    public float moveSpeed = 3f;
    [Tooltip("플레이어 머리 위로 유지할 높이")]
    public float hoverHeight = 4f;
    [Tooltip("플레이어 감지 거리")]
    public float detectionRange = 10f;

    [Header("공격 설정")]
    public GameObject bulletPrefab; // 기존 bullet.cs가 붙어있는 투사체 프리팹
    public Transform firePoint;     // 침/물방울을 떨어뜨릴 위치 (몬스터 하단)
    public float fireRate = 1.5f;   // 공격 주기 (1초에 몇 번 쏠지)

    [Tooltip("플레이어와의 X축 거리가 이 값 이하일 때(머리 위쯤 왔을 때) 공격")]
    public float dropAttackRangeX = 1.5f;

    private Transform player;
    private Rigidbody2D rb;
    private float nextFireTime;

    void Start()
    {
        // 플레이어 찾기 (태그가 "Player"로 설정되어 있어야 함)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다! 태그를 확인하세요.");
        }

        rb = GetComponent<Rigidbody2D>();

        // ★ 핵심: 공중 몬스터이므로 중력을 0으로 만들어 떨어지지 않게 합니다.
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true; // 회전 방지
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 감지 범위 안에 플레이어가 들어오면 추적 시작
        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
            HandleAttack();
        }
    }

    private void MoveTowardsPlayer()
    {
        // 목표 위치 = 플레이어의 X 위치, Y 위치는 플레이어 머리 위(hoverHeight)
        Vector2 targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);

        // 부드럽게 목표 위치를 향해 이동
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 플레이어 방향으로 스프라이트 좌우 반전 (enemy.cs 로직 재사용)
        if (player.position.x > transform.position.x && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void HandleAttack()
    {
        // 플레이어와 몬스터의 X축 거리(수평 거리) 계산
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // 플레이어가 몬스터 바로 아래쪽(dropAttackRangeX 이내)에 오면 공격
        if (distanceX <= dropAttackRangeX)
        {
            if (Time.time >= nextFireTime)
            {
                DropBullet();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    private void DropBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 투사체 생성 (방향 지정 없이 생성만 하면 Droplet.cs가 알아서 중력으로 떨어뜨림)
        Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }
}