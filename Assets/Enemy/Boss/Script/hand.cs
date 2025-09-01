using UnityEngine;

public class MudHandController : MonoBehaviour
{
    public float destroytime;
    public int damage;
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

    }

    void Update()
    {
        ////Destroy(gameObject, destroytime);
        //if (transform.position.y < initialY + 0.5f)
        //{
        //    transform.Translate(Vector2.up * handspeed * Time.deltaTime);

        //}
    }
    // Update is called once per frame
    void Attack()
    {
        animator.SetTrigger("Attack");   
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 플레이어에 "Player" 태그를 부여해야 합니다.
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>(); // 플레이어 체력 스크립트
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Destroy(gameObject);// 플레이어에게 데미지 적용

        }
    }
}
