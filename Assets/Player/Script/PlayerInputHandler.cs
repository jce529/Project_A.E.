using UnityEngine;

/// <summary>
/// �÷��̾��� ���� �� ��ų �Է��� ó���ϴ� �߻� Ŭ�����Դϴ�.
/// � Ű�� ���ȴ����� �����ϰ�, ���� �ൿ�� �ڽ� Ŭ�������� �����մϴ�.
/// </summary>
public abstract class PlayerInputHandler : MonoBehaviour
{
    protected virtual void Start()
    {
        if (InputHandler.Instance == null) { Debug.LogError("InputHandler가 씬에 없습니다!"); return; }
        InputHandler.Instance.OnBasicAttackEvent += OnBasicAttack;
        InputHandler.Instance.OnSkill1Event      += OnSkillE;
        InputHandler.Instance.OnSkill2Event      += OnSkillR;
        InputHandler.Instance.OnHealEvent        += OnHeal;
        InputHandler.Instance.OnSkillQEvent      += OnSkillQ;
    }

    protected virtual void OnDestroy()
    {
        if (InputHandler.Instance == null) return;
        InputHandler.Instance.OnBasicAttackEvent -= OnBasicAttack;
        InputHandler.Instance.OnSkill1Event      -= OnSkillE;
        InputHandler.Instance.OnSkill2Event      -= OnSkillR;
        InputHandler.Instance.OnHealEvent        -= OnHeal;
        InputHandler.Instance.OnSkillQEvent      -= OnSkillQ;
    }

    protected virtual void Update() { }

    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE();
    protected abstract void OnSkillR();
    protected abstract void OnHeal();
    protected abstract void OnSkillQ();
}