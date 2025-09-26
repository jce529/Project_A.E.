using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 파일 이름과 클래스 이름을 'PlayerController' (P와 C 대문자)로 통일해주세요.
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D capsuleCollider;
    private PlayerInput playerInput;

    [Header("Player Settings")]
    public float jumpPower = 7f;
    public float defaultSpeed = 4f;
    public float runSpeed = 7f;

    [Header("Layer Masks")]
    public LayerMask groundLayer;
    public LayerMask platformLayer;

    // 내부 상태 변수
    private Vector2 moveInput;
    private float maxSpeed;
    private int jumpCount = 0;

    // Awake는 게임 시작 시 한 번만 호출됩니다.
    void Awake()
    {
        // 1. 필요한 컴포넌트들을 미리 찾아둡니다.
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerInput = GetComponent<PlayerInput>();

        // 2. 액션맵을 활성화합니다.
        playerInput.actions.FindActionMap("Player").Enable();

        // 3. 코드로 직접 이벤트를 구독(연결)합니다.
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Jump"].performed += OnJump;
    }

    // 오브젝트가 파괴될 때 구독을 해제하여 메모리 누수를 방지합니다.
    void OnDestroy()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Jump"].performed -= OnJump;
    }

    #region Input Callbacks
    // --- Input System이 호출할 함수들 ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // 버튼을 누르는 '순간'에만 실행됩니다.
        if (context.performed)
        {
            // 아래 방향키 또는 S키가 눌려있는지 직접 확인합니다.
            bool isPressingDown = moveInput.y < -0.5f;

            // 조건 1: 아래 점프 (아래를 누르고 있고, 플랫폼 위에 있을 때)
            if (isPressingDown && isPlatform())
            {
                // TemporarilyIgnoreOneWay 코루틴을 StartCoroutine으로 올바르게 호출합니다.
                StartCoroutine(TemporarilyIgnoreOneWay());
            }
            // 조건 2: 일반 점프
            else if (jumpCount < 1)
            {
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0); // Y축 속도를 초기화하여 일관된 점프 높이를 만듭니다.
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                jumpCount++;
            }
        }
    }
    #endregion

    #region Physics & Updates
    // --- 유니티 생명주기 함수들 ---

    // 매 프레임마다 호출됩니다.
    void Update()
    {
        resetJumpCount();
    }

    // 고정된 시간 간격으로 물리 계산에 사용됩니다.
    void FixedUpdate()
    {
        // 달리기 키(LeftShift) 확인
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        {
            maxSpeed = runSpeed;
        }
        else
        {
            maxSpeed = defaultSpeed;
        }

        // 이동 및 캐릭터 방향 전환
        rigid.linearVelocity = new Vector2(moveInput.x * maxSpeed, rigid.linearVelocity.y);

        if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = true;
    }
    #endregion

    #region Coroutines & Utility
    // --- 기타 유틸리티 함수 및 코루틴 ---

    // 땅 또는 플랫폼을 감지하는 함수들
    bool isGrounded() => capsuleCollider.IsTouchingLayers(groundLayer);
    bool isPlatform() => capsuleCollider.IsTouchingLayers(platformLayer);

    // 땅에 닿았을 때 점프 횟수를 초기화하는 함수
    void resetJumpCount()
    {
        if (isGrounded() || isPlatform())
        {
            jumpCount = 0;
        }
    }

    // 잠시 동안 플랫폼과의 충돌을 무시하는 코루틴
    IEnumerator TemporarilyIgnoreOneWay()
    {
        // 플랫폼과의 충돌을 일시적으로 비활성화
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), true);

        // 0.25초 동안 대기
        yield return new WaitForSeconds(0.25f);

        // 플랫폼과의 충돌을 다시 활성화
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), false);
    }
    #endregion
}