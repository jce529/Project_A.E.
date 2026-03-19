using UnityEngine;

/// <summary>
/// InputHandler의 이벤트를 구독하여 자식 클래스의 함수를 실행합니다.
/// </summary>
public abstract class PlayerAttackBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnBasicAttackEvent += OnBasicAttack;

            // [수정] InputHandler의 Skill1 이벤트 -> 자식의 OnSkillE 실행
            InputHandler.Instance.OnSkill1Event += OnSkillE;

            // [수정] InputHandler의 Skill2 이벤트 -> 자식의 OnSkillR 실행
            InputHandler.Instance.OnSkill2Event += OnSkillR;

            InputHandler.Instance.OnHealEvent += OnHeal;
        }
    }

    protected virtual void OnDisable()
    {
        if (InputHandler.Instance != null)
        {
            // [중요] OnEnable에서 연결한 것과 똑같은 짝으로 해제해야 합니다.
            InputHandler.Instance.OnBasicAttackEvent -= OnBasicAttack;
            InputHandler.Instance.OnSkill1Event -= OnSkillE;
            InputHandler.Instance.OnSkill2Event -= OnSkillR;
            InputHandler.Instance.OnHealEvent -= OnHeal;
        }
    }

    // 자식 클래스(PlayerAttack)가 구현할 추상 메서드들
    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE(); // Skill 1
    protected abstract void OnSkillR(); // Skill 2
    protected abstract void OnHeal();
}