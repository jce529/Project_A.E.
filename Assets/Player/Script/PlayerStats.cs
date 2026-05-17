/*
 * Author: ariel oliveira [o.arielg@gmail.com]
 * Modified based on user feedback.
 * PlayerStats.cs: HP를 상속받아 플레이어 고유의 기능을 추가한 스크립트
 */
using UnityEngine;

// 1. HP 클래스를 상속받습니다.
public class PlayerStats : HP
{
    #region Sigleton
    private static PlayerStats instance;
    public static PlayerStats Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<PlayerStats>();
            }
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private float maxTotalHealth; // 플레이어만 가지는 최대 성장 가능 체력
    public float MaxTotalHealth { get { return maxTotalHealth; } }

    // 2. 플레이어의 고유 기능인 '힐' 메서드를 추가합니다.
    public override void Heal(float amount)
    {
        health += amount;
        ClampHealth(); // 부모 클래스의 ClampHealth()를 호출하여 체력 보정 및 UI 업데이트
    }

    // 3. 플레이어의 고유 기능인 '최대 체력 증가' 메서드를 추가합니다.
    public void AddHealth()
    {
        if (maxHealth < maxTotalHealth)
        {
            maxHealth += 1;
            health = maxHealth; // 최대 체력이 늘어나면 현재 체력도 채워줌

            // 부모의 onHealthChangedCallback을 호출하여 UI 등 변경 사항 알림
            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }
    }

    // (선택 사항) 플레이어가 데미지를 받을 때 특별한 처리가 필요하다면 TakeDamage를 override 합니다.
    // 예: 무적 상태, 데미지 감소 등
    
    public override void TakeDamage(float dmg)
    {

        base.TakeDamage(dmg); // 부모 클래스(HP)의 원래 데미지 처리 로직 호출
        Debug.Log("Player has taken damage!");
    }
    
}