using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

public class playerControll: MonoBehaviour
{
    private float maxSpeed;//달리기 속도
    public float ignoreTime = 0.25f; //타일무시시간
    public float jumpPower;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public LayerMask ladderLayer;
    public int jumpCount = 0;
    private float virticalInput;
    public float climbSpeed = 0.3f;// 사다리 오르는 속도
    private float defaultSpeed = 3;//걷기 속도
    public float dashSpeed;
    private float dashTime;
    public float defaultTime;



    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private CapsuleCollider2D capsuleCollider;

    private bool isLadder = false;
    private bool isdash;

    private LadderMove ladderMove;




    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < 1)
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
        }
        resetJumpCount();




    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (isGrounded() || isPlatform()) rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.linearVelocity.x > maxSpeed)
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < maxSpeed*(-1))
            rigid.linearVelocity = new Vector2(maxSpeed*(-1), rigid.linearVelocity.y);

        if (Input.GetKey(KeyCode.LeftShift))//달리기
        {
            maxSpeed = 6;
        }
        else
        {
            maxSpeed = defaultSpeed;
        }
        if (Input.GetKey(KeyCode.X))
        {
            isdash = true;
            maxSpeed = 6;
        }
        if (dashTime <= 0)
        {

            if (isdash)
                dashTime = defaultTime;
        }
        else
        {
            dashTime -= Time.deltaTime;
            maxSpeed = dashSpeed;
        }
        isdash = false;

        if (Input.GetAxisRaw("Vertical") < -0.5f && Input.GetButtonDown("Jump") && isPlatform()) //타일무시
        {
            TemporarilyIgnoreOneWay();
        }


    }

    bool isGrounded()
    {
        return capsuleCollider.IsTouchingLayers(groundLayer);
    }

    bool isPlatform()
    {
        return capsuleCollider.IsTouchingLayers(platformLayer);
    }

    void resetJumpCount()
    {
        if (isGrounded() || isPlatform())
        {
            jumpCount = 0; // Reset jump count when grounded
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isLadder = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isLadder = false;
            virticalInput = 0; // Reset vertical input when exiting the ladder
            rigid.gravityScale = 1; // Re-enable gravity
            capsuleCollider.isTrigger = false; // Re-enable collision with the ladder
        }
    }

    IEnumerator TemporarilyIgnoreOneWay()
    {
        Physics2D.IgnoreLayerCollision(8/*플레이어 레이어 인덱스*/, 6/*플랫폼 레이어 인덱스*/, true);

        // ignoreTime 만큼 대기
        yield return new WaitForSeconds(ignoreTime);

        // 충돌 복원
        Physics2D.IgnoreLayerCollision(8, 6, false);
    }
}
