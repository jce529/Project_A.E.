using UnityEngine;
using WaterMonster.Phase2;

public class WaveSlice : MonoBehaviour
{
    public float damage = 15f;
    public float radius = 2.5f;
    public GameObject waveEffectPrefab;
    public WaterController waterController;
    public PlayerStats playerStats;

    [SerializeField] private DamageElement element = DamageElement.Water;

    public void waveSlice()
    {
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
        {
            waterController.UseBottle(2);

            GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                // Phase 2: WaterPuddle destruction (D-12, D-13)
                if (hit.CompareTag("WaterPuddle"))
                {
                    var puddle = hit.GetComponent<WaterPuddle>();
                    if (puddle != null && puddle.isDestructible)
                    {
                        PuddlePool.Instance.Return(puddle);
                    }
                    continue;
                }

                if (!hit.CompareTag("HitBox")) continue;

                // 1. 보스 우선 검색 (BossStatsSystem — NewBoss / WaterMonster)
                var bossStats = hit.GetComponentInParent<BossStatsSystem>();
                if (bossStats != null)
                {
                    bossStats.TakeDamageInfo(new DamageInfo { amount = damage, element = element });
                    continue;
                }

                // 2. 일반 적 검색 (HP — 기존 적군)
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
