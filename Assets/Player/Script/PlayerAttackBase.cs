using UnityEngine;

/// <summary>
/// InputHandler�� �̺�Ʈ�� �����Ͽ� �ڽ� Ŭ������ �Լ��� �����մϴ�.
/// </summary>
public abstract class PlayerAttackBase : MonoBehaviour
{
    private bool _subscribed = false;

    protected virtual void Start()
    {
        Subscribe();
    }

    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_subscribed || InputHandler.Instance == null) return;
        InputHandler.Instance.OnBasicAttackEvent += OnBasicAttack;
        InputHandler.Instance.OnSkill1Event += OnSkillE;
        InputHandler.Instance.OnSkill2Event += OnSkillR;
        InputHandler.Instance.OnHealEvent += OnHeal;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || InputHandler.Instance == null) return;
        InputHandler.Instance.OnBasicAttackEvent -= OnBasicAttack;
        InputHandler.Instance.OnSkill1Event -= OnSkillE;
        InputHandler.Instance.OnSkill2Event -= OnSkillR;
        InputHandler.Instance.OnHealEvent -= OnHeal;
        _subscribed = false;
    }

    // �ڽ� Ŭ����(PlayerAttack)�� ������ �߻� �޼����
    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE(); // Skill 1
    protected abstract void OnSkillR(); // Skill 2
    protected abstract void OnHeal();
}