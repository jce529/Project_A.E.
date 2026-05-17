using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 보스 우선 검색 (BossStatsSystem — NewBoss / WaterMonster)
        var bossStats = other.GetComponentInParent<BossStatsSystem>();
        if (bossStats != null)
        {
            bossStats.TakeDamageInfo(new DamageInfo { amount = damage, type = DamageType.Normal });
            return;
        }

        // 2. 일반 HP 컴포넌트 검색 (기존 적군)
        HP hpTarget = other.GetComponentInParent<HP>();
        if (hpTarget != null)
        {
            hpTarget.TakeDamage(damage);
            return;
        }

        // 3. PlayerStats 컴포넌트 검색 (플레이어 타격 시)
        PlayerStats playerStatsTarget = other.GetComponentInParent<PlayerStats>();
        if (playerStatsTarget != null)
        {
            playerStatsTarget.TakeDamage(damage);
        }
    }
}
