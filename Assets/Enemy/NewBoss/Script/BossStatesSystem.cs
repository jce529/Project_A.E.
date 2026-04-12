using UnityEngine;
using System;

public class BossStatsSystem : MonoBehaviour
{
    [Header("Settings")]
    public float MaxHealth = 1000f;
    public float MaxWater = 100f;
    public float WaterDecayRate = 1.0f; // 초당 자원 소모

    protected float _currentHealth;
    protected float _currentWater;

    // [정보 제공 부분] 외부에서 현재 상태 볼 수 있게 '프로퍼티' 추가
    public float CurrentHealth => _currentHealth;
    public float CurrentWater => _currentWater;

    public bool IsBarrierActive => _currentWater > 0;

    // 이벤트: 배리어가 깨졌을 때나 데미지(반격) 발생 시
    public event Action OnWaterDepleted;
    public event Action OnDamageTaken;

    protected void InvokeOnDamageTaken() { OnDamageTaken?.Invoke(); }

    void Start()
    {
        _currentHealth = MaxHealth;
        _currentWater = MaxWater;
    }

    void Update()
    {
        // 배리어가 활성화되어 있으면 수분 소모
        if (IsBarrierActive)
        {
            ConsumeWater(WaterDecayRate * Time.deltaTime);
        }
    }

    public void ConsumeWater(float amount)
    {
        if (_currentWater <= 0) return;

        _currentWater -= amount;
        if (_currentWater <= 0)
        {
            _currentWater = 0;
            OnWaterDepleted?.Invoke(); // 물 소진 이벤트 발생
        }
        // UI 업데이트 등은 후속 호출
    }

    public void RestoreWater()
    {
        _currentWater = MaxWater;
        // 아머나 그로기 회복 시 호출
    }

    /// <summary>
    /// Hook fired when health is restored.
    /// </summary>
    public virtual void OnHealed(float amount) { }

    public void TakeDamage(float damage)
    {
        TakeDamage(new DamageInfo { amount = damage, element = DamageElement.None });
    }

    /// <summary>
    /// 외부 호출을 위한 DamageInfo 기반 데미지 진입점입니다.
    /// 서브클래스(WaterMonsterStats)에서 오버라이드한 로직이 실행되도록 protected virtual 메소드로 전달합니다.
    /// </summary>
    public void TakeDamageInfo(DamageInfo info)
    {
        TakeDamage(info);
    }

    protected virtual void TakeDamage(DamageInfo info)
    {
        if (info.amount <= 0f) return;

        if (IsBarrierActive)
        {
            // 배리어 있음: 수분 소모 (기존 로직: 20%)
            ConsumeWater(MaxWater * 0.2f);
            // 반격 상태 전환용 이벤트 알림
            OnDamageTaken?.Invoke();
        }
        else
        {
            // 배리어 없음: 체력 감소
            _currentHealth -= info.amount;
        }

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }

    protected virtual void Die() { /* 사망 처리 */ }
}
