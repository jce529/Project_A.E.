using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Shockwave : MonoBehaviour
{
    [SerializeField] private float speed = 8f;      // 이동 속도
    [SerializeField] private float lifetime = 2f;     // 지속 시간

    private Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
        // 왼쪽으로 갈 경우 이미지 좌우 반전
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("충격파 적중!");
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Pillar"))
        {
            Destroy(gameObject); // 벽에 막히면 소멸
        }
    }
}