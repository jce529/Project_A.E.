using UnityEngine;
using WaterMonster.Phase2;

public class WaveSlice : MonoBehaviour
{
    public float damage = 15f;
    public float radius = 2.5f;
    public GameObject waveEffectPrefab;
    public WaterController waterController;
    public PlayerStats playerStats;

    public void waveSlice()
    {
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
        {
            waterController.UseBottle(2);

            GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {

                // Phase 2: WaterPuddle destruction
                var puddle = hit.GetComponent<WaterPuddle>();
                if (puddle != null)
                {
                    PuddlePool.Instance.Return(puddle);
                    continue;
                }

                // 1. EnemyHitBox (자식 피격 판정 오브젝트) 우선 체크
                var enemyHitBox = hit.GetComponent<EnemyHitBox>();
                if (enemyHitBox != null)
                {
                    enemyHitBox.TakeDamage(new DamageInfo { amount = damage, type = DamageType.WaveSlash });
                    continue;
                }

                // 2. 보스 직접 검색 (BossStatsSystem — NewBoss / WaterMonster)
                var bossStats = hit.GetComponentInParent<BossStatsSystem>();
                if (bossStats != null)
                {
                    bossStats.TakeDamageInfo(new DamageInfo { amount = damage, type = DamageType.WaveSlash });
                    continue;
                }

                // 3. 일반 적 검색 (HP — 기존 적군)
                HP target = hit.GetComponentInParent<HP>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
            Destroy(wave, 1.0f);
        }
    }
}
