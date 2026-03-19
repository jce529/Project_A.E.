using UnityEngine;
using System.Collections;

public class FloorHandAttack : IAttackStrategy
{
    // �������̽� ����
    public float Cooldown => 4.0f; // ���� attackInterval
    public string AnimationName => "Attack_FloorHand"; // �ִϸ��̼� �̸� (�ʿ�� ����)

    // hand.cs�� �ִ� ��������
    private GameObject warningPrefab;
    private GameObject handVisualPrefab;
    private float warningDuration = 1.0f;
    private float handRiseSpeed = 5f;
    private float handRiseHeight = 3f;
    private float attackLifetime = 2.0f;

    // ������: �ʿ��� �������� �޾ƿɴϴ�. (BossController���� �Ѱ��� ����)
    public FloorHandAttack(GameObject warning, GameObject hand)
    {
        this.warningPrefab = warning;
        this.handVisualPrefab = hand;
    }

    public void ExecuteAttack(BossController boss)
    {
        // BossController�� MonoBehaviour�̹Ƿ� �ڷ�ƾ�� ������ �� �ֽ��ϴ�.
        boss.StartCoroutine(AttackSequence(boss));
        Debug.Log("�ٴ� �� ���� ����!");
    }

    // ���� hand.cs�� AttackSequence�� ������
    private IEnumerator AttackSequence(BossController boss)
    {
        if (boss.Target == null) yield break;

        // 1. �÷��̾� �߹� ��ġ ���
        Vector2 targetPos = GetGroundPosition(boss.Target.position);

        // 2. ��� ǥ��
        if (warningPrefab != null)
        {
            GameObject warning = Object.Instantiate(warningPrefab, targetPos, Quaternion.identity);
            yield return new WaitForSeconds(warningDuration);
            Object.Destroy(warning);
        }

        // 3. �� ���� �� ���
        if (handVisualPrefab != null)
        {
            GameObject hand = Object.Instantiate(handVisualPrefab, targetPos, Quaternion.identity);
            float startY = hand.transform.position.y;
            float elapsed = 0f;

            while (elapsed < attackLifetime && hand != null)
            {
                // ���� ���� ���̱��� �ö�
                if (hand.transform.position.y < startY + handRiseHeight)
                {
                    hand.transform.Translate(Vector3.up * handRiseSpeed * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (hand != null) Object.Destroy(hand);
        }
    }

    // ���� hand.cs�� GetGroundPosition ���� ����
    private Vector2 GetGroundPosition(Vector2 playerPosition)
    {
        // Ground ���̾� ����ũ (�ʿ信 ���� �ε��� ����)
        int groundMask = 1 << LayerMask.NameToLayer("Ground");
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, Vector2.down, 10f, groundMask);

        if (hit.collider != null)
            return hit.point;
        else
            return new Vector2(playerPosition.x, playerPosition.y - 1f); // �ٴ� ������ �׳� �Ʒ�
    }
}