using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime; // 총알의 수명
    public int damage;
    private Animator animator;
    private Rigidbody2D rb;

   
    void Awake() // Start 대신 Awake에서 GetComponent를 하는 것이 더 안정적입니다.
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet 오브젝트에 Rigidbody2D 컴포넌트가 없습니다!", this);
            Destroy(gameObject); // Rigidbody가 없으면 총알이 작동할 수 없으므로 파괴
            return;
        }
        Destroy(gameObject, lifeTime); // 일정 시간 후 총알 파괴
    }

    public void SetDirection(Vector2 direction)
    {
        // rb가 null이 아닌지 다시 한번 확인하여 NullReferenceException을 방지합니다.
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        else
        {
            Debug.LogError("Bullet의 Rigidbody2D가 null입니다. SetDirection 호출 불가.", this);
            Destroy(gameObject); // Rigidbody가 없으면 총알이 이동할 수 없으므로 파괴
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // "AttackRange" 태그가 정의되어 있는지 확인 (오류 1 해결 후)
        // 그리고 AttackRange 태그가 있는 오브젝트는 "Is Trigger"이므로,
        // Collider2D.CompareTag()를 사용하는 것은 적절합니다.
        if (other.CompareTag("Player"))
        {
            // 플레이어에게 데미지를 주는 로직 추가
            Debug.Log("플레이어 피격!");
            PlayerStats playerStats = other.GetComponent<PlayerStats>(); // 플레이어 체력 스크립트
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // AttackRange 콜라이더에 닿았을 때는 총알이 파괴되지 않도록 합니다.
        // 적 자신이나 적의 AttackRange 콜라이더와 충돌 시 총알이 파괴되지 않도록
        else if (!other.CompareTag("Enemy") && other.isTrigger == false)
        {
            // isTrigger가 아닌 다른 물리적 콜라이더와 충돌 시 파괴 (예: 벽)
            Destroy(gameObject);
        }
    }
}
