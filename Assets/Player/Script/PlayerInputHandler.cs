using UnityEngine;

/// <summary>
/// 플레이어의 공격 및 스킬 입력을 처리하는 추상 클래스입니다.
/// 어떤 키가 눌렸는지만 감지하고, 실제 행동은 자식 클래스에서 정의합니다.
/// </summary>
public abstract class PlayerInputHandler : MonoBehaviour
{
    // Update 함수를 virtual로 선언하여 자식 클래스가 원한다면 이 함수 자체를 재정의할 수도 있습니다.
    protected virtual void Update()
    {
        // 기본공격 입력 감지
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            OnBasicAttack();
        }

        // E 스킬 입력 감지
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnSkillE();
        }

        // R 스킬 입력 감지
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnSkillR();
        }

        // 1번 키(힐) 입력 감지
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnHeal();
        }

        // Q 입력 감지
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnSkillQ();
        }
    }

    // 자식 클래스가 반드시 구현해야 할 추상 메서드들을 선언합니다.
    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE();
    protected abstract void OnSkillR();
    protected abstract void OnHeal();
    protected abstract void OnSkillQ();
}