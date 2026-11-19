using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Droplet : MonoBehaviour
{
    [Header("투하 설정")]
    public int damage = 10;
    public float lifeTime = 3f;

    [Tooltip("떨어질 때 좌우로 살짝 흩뿌려지는 힘 (자연스러움 추가)")]
    public float randomXForce = 1.5f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // 1. 떨어지는 느낌을 위해 중력을 활성화합니다. (값이 클수록 무겁게 빨리 떨어짐)
        rb.gravityScale = 1.5f;

        // 2. 일직선으로만 떨어지면 기계적이니, 톡 떨어질 때 좌우로 살짝 랜덤한 힘을 줍니다.
        float randomX = Random.Range(-randomXForce, randomXForce);

        // 위로 살짝 튀어올랐다가(y: 1f) 떨어지게 만들면 더 '뱉어내는' 느낌이 납니다.
        rb.linearVelocity = new Vector2(randomX, 1f);

        // 3. 수명이 다하면 메모리에서 해제
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어에게 데미지 적용
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("ground(slow)")|| other.CompareTag("ground(fast)"))
        {
            Destroy(gameObject);
        }
    }
}