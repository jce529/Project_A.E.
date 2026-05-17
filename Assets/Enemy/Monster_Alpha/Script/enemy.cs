using UnityEngine;
using UnityEngine.Events;

// 1. 파동참(스킬) 공격을 정상적으로 맞기 위해 필요한 인터페이스를 추가했어!
public interface IDamageable
{
    void TakeDamage(float damage);
}

// 2. 클래스 이름을 파일 이름과 똑같이 소문자 'enemy'로 맞췄어!
public class enemy : MonoBehaviour, IDamageable
{
    [Header("체력 설정")]
    public float health = 100f;

    [Header("원거리 공격 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float detectionRange = 5f;

    [Header("사망 시 발동할 이벤트")]
    public UnityEvent onDeathEvent;

    private Transform playerTransform;
    private bool playerInAttackRange = false;
    private float nextFireTime;

    void Start()
    {
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
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerInAttackRange = (distanceToPlayer <= detectionRange) && (playerTransform.position.y >= transform.position.y - 0.5f);

        if (playerInAttackRange)
        {
            Vector2 targetPos = (Vector2)playerTransform.position + new Vector2(0, 0.5f);
            Vector2 directionToPlayer = (targetPos - (Vector2)firePoint.position).normalized;

            // 방향 전환 로직
            if (directionToPlayer.x > 0 && transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (directionToPlayer.x < 0 && transform.localScale.x > 0)
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
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet bulletScript = bullet.GetComponent<bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }
    }

    // 플레이어의 공격을 받았을 때 실행되는 함수
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " 피격! 남은 체력 : " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 사망!");

        // 죽기 직전에 등록된 이벤트(비밀의 벽 해제 등)를 실행
        if (onDeathEvent != null)
        {
            onDeathEvent.Invoke();
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}