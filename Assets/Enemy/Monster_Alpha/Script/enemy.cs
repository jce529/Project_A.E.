
using UnityEngine;

public class enemy : HP
{
    public GameObject bulletPrefab; // 발사할 총알 프리팹
    public Transform firePoint;     // 총알이 발사될 위치 (적 캐릭터의 자식 오브젝트로 설정)
    public float fireRate = 2f;     // 총알 발사 주기
    public float detectionRange = 5f; // 플레이어 감지 범위 (AttackRange 콜라이더와 일치시키는 것이 좋음)

    private Transform playerTransform; // 플레이어 Transform
    private bool playerInAttackRange = false; // 플레이어가 공격 범위 안에 있는지
    private float nextFireTime; // 다음 발사 가능 시간

    void Start()
    {
        // 플레이어 오브젝트를 찾습니다. (플레이어에 "Player" 태그를 꼭 붙여주세요!)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다. 플레이어에 'Player' 태그를 부여했는지 확인하세요.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return; // 플레이어가 없으면 아무것도 안 함

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerInAttackRange = (distanceToPlayer <= detectionRange) && (playerTransform.position.y >= transform.position.y - 0.5f);

        if (playerInAttackRange)
        {
            Vector2 targetPos = (Vector2)playerTransform.position + new Vector2(0, 0.5f);
            Vector2 directionToPlayer = (targetPos - (Vector2)firePoint.position).normalized;


            if (directionToPlayer.x > 0 && transform.localScale.x < 0) // 플레이어가 오른쪽에 있는데 적이 왼쪽을 보고 있으면
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (directionToPlayer.x < 0 && transform.localScale.x > 0) // 플레이어가 왼쪽에 있는데 적이 오른쪽을 보고 있으면
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // 총알 발사
            if (Time.time >= nextFireTime)
            {
                ShootBullet(directionToPlayer);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    void ShootBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet bulletScript = bullet.GetComponent<bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }
    }

    public override void Die()
    {
        Debug.Log(gameObject.name + " 사망!");
        base.Die();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}