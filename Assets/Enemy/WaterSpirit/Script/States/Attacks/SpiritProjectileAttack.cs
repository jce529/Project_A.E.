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

        // 루트 또는 자식에서 컴포넌트 탐색
        var proj = go.GetComponentInChildren<SpiritProjectile>();
        if (proj != null)
        {
            proj.Init(dir, spirit.ProjectileDamage, spirit.PlayerLayer);
            Debug.Log($"[SpiritProjectileAttack] 발사 dir={dir}, pos={boss.transform.position}");
        }
        else
        {
            Debug.LogWarning("[SpiritProjectileAttack] SpiritProjectile 컴포넌트를 찾을 수 없습니다! 프리팹 구조를 확인하세요.");
        }
    }
}
