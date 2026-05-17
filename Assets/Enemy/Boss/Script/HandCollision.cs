using UnityEngine;

public class HandCollision : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HP hp = other.GetComponent<HP>();
            if (hp != null)
                hp.TakeDamage(damage);

            Destroy(gameObject); // 손 오브젝트 삭제
        }
    }
}
