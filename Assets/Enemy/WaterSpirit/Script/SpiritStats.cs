using UnityEngine;

public class SpiritStats : BossStatsSystem
{
    public bool IsDummy { get; set; } = false;

    protected virtual void Reset()
    {
        // D-01c: Always disable barrier by setting MaxWater to 0
        MaxWater = 0f;
    }

    protected override void TakeDamage(DamageInfo info)
    {
        // D-01b: Guard clause
        if (info.amount <= 0f) return;

        // Direct health reduction, bypassing barrier logic entirely
        _currentHealth -= info.amount;
        Debug.Log($"[SpiritStats] 피격! 데미지: {info.amount}, 남은 체력: {_currentHealth}/{MaxHealth}");

        // Fire event for counter-attacks/behavior changes
        InvokeOnDamageTaken();

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            Die();
        }
    }

    protected override void Die()
    {
        Debug.Log("[SpiritStats] 사망 처리!");
        gameObject.SetActive(false);
    }
}
