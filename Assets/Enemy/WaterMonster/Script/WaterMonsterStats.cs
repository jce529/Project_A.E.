// Phase 1 — WaterMonsterStats
// Decisions: D-08 (inherit BossStatsSystem), D-09 (MaxWater=0),
//            D-11 (Water heals, non-Water damages, HP-cost never kills)
// Requirements: REQ-WM-01, REQ-WM-02

using UnityEngine;

public class WaterMonsterStats : BossStatsSystem
{
    [Header("Enrage Tick")]
    [SerializeField] private float enrageTickInterval = 1.5f;
    [SerializeField] private float enrageTickAmount = 5f;

    private bool _isEnraged = false;
    private float _lastTickTime = 0f;

    public float CurrentHealth => _currentHealth;

    public void SetEnraged(bool value) { _isEnraged = value; }

    protected override void Update()
    {
        base.Update();
        if (!_isEnraged) return;
        if (Time.time - _lastTickTime < enrageTickInterval) return;
        _lastTickTime = Time.time;
        SpendHpCost(enrageTickAmount);
    }

    protected override void TakeDamage(DamageInfo info)
    {

        if (info.amount <= 0f) 
        {
            Debug.LogWarning("[WaterMonsterStats] 데미지량이 0 이하입니다.");
            return;
        }

        // 기본 공격(Normal)과 파동참(WaveSlash)만 데미지를 줌
        if (info.type == DamageType.Normal || info.type == DamageType.WaveSlash)
        {
            base.TakeDamage(info);
            InvokeOnDamageTaken();
            return;
        }

        // 그 외의 공격은 보스를 회복시킴
        float healAmount = info.amount;
        _currentHealth = Mathf.Min(MaxHealth, _currentHealth + healAmount);
        OnHealed(healAmount);
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
    /// 부적절한 공격(Type.Other 등)이 보스를 회복시킬 때 호출되는 훅입니다.
    /// </summary>
    public override void OnHealed(float amount)
    {
        HealPopupSpawner.SpawnHealPopup(transform.position + Vector3.up, amount);
    }

    protected override void Die()
    {
        // Phase 11 (D-01): boss defeat auto-save. Group B - BossStatsSystem has NO OnDeath
        // event, so the call goes directly inside this Die() override.
        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SaveOnBossDefeated("WaterMonster");

        gameObject.SetActive(false);
    }

    protected virtual void Reset()
    {
        MaxWater = 0f; // D-09: disable barrier / water decay naturally
    }
}
