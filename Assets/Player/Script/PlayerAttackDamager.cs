using UnityEngine;

public class PlayerAttackDamager : Damager
{
    public PlayerAttack playerAttack;
    private DamageType? _overrideType = null;

    public void SetOverrideType(DamageType type) => _overrideType = type;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        float dmg = playerAttack != null ? playerAttack.damage : 0;
        DamageInfo info = new DamageInfo { amount = dmg, type = _overrideType ?? DamageType.Normal };

        Debug.Log($"[PlayerAttackDamager] 충돌 감지: {other.gameObject.name} (layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // 1. EnemyHitBox (특수 피격 판정)
        var hitbox = other.GetComponent<EnemyHitBox>() ?? other.GetComponentInParent<EnemyHitBox>();
        if (hitbox != null)
        {
            Debug.Log($"[PlayerAttackDamager] EnemyHitBox 발견 → TakeDamage 전달 (ownerStats null이면 무효)");
            hitbox.TakeDamage(info);
            return;
        }

        // 2. BossStatsSystem (보스)
        var boss = other.GetComponent<BossStatsSystem>() ?? other.GetComponentInParent<BossStatsSystem>();
        if (boss != null)
        {
            boss.TakeDamageInfo(info);
            return;
        }

        // 3. 일반 적 (HP)
        var hp = other.GetComponent<HP>() ?? other.GetComponentInParent<HP>();
        if (hp != null)
        {
            Debug.Log($"[PlayerAttackDamager] HP 컴포넌트 발견 → {hp.gameObject.name}에 {dmg} 데미지");
            hp.TakeDamage(dmg);
            return;
        }

        Debug.Log($"[PlayerAttackDamager] {other.gameObject.name} — 데미지 대상 없음 (HP/BossStats/HitBox 모두 없음)");
    }

    protected override void ApplyDamageEffect(HP targetHP)
    {
        if (playerAttack == null) return;
        targetHP.TakeDamage(playerAttack.damage);
    }
}