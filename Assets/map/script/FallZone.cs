using UnityEngine;

public class FallZone : MonoBehaviour
{
    [Header("낙사 데미지 설정")]
    public float fallDamage = 1f; // 여기서 원하는 데미지 수치를 설정하세요!

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 데미지 입히기 (PlayerStats 사용)
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(fallDamage);
            }

            // 2. 체크포인트로 위치 되돌리기 (PlayerRespawn 사용)
            PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
            if (playerRespawn != null)
            {
                playerRespawn.RespawnPosition();
            }
        }
    }
}