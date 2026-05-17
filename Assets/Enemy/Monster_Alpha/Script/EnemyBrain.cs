using UnityEngine;
using System.Collections;

public enum EnemyType
{
    Melee,   // �Ϲ� ���� ����
    Ranged,  // ���Ÿ� ����
    Dash     // ���� ����
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour
{
    [Header("�� ���� ����")]
    public EnemyType enemyType;

    [Header("����(Dash) ����")]
    [Tooltip("�÷��̾ �� �Ÿ� �ȿ� ������ �����մϴ�.")]
    public float dashTriggerRange = 5f;
    public float dashForce = 15f;
    public float dashCooldown = 3f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("���Ÿ�(Ranged) ����")]
    [Tooltip("�÷��̾ �� �Ÿ� �ȿ� ������ ���缭 �����մϴ�.")]
    public float attackRange = 6f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab; // �߻�ü ������
    private float lastAttackTime;

    // ���� ������Ʈ ĳ��
    private Chase chaseComponent;
    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        chaseComponent = GetComponent<Chase>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (enemyType)
        {
            case EnemyType.Melee:
                // �Ϲ� ����: Chase ������Ʈ�� ��� �˾Ƽ� �ϵ��� �Ӵϴ�.
                if (chaseComponent != null && !chaseComponent.enabled)
                    chaseComponent.enabled = true;
                break;

            case EnemyType.Ranged:
                HandleRanged(distance);
                break;

            case EnemyType.Dash:
                HandleDash(distance);
                break;
        }
    }

    private void HandleDash(float distance)
    {
        if (isDashing) return;

        // ��Ÿ� �ȿ� ���԰� ���� ��Ÿ���� á�ٸ� ����
        if (distance <= dashTriggerRange && canDash)
        {
            StartCoroutine(DashRoutine());
        }
        else if (!isDashing && chaseComponent != null)
        {
            // ���� ���� �ƴ� ���� ���ó�� ����
            chaseComponent.enabled = true;
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;

        // 1. ���� �� ��� ���� (�غ� ���� - �÷��̾ ���� Ÿ�̹� ����)
        if (chaseComponent != null) chaseComponent.enabled = false;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        // 2. �÷��̾� �������� ���ϰ� ���� (�������� �� ����)
        if (player != null)
        {
            Vector2 dashDirection = (player.position - transform.position).normalized;
            rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        }

        // 3. ���� ���� �ð� ���
        yield return new WaitForSeconds(0.3f);

        // 4. ���� ���� �� ��Ÿ�� ����
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleRanged(float distance)
    {
        // ���Ÿ��� ��Ÿ� �ȿ� ������ ������ ���߰� ����
        if (distance <= attackRange)
        {
            if (chaseComponent != null) chaseComponent.enabled = false;
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ShootProjectile();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // ��Ÿ� ���̸� �ٽ� �ٰ���
            if (chaseComponent != null) chaseComponent.enabled = true;
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("�߻�ü �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // �÷��̾� �������� �߻�ü ���� (�߻�ü ��ü�� ��ũ��Ʈ�� ���ư��� ���� ó���Ѵٰ� ����)
        Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Debug.Log(gameObject.name + "��(��) ���Ÿ� ������ �߽��ϴ�!");
    }

    // �ν����Ϳ��� ��Ÿ��� ���� ���� �׷��ִ� ���
    void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyType.Dash)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, dashTriggerRange);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}