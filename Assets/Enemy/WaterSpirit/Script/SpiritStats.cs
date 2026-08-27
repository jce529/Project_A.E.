using UnityEngine;

public class SpiritStats : BossStatsSystem
{
    public bool IsDummy { get; set; } = false;
    private bool _stage2Triggered = false;

    protected virtual void Reset()
    {
        // D-01c: Always disable barrier by setting MaxWater to 0
        MaxWater = 0f;
    }

    protected override void TakeDamage(DamageInfo info)
    {
        // D-01b: Guard clause
        if (info.amount <= 0f) return;

        // D-07a: 분신 (IsDummy == true) 은 데미지를 받지 않음 (S2-05)
        if (IsDummy)
        {
            return;
        }

        // Direct health reduction, bypassing barrier logic entirely
        _currentHealth -= info.amount;

        // Fire event for counter-attacks/behavior changes
        InvokeOnDamageTaken();

        // CORE-03 (D-01a, D-01b, D-01c): HP 50% 이하 최초 도달 시 Stage 2 전환 (1회 가드)
        if (!_stage2Triggered && _currentHealth > 0f && _currentHealth <= MaxHealth * 0.5f)
        {
            _stage2Triggered = true;

            var spiritController = GetComponent<SpiritController>();
            if (spiritController != null)
            {
                spiritController.OnStage2Trigger();
            }
            else
            {
                Debug.LogWarning("[SpiritStats] SpiritController 컴포넌트를 찾을 수 없어 Stage 2 트리거 실패.");
            }
        }

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            Die();
        }
    }

    protected override void Die()
    {
        
        // Bug 1 해결: 보스 사망 시 모든 분신 제거
        var spiritController = GetComponent<SpiritController>();
        if (spiritController != null)
        {
            spiritController.CleanupClones();
        }

        // Phase 11 (D-01): boss defeat auto-save. Group B - BossStatsSystem has NO OnDeath
        // event, so the call goes directly inside this Die() override.
        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SaveOnBossDefeated("WaterSpirit");

        gameObject.SetActive(false);
    }
}
