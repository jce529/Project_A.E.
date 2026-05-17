using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Damager : MonoBehaviour
{
    [Header("공통 타겟 설정")]
    [Tooltip("데미지를 입힐 수 있는 오브젝트의 레이어를 선택합니다.")]
    public LayerMask damageableLayers;

    #region Unity Event Functions (충돌 감지)

    // --- ⭐ [수정됨] OnCollisionEnter와 OnTriggerEnter를 protected virtual로 변경 ---
    // 자식 클래스가 충돌 이벤트 자체를 오버라이드하여 더 복잡한 로직을 구현할 수 있도록 virtual로 선언합니다.
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplyDamage(collision.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other.gameObject);
    }

    #endregion

    /// <summary>
    /// 충돌한 게임 오브젝트에게 데미지를 입히려고 시도하는 공통 로직
    /// </summary>
    private void TryApplyDamage(GameObject target)
    {
        if (((1 << target.layer) & damageableLayers) != 0)
        {
            HP targetHP = target.GetComponent<HP>();
            if (targetHP != null)
            {
                ApplyDamageEffect(targetHP);
            }
        }
        // --- ⭐ [제거됨] 파괴 관련 로직을 여기서 더 이상 처리하지 않습니다. ---
    }

    /// <summary>
    /// (추상 메서드) 실제 데미지나 효과를 적용하는 부분.
    /// </summary>
    protected abstract void ApplyDamageEffect(HP targetHP);
}