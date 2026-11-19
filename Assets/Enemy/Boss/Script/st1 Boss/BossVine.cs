using UnityEngine;

public class BossVine : MonoBehaviour
{
    [Header("Vine Prefab")]
    public GameObject vineVisualPrefab;

    [Header("Attack Settings")]
    public float attackLifetime = 2.0f;
    public float attackInterval = 4f;
    public float detectionRadius = 12f;

    // Manager가 이 함수를 호출해 덩쿨을 한 번 생성합니다.
    public void TriggerVine(Vector2 playerPosition)
    {
        Vector2 targetPos = GetGroundPosition(playerPosition);

        GameObject vine = Instantiate(vineVisualPrefab, targetPos, Quaternion.identity);

        // 생성된 덩쿨은 정해진 수명 이후 스스로 파괴되도록 Destroy 지연 기능 사용
        Destroy(vine, attackLifetime);
    }

    private Vector2 GetGroundPosition(Vector2 playerPosition)
    {
        int groundMask = LayerMask.GetMask("Ground", "Platform");
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, Vector2.down, 10f, groundMask);
        if (hit.collider != null) return hit.point;
        else return new Vector2(playerPosition.x, playerPosition.y - 1f);
    }
}