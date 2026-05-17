using UnityEngine;
using UnityEngine.Events;

public class HP : MonoBehaviour
{
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    [SerializeField]
    protected float health;
    [SerializeField]
    protected float maxHealth;

    [Header("사망 시 발동할 이벤트")]
    public UnityEvent onDeathEvent;

    [Header("Flash Effect")]
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }

    protected virtual void Awake()
    {
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

    public virtual void TakeDamage(float dmg)
    {
        health -= dmg;
        if (spriteRenderer != null)
            StartCoroutine(FlashColor());
        ClampHealth();

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (onDeathEvent != null)
        {
            onDeathEvent.Invoke();
        }

        Destroy(gameObject);
        Debug.Log(gameObject.name + " has been destroyed.");
    }

    protected void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }

    public virtual void Heal(float amount)
    {
        health += amount;
        ClampHealth();
        Debug.Log(gameObject.name + " healed for " + amount + " points.");
    }

    protected System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}