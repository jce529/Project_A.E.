using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInteraction))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D capsuleCollider;
    private PlayerInput playerInput;
    private PlayerAnimator playerAnimator;

    [Header("Player Settings")]
    public float jumpPower = 15f;
    public float defaultSpeed = 3f;
    public float runSpeed = 7f;
    public float climbSpeed = 5f;

    [Header("장판 효과")]
    public float slowAmount = -1.5f; // 감속
    public float fastAmount = 3f;    // 가속
    public float currentSpeedModifier = 0f;
    private bool needSpeedReset = false;

    [Header("Layer Masks")]
    public LayerMask groundLayer;
    public LayerMask platformLayer;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    [Tooltip("Reference controller behavior: pressing Shift dashes, holding it runs. Disable to keep X dash.")]
    public bool dashOnRunPress = true;
    // Compatibility with callers using the supplied controller's dashPower name.
    public float dashPower { get => dashSpeed; set => dashSpeed = value; }
    public float dashDuration = 0.15f;
    public float dashCooldown = 1.0f;
    public Color dashColor = Color.cyan;

    private bool canDash = true;
    private bool isDashing = false;
    private Color originalColor;

    // 내부 상태 변수
    private Vector2 moveInput;
    private float maxSpeed;
    private int jumpCount = 0;
    private GameObject currentPlatform = null;

    private bool isKnockedBack = false;
    public bool movementLocked = false; // PlayerAttack과 연동됨

    private bool _isImprisoned;
    public bool IsImprisoned
    {
        get => _isImprisoned;
        set { _isImprisoned = value; movementLocked = value; }
    }

    public bool isLadder = false;
    public bool isClimbing = false;
    private float defaultGravity;

    void Awake()
    {
        if (GetComponent<PlayerInteraction>() == null) gameObject.AddComponent<PlayerInteraction>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerInput = GetComponent<PlayerInput>();

        playerAnimator = GetComponent<PlayerAnimator>();

        originalColor = spriteRenderer.color;
        defaultGravity = rigid.gravityScale;

        playerInput.actions.FindActionMap("Player").Enable();

        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Jump"].performed += OnJump;
    }
    // ==========================================
    // [대화 시스템 연동 이벤트 구독]
  
    void OnEnable()
    {
        // 대화가 시작/종료될 때 Lock/Unlock 함수를 실행하도록 방송 구독
        DialogueManager.OnDialogueStarted += LockForDialogue;
        DialogueManager.OnDialogueEnded += UnlockForDialogue;
    }

    void OnDisable()
    {
        DialogueManager.OnDialogueStarted -= LockForDialogue;
        DialogueManager.OnDialogueEnded -= UnlockForDialogue;
    }

    private void LockForDialogue()
    {
        movementLocked = true; // 이동 제한 스위치 ON
        moveInput = Vector2.zero; // 누르고 있던 키보드 입력 초기화

        // 💡 중요: 걷던 중에 대화가 시작되면 얼음판처럼 미끄러지지 않도록 X축 속도를 0으로 강제 고정 (Y축 중력은 유지)
        if (rigid != null)
        {
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
        }

        // 애니메이션을 강제로 Idle(대기) 상태로 변경
        if (playerAnimator != null)
        {
            playerAnimator.UpdateAnimation(Vector2.zero, false, IsGrounded(), true);
        }
    }

    private void UnlockForDialogue()
    {
        // 만약 대화가 끝났는데 플레이어가 감옥(IsImprisoned) 상태라면 풀어주면 안 되므로 예외 처리
        if (!_isImprisoned)
        {
            movementLocked = false;
        }
    }
 // ==========================================
    void OnDestroy()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Jump"].performed -= OnJump;
    }

    public void ApplyKnockback(Vector2 dir, float force)
    {
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 dir, float force)
    {
        isKnockedBack = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForFixedUpdate();
        rigid.linearVelocity = dir * force;

        yield return new WaitForSeconds(0.2f);
        isKnockedBack = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (moveInput.y < 0 && currentPlatform != null)
        {
            StartCoroutine(DropDownFromPlatform());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && !movementLocked)
        {
            if (isClimbing)
            {
                isClimbing = false;
                rigid.gravityScale = defaultGravity;
            }

            if (jumpCount < 1)
            {
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpPower);
                jumpCount++;
            }
        }
    }

    void Update()
    {
        if (movementLocked) return;

        ResetJumpCount();

        // 공중에서 벗어난 장판 효과를 착지 시 초기화
        if (IsGrounded() && needSpeedReset)
        {
            currentSpeedModifier = 0f;
            needSpeedReset = false;
        }

        if (Keyboard.current != null && (dashOnRunPress ? Keyboard.current.leftShiftKey.wasPressedThisFrame : Keyboard.current.xKey.wasPressedThisFrame) && canDash)
        {
            StartCoroutine(DashTowardsMouse());
        }
    }

    void FixedUpdate()
    {
        if (isDashing || isKnockedBack || movementLocked) return;

        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // 팀원의 장판 기믹 적용
        float baseSpeed = isRunning ? runSpeed : defaultSpeed;
        maxSpeed = baseSpeed + currentSpeedModifier;
        if (maxSpeed < 1f) maxSpeed = 1f; // 너무 느려져서 조작이 반전되는 것 방지

        if (isLadder && Mathf.Abs(moveInput.y) > 0.1f)
        {
            isClimbing = true;
        }

        if (isClimbing)
        {
            rigid.gravityScale = 0f;
            rigid.linearVelocity = new Vector2(moveInput.x * maxSpeed, moveInput.y * climbSpeed);
        }
        else
        {
            rigid.gravityScale = defaultGravity;
            rigid.linearVelocity = new Vector2(moveInput.x * maxSpeed, rigid.linearVelocity.y);
        }

        playerAnimator.UpdateAnimation(moveInput, isRunning, IsGrounded(), movementLocked);
    }

    private IEnumerator DashTowardsMouse()
    {
        canDash = false;
        isDashing = true;

        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // Keep ground collision active during dash, as in the supplied controller.
        spriteRenderer.color = dashColor;
        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rigid.linearVelocity = direction * dashSpeed;
            yield return null;
        }

        rigid.linearVelocity = Vector2.zero;
        rigid.gravityScale = originalGravity;
        // Ground collision stays enabled throughout the dash.
        spriteRenderer.color = originalColor;

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & platformLayer) != 0)
        {
            currentPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    public bool IsGrounded()
    {
        float extraHeight = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(capsuleCollider.bounds.center,
                                              Vector2.down,
                                              capsuleCollider.bounds.extents.y + extraHeight,
                                              groundLayer | platformLayer);
        return hit.collider != null;
    }

    bool IsOnPlatform() => currentPlatform != null;

    void ResetJumpCount()
    {
        if (IsGrounded() || IsOnPlatform())
        {
            jumpCount = 0;
        }
    }

    // 🌟 버그 수정됨: Trigger 방식을 버리고, 특정 플레이어와 플랫폼 간의 충돌만 무시하도록 변경
    IEnumerator DropDownFromPlatform()
    {
        if (currentPlatform != null)
        {
            Collider2D platformCollider = currentPlatform.GetComponent<Collider2D>();
            if (platformCollider != null)
            {
                Physics2D.IgnoreCollision(capsuleCollider, platformCollider, true);
                yield return new WaitForSeconds(0.25f);
                Physics2D.IgnoreCollision(capsuleCollider, platformCollider, false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isLadder = true;
        }
        else if (other.CompareTag("ground(slow)")) currentSpeedModifier = slowAmount; // 감속 장판
        else if (other.CompareTag("ground(fast)")) currentSpeedModifier = fastAmount; // 가속 장판
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
            rigid.gravityScale = defaultGravity;
        }
        else if (other.CompareTag("ground(slow)") || other.CompareTag("ground(fast)"))
        {
            if (!IsGrounded())
            {
                needSpeedReset = true; // 점프해서 벗어났으면 착지할 때까지 장판 효과 유지 (관성 점프 가능)
            }
            else
            {
                currentSpeedModifier = 0f;
            }
        }
    }
}