/*
 * HP.cs : ��� ĳ���Ϳ� �ı� ������ ������Ʈ�� ����� �Ǵ� ü�� ��ũ��Ʈ
 */
using System;
using UnityEngine;

public class HP : MonoBehaviour
{
    // ü�� ���� �� �ٸ� ��ũ��Ʈ�� �˸��� ���� ��������Ʈ
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    [SerializeField]
    protected float health; // �ڽ� Ŭ�������� ���� �����ϵ��� protected�� ����
    [SerializeField]
    protected float maxHealth;

    [Header("Flash Effect")]
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    // [�߰� 1] �׾��� �� �˸��� ���� �̺�Ʈ
    public event Action OnDeath;

    // [�߰� 2] üũ�ϸ� ü���� 0�� �Ǿ �ٷ� Destroy���� ���� (������)
    public bool ManualDeath = false;

    // �ܺο��� ���� ü�°� �ִ� ü���� ���� �� �ֵ��� ������Ƽ �߰�
    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }

    protected virtual void Awake()
    {
        // ���� �� ü���� �ִ�� ����
        health = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"{name}�� SpriteRenderer�� �����ϴ�. Flash ȿ���� �۵����� �ʽ��ϴ�.");
        }
    }

    // �������� �޴� ����� ��� HP�� ���� ������Ʈ�� ���� ���
    // virtual Ű����� �ڽ� Ŭ������ �� ����� Ȯ��(override)�� �� �ֵ��� ���
    public virtual void TakeDamage(float dmg)
    {
        health -= dmg;
        if (spriteRenderer != null)
            StartCoroutine(FlashColor());
        ClampHealth();

        // �������� �޾� ü���� 0 ���ϰ� �Ǹ� Die() �޼��� ȣ��
        if (health <= 0)
        {
            Die();
        }
    }

    // ü���� 0�� �Ǿ��� ���� ó�� (�⺻�����δ� ������Ʈ �ı�)
    // virtual Ű����� �ڽ� Ŭ������ �׾��� ���� �ൿ�� �ٸ��� ������ �� �ֵ��� ��
    public virtual void Die()
    {
        // [�߰� 3] �׾��ٴ� ��ȣ�� ���� ����
        OnDeath?.Invoke();

        // ���� ���(ManualDeath)�� ���� ������, ���⼭ Destroy ���� �ʰ� ����
        if (ManualDeath)
        {
            Debug.Log($"{gameObject.name} ��� ���� ���� (������Ʈ �ı��� ����)");
            return;
        }

        // ���� ���� (�Ϲ� ���ʹ� ���⼭ �ı���)
        Destroy(gameObject);
        Debug.Log(gameObject.name + " has been destroyed.");
    }

    // ü���� 0�� maxHealth ������ ������ �����ϰ�, ���� ������ �˸�
    protected void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }

    public virtual void Heal(float amount)
    {
        health += amount;
        ClampHealth(); // ü���� �����ϰ� UI ������Ʈ�� ���� ȣ��
        Debug.Log(gameObject.name + " healed for " + amount + " points.");
    }

    protected System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}