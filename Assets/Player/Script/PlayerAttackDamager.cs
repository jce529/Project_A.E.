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

        // 1. EnemyHitBox (특수 피격 판정)
        var hitbox = other.GetComponent<EnemyHitBox>() ?? other.GetComponentInParent<EnemyHitBox>();
        if (hitbox != null)
        {
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
            hp.TakeDamage(dmg);
        }
    }

    protected override void ApplyDamageEffect(HP targetHP) => targetHP.TakeDamage(playerAttack.damage);
}