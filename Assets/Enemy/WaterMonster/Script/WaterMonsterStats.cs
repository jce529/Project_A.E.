// Phase 1 — WaterMonsterStats
// Decisions: D-08 (inherit BossStatsSystem), D-09 (MaxWater=0),
//            D-11 (Water heals, non-Water damages, HP-cost never kills)
// Requirements: REQ-WM-01, REQ-WM-02

using UnityEngine;

public class WaterMonsterStats : BossStatsSystem
{
    [Header("Water Monster Tuning")]
    [Tooltip("Multiplier applied to incoming Water damage when healing (1.0 = heal equal to damage).")]
    [SerializeField] private float waterHealMultiplier = 1f;

    public float CurrentHealth => _currentHealth;

    protected override void TakeDamage(DamageInfo info)
    {
        if (info.amount <= 0f) return;

        if (info.element == DamageElement.Water)
        {
            float healAmount = info.amount * waterHealMultiplier;
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + healAmount);
            OnHealed(healAmount);
            return;
        }

        // Non-water → standard damage path via base
        base.TakeDamage(info);
        InvokeOnDamageTaken();
    }

    /// <summary>
    /// Spend HP as an attack cost. Never kills the boss — clamps to minimum 1 HP.
    /// Does NOT fire OnDamageTaken (self-cost is not incoming damage).
    /// REQ-WM-02.
    /// </summary>
    public void SpendHpCost(float amount)
    {
        if (amount <= 0f) return;
        _currentHealth = Mathf.Max(1f, _currentHealth - amount);
    }

    /// <summary>
    /// Hook fired when Water-element damage heals the boss.
    /// Override in controller or wire via UnityEvent to trigger heal VFX / popup.
    /// </summary>
    public override void OnHealed(float amount)
    {
        HealPopupSpawner.SpawnHealPopup(transform.position + Vector3.up, amount);
    }

    protected override void Die()
    {
        gameObject.SetActive(false);
    }

    protected virtual void Reset()
    {
        MaxWater = 0f; // D-09: disable barrier / water decay naturally
    }
}
