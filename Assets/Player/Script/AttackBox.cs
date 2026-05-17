using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 충돌한 대상에게서 HP 컴포넌트를 먼저 찾아봅니다.
        HP hpTarget = other.GetComponentInParent<HP>();
        if (hpTarget != null)
        {
            // HP 컴포넌트를 찾았다면 데미지를 입히고 함수를 종료합니다.
            hpTarget.TakeDamage(damage);
          
            return; // 대상을 찾았으므로 더 이상 진행하지 않음
        }

        // 2. HP 컴포넌트가 없다면, PlayerStats 컴포넌트를 찾아봅니다.
        PlayerStats playerStatsTarget = other.GetComponentInParent<PlayerStats>();
        if (playerStatsTarget != null)
        {
            // PlayerStats 컴포넌트를 찾았다면 데미지를 입힙니다.
            playerStatsTarget.TakeDamage(damage);
            
        }
    }
}