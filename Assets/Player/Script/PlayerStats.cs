/*
 * Author: ariel oliveira [o.arielg@gmail.com]
 * Modified based on user feedback.
 * PlayerStats.cs: HP를 상속받아 플레이어 고유의 기능을 추가한 스크립트
 */
using UnityEngine;

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
    private PlayerAnimator playerAnimator;

    private void Awake()
    {
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    public override void Heal(float amount)
    {
        health += amount;
        ClampHealth(); // 부모 클래스의 ClampHealth()를 호출하여 체력 보정 및 UI 업데이트
    }

    public void AddHealth()
    {
        if (maxHealth < maxTotalHealth)
        {
            maxHealth += 1;
            health = maxHealth; // 최대 체력이 늘어나면 현재 체력도 채워줌

            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        Debug.Log("Player has taken damage!");

        if (playerAnimator != null)
        {
            playerAnimator.PlayHitAnimation();
        }

        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.ResetComboState();
        }
    }
 
    void OnEnable()
    {
        // 액션 신호 구독
        DialogueManager.OnDialogueAction += HandleDialogueAction;
    }

    void OnDisable()
    {
        // 구독 해제
        DialogueManager.OnDialogueAction -= HandleDialogueAction;
    }

    private void HandleDialogueAction(string actionID)
    {
        switch (actionID)
        {
            case "Heal_50":
                Heal(50); // PlayerStats의 기존 메서드 활용
                Debug.Log("💚 대화창 신호 수신: 체력을 50 회복합니다.");
                break;
            case "Damage_10":
                TakeDamage(10); // PlayerStats의 기존 메서드 활용
                Debug.Log("💔 대화창 신호 수신: 체력을 10 감소시킵니다.");
                break;
        }
    }

}