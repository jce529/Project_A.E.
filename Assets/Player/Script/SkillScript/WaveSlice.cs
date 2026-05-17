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
                Debug.Log($"[WaveSlice] 충돌 감지: {hit.name} (태그: {hit.tag})");

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
                    Debug.Log($"[WaveSlice] EnemyHitBox 감지! 파동참 데미지 {damage} 전달.");
                    enemyHitBox.TakeDamage(new DamageInfo { amount = damage, type = DamageType.WaveSlash });
                    continue;
                }

                // 2. 보스 직접 검색 (BossStatsSystem — NewBoss / WaterMonster)
                var bossStats = hit.GetComponentInParent<BossStatsSystem>();
                if (bossStats != null)
                {
                    Debug.Log($"[WaveSlice] BossStatsSystem 직접 감지! 파동참 데미지 {damage} 전달.");
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
        else {Debug.Log("���� �����մϴ�"); }
    }
}
