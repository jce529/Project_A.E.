using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (respawnPoint == null)
        {
            GameObject initialPoint = new GameObject("InitialCheckpoint");
            initialPoint.transform.position = transform.position;
            respawnPoint = initialPoint.transform;
        }
    }

    public void RespawnPosition()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void RespawnAtStart()
    {
        transform.position = startPosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void UpdateCheckpoint(Transform newPoint)
    {
        respawnPoint = newPoint;
    }

    // 포탈 이동 직후 호출되어 '현재 맵의 시작점'을 갱신함
    public void SyncStartPosition(Transform portalSpawnPoint)
    {
        startPosition = portalSpawnPoint.position;
        UpdateCheckpoint(portalSpawnPoint);
    }
}