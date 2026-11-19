using System.Collections;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [Header("공격 설정")]
    public float warningDuration = 1.0f;
    public float attackLifetime = 2.0f;
    public float handRiseSpeed = 2f;
    public float handRiseHeight = 2f;
    public int damage = 10;
    public float attackInterval = 4f;
    public float detectionRadius = 12f;

    [Header("프리팹")]
    public GameObject warningPrefab;
    public GameObject handVisualPrefab;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            StartCoroutine(AttackLoop());
        }
        else
        {
            Debug.LogWarning("Player not found. Hand system disabled.");
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return StartCoroutine(AttackSequence());
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator AttackSequence()
    {
        if (player == null || Vector2.Distance(transform.position, player.position) > detectionRadius)
            yield break;

        Vector2 playerPos = player.position;
        Vector2 targetPos = GetGroundPosition(playerPos);

        GameObject warning = Instantiate(warningPrefab, targetPos, Quaternion.identity);
        yield return new WaitForSeconds(warningDuration);
        Destroy(warning);

        GameObject hand = Instantiate(handVisualPrefab, targetPos, Quaternion.identity);

        float startY = hand.transform.position.y;
        bool reachedTop = false;

        float elapsed = 0f;
        while (elapsed < attackLifetime)
        {
            if (hand == null)
                yield break;

            if (!reachedTop)
            {
                hand.transform.Translate(Vector3.up * handRiseSpeed * Time.deltaTime);
                if (hand.transform.position.y >= startY + handRiseHeight)
                    reachedTop = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(hand);
    }

    private Vector2 GetGroundPosition(Vector2 playerPosition)
    {
        int groundMask = LayerMask.GetMask("Ground", "Platform");
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, Vector2.down, 10f, groundMask);
        if (hit.collider != null)
            return hit.point;
        else
            return new Vector2(playerPosition.x, playerPosition.y - 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HP hp = other.GetComponent<HP>();
            if (hp != null)
                hp.TakeDamage(damage);
        }
    }
}
