using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private PlayerRespawn playerRespawn;

    private void OnEnable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnInteractEvent += HandleInteractInput;
    }

    private void OnDisable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnInteractEvent -= HandleInteractInput;
    }

    private void HandleInteractInput()
    {
        // 범위 안에서 현재 설정된 상호작용 키를 눌렀을 때
        if (isPlayerInRange)
        {

            // Phase 11 (D-01): checkpoint interaction is a save trigger. The checkpoint's
            // own GameObject name is reused as the PlayerSpawner spawn point name (D-05).
            if (SaveLoadManager.Instance != null)
                SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name);

            if (playerRespawn != null)
            {
                playerRespawn.UpdateCheckpoint(this.transform);
            }
            else
            {
                Debug.LogError(" 실패: PlayerRespawn 스크립트를 찾지 못했습니다.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRespawn = collision.GetComponent<PlayerRespawn>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRespawn = null;
        }
    }
}
