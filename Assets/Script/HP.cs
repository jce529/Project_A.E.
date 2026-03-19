/*
 * HP.cs : 모든 캐릭터와 파괴 가능한 오브젝트의 기반이 되는 체력 스크립트
 */
using System;
using UnityEngine;

public class HP : MonoBehaviour
{
    // 체력 변경 시 다른 스크립트에 알리기 위한 델리게이트
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    [SerializeField]
    protected float health; // 자식 클래스에서 접근 가능하도록 protected로 변경
    [SerializeField]
    protected float maxHealth;

    [Header("Flash Effect")]
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    // [추가 1] 죽었을 때 알림을 보낼 이벤트
    public event Action OnDeath;

    // [추가 2] 체크하면 체력이 0이 되어도 바로 Destroy되지 않음 (보스용)
    public bool ManualDeath = false;

    // 외부에서 현재 체력과 최대 체력을 읽을 수 있도록 프로퍼티 추가
    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }

    protected virtual void Awake()
    {
        // 시작 시 체력을 최대로 설정
        health = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"{name}에 SpriteRenderer가 없습니다. Flash 효과가 작동하지 않습니다.");
        }
    }

    // 데미지를 받는 기능은 모든 HP를 가진 오브젝트의 공통 기능
    // virtual 키워드로 자식 클래스가 이 기능을 확장(override)할 수 있도록 허용
    public virtual void TakeDamage(float dmg)
    {
        health -= dmg;
        if (spriteRenderer != null)
            StartCoroutine(FlashColor());
        ClampHealth();

        // 데미지를 받아 체력이 0 이하가 되면 Die() 메서드 호출
        if (health <= 0)
        {
            Die();
        }
    }

    // 체력이 0이 되었을 때의 처리 (기본적으로는 오브젝트 파괴)
    // virtual 키워드로 자식 클래스가 죽었을 때의 행동을 다르게 정의할 수 있도록 함
    public virtual void Die()
    {
        // [추가 3] 죽었다는 신호를 먼저 보냄
        OnDeath?.Invoke();

        // 수동 사망(ManualDeath)이 켜져 있으면, 여기서 Destroy 하지 않고 끝냄
        if (ManualDeath)
        {
            Debug.Log($"{gameObject.name} 사망 상태 진입 (오브젝트 파괴는 보류)");
            return;
        }

        // 기존 로직 (일반 몬스터는 여기서 파괴됨)
        Destroy(gameObject);
        Debug.Log(gameObject.name + " has been destroyed.");
    }

    // 체력을 0과 maxHealth 사이의 값으로 유지하고, 변경 사항을 알림
    protected void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }

    public virtual void Heal(float amount)
    {
        health += amount;
        ClampHealth(); // 체력을 보정하고 UI 업데이트를 위해 호출
        Debug.Log(gameObject.name + " healed for " + amount + " points.");
    }

    protected System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}