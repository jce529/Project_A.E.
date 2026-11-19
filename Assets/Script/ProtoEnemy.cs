using UnityEngine;
using UnityEngine.Events; // 이벤트를 사용하기 위해 추가

public class ProtoEnemy : MonoBehaviour
{
    public float hp = 100f;

    // 보스가 죽었을 때 실행할 이벤트 (에디터에서 SecretDoor를 연결할 곳)
    public UnityEvent onBossDefeated;

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log("프로토 보스 체력: " + hp);

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 연결된 이벤트(비밀의 문 잠금 해제) 실행!
        if (onBossDefeated != null)
        {
            onBossDefeated.Invoke();
        }

        Destroy(gameObject); // 보스 삭제
    }
}