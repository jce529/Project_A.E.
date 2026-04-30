using UnityEngine;

public class SpiritProjectileAttack : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Target == null) return;
        if (!(boss is SpiritController spirit)) return;

        if (spirit.ProjectilePrefab == null)
        {
            Debug.LogWarning("[SpiritProjectileAttack] ProjectilePrefab이 할당되지 않았습니다!");
            return;
        }

        // 발사 시점의 플레이어 방향 계산
        Vector2 dir = ((Vector2)boss.Target.position - (Vector2)boss.transform.position).normalized;

        // 투사체 생성
        var go = Object.Instantiate(spirit.ProjectilePrefab, boss.transform.position, Quaternion.identity);
        
        // 투사체 초기화
        var proj = go.GetComponent<SpiritProjectile>();
        if (proj != null)
        {
            proj.Init(dir, spirit.ProjectileDamage);
        }
    }
}
