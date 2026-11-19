using UnityEngine;

public class LadderMovement : MonoBehaviour
{
    [Header("��ٸ� ����")]
    public float climbSpeed = 5f; // ��ٸ� Ÿ�� �ӵ�

    private float verticalInput;
    private bool isLadder;      // ��ٸ� ���� �ȿ� �ִ°�?
    private bool isClimbing;    // ���� ��ٸ��� Ÿ�� �ִ°�?

    private Rigidbody2D rb;
    private float defaultGravity; // ���� �߷� �� �����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; // ���� ���� �� ���� �߷� ���� ����ص�
    }

    void Update()
    {
        // W, S Ű �Է� �ޱ� (��: +1, �Ʒ�: -1, ����: 0)
        verticalInput = Input.GetAxisRaw("Vertical");

        // 1. ��ٸ� ���� �ȿ� �ְ�(isLadder) && ���Ʒ� Ű�� �����ٸ� -> ��ٸ� Ÿ�� ����
        if (isLadder && Mathf.Abs(verticalInput) > 0f)
        {
            isClimbing = true;
        }
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            // ��ٸ� Ÿ�� �߿��� �߷��� 0���� ���� �귯������ �ʰ� ��
            rb.gravityScale = 0f;

            // ���Ʒ��� �ӵ��� �� (�¿� �̵��� 0���� �����Ͽ� ��ٸ��� �� �ٰ� ��)
            rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed);
        }
        else
        {
            // ��ٸ��� �� Ÿ�� �ִٸ� ���� �߷����� ����
            rb.gravityScale = defaultGravity;
        }
    }

    // ��ٸ� ����(Trigger)�� ������ ��
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = true;
        }
    }

    // ��ٸ� ����(Trigger)���� ������ ��
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false; // ��ٸ����� ����� Ÿ�� ���� ����
        }
    }
}