using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D capsuleCollider;
    private Animator anim;

    [Header("Player Settings")]
    public float jumpPower = 15f;
    public float defaultSpeed = 3f;
    public float runSpeed = 7f;
    public float ladderSpeed = 5f;
    [HideInInspector] public float speedModifier = 1f;

    [Header("Layer Masks")]
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public LayerMask ladderLayer;

    // 내부 상태 변수
    private Vector2 moveInput;
    private float maxSpeed;
    private int jumpCount = 0;
    private GameObject currentPlatform = null;
    private bool isRunning = false;

    private bool isKnockedBack = false;
    private bool isOnLadder = false;

    public bool IsImprisoned { get; set; }

    // 연결 여부 확인용
    private bool isConnected = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
    }

    // ★★★ [변경됨] Start에서 코루틴으로 연결 시도 ★★★
    // OnEnable 대신 Start를 사용해 확실하게 초기화 후 연결합니다.
    IEnumerator Start()
    {
        // 1. InputHandler가 준비될 때까지 무한 대기 (안전장치)
        while (InputHandler.Instance == null)
        {
            UnityEngine.Debug.LogWarning("⏳ [PlayerController] InputHandler 찾는 중...");
            yield return null; // 다음 프레임까지 대기
        }

        // 2. 준비 완료되면 연결
        ConnectInput();
    }

    void ConnectInput()
    {
        if (isConnected) return; // 이미 연결됐으면 패스

        InputHandler.Instance.OnMoveEvent += HandleMove;
        InputHandler.Instance.OnJumpEvent += HandleJump;
        InputHandler.Instance.OnRunEvent += HandleRun;

        isConnected = true;
        UnityEngine.Debug.Log("✅ [PlayerController] 드디어 연결 성공!");
    }

    // 오브젝트가 꺼지거나 파괴될 때 구독 해제
    void OnDisable()
    {
        if (isConnected && InputHandler.Instance != null)
        {
            InputHandler.Instance.OnMoveEvent -= HandleMove;
            InputHandler.Instance.OnJumpEvent -= HandleJump;
            InputHandler.Instance.OnRunEvent -= HandleRun;
            isConnected = false;
        }
    }

    // --- 입력 처리 함수들 ---

    private void HandleMove(Vector2 input)
    {
        moveInput = input;
        // UnityEngine.Debug.Log($"이동 입력: {input}"); // 입력 들어오는지 확인용

        if (moveInput.y < 0 && currentPlatform != null && !isOnLadder)
        {
            StartCoroutine(DropDownFromPlatform());
        }
    }

    private void HandleJump()
    {
        if (IsImprisoned) return;
        if (isOnLadder)
        {
            isOnLadder = false;
            rigid.gravityScale = 1f;
            anim.SetBool("isClimbing", false);

            jumpCount = 1;
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * (jumpPower * 0.7f), ForceMode2D.Impulse);
        }
        else if (jumpCount < 1)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    private void HandleRun(bool isPressed)
    {
        isRunning = isPressed;
    }

    // --- 물리 및 이동 로직 ---

    void FixedUpdate()
    {
        if (IsImprisoned) { rigid.linearVelocity = Vector2.zero; return; }
        if (isKnockedBack) return;

        if (isOnLadder)
        {
            HandleLadderMovement();
        }
        else
        {
            HandleGroundMovement();
        }
    }

    void Update()
    {
        if (!isOnLadder)
        {
            ResetJumpCount();
        }
    }

    private void HandleGroundMovement()
    {
        maxSpeed = (isRunning ? runSpeed : defaultSpeed) * speedModifier;

        rigid.linearVelocity = new Vector2(moveInput.x * maxSpeed, rigid.linearVelocity.y);

        anim.SetBool("isMove", Mathf.Abs(moveInput.x) > 0.1f);
        anim.SetBool("isRun", isRunning && Mathf.Abs(moveInput.x) > 0.1f);
        anim.SetBool("isJump", !IsGrounded());
        anim.SetBool("isClimbing", false);

        if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = true;
    }

    private void HandleLadderMovement()
    {
        rigid.gravityScale = 0f;
        rigid.linearVelocity = new Vector2(0f, moveInput.y * ladderSpeed);
        jumpCount = 0;

        anim.SetBool("isClimbing", Mathf.Abs(moveInput.y) > 0.1f);
        anim.SetBool("isMove", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isJump", false);
    }

    // --- 충돌 감지 및 유틸리티 ---

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & ladderLayer) != 0)
        {
            isOnLadder = true;
            rigid.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & ladderLayer) != 0)
        {
            isOnLadder = false;
            rigid.gravityScale = 1f;
            anim.SetBool("isClimbing", false);
        }
    }

    bool IsGrounded()
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

    IEnumerator DropDownFromPlatform()
    {
        if (currentPlatform != null)
        {
            Collider2D platformCollider = currentPlatform.GetComponent<Collider2D>();
            if (platformCollider != null)
            {
                platformCollider.isTrigger = true;
                yield return new WaitForSeconds(0.25f);
                platformCollider.isTrigger = false;
            }
        }
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
}