using UnityEngine;

// 이 스크립트는 플레이어의 애니메이션과 스프라이트 반전(Flip)만 전담합니다.
public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    [Header("Animation Hashes")]
    private readonly int animIsMove = Animator.StringToHash("isMove");
    private readonly int animIsRun = Animator.StringToHash("isRun");
    private readonly int animIsJump = Animator.StringToHash("isJump");
    private readonly int animAttack = Animator.StringToHash("Attack");

    void Awake()
    {
        // 동일한 게임오브젝트에 있는 컴포넌트들을 가져옵니다.
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// PlayerController로부터 현재 물리/입력 상태를 전달받아 애니메이션을 업데이트합니다.
    /// </summary>
    public void UpdateAnimation(Vector2 moveInput, bool isRunning, bool isGrounded, bool movementLocked)
    {
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;

        // 1. 파라미터 업데이트
        anim.SetBool(animIsMove, isMoving);
        anim.SetBool(animIsRun, isMoving && isRunning);
        anim.SetBool(animIsJump, !isGrounded);

        // 2. 캐릭터 이동 방향에 따른 좌우 반전 (공격 중이 아닐 때만)
        if (!movementLocked)
        {
            if (moveInput.x > 0) spriteRenderer.flipX = false;
            else if (moveInput.x < 0) spriteRenderer.flipX = true;
        }
    }

    /// <summary>
    /// 외부(공격 스크립트 등)에서 호출하여 공격 애니메이션을 실행합니다.
    /// </summary>
    public void PlayAttackAnimation()
    {
        anim.SetTrigger(animAttack);
    }
}