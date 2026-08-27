using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private PlayerRespawn playerRespawn;
    public bool isActiveCheckpoint = false;

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
        // 범위 안에서 F키를 눌렀을 때
        if (isPlayerInRange)
        {

            if (!isActiveCheckpoint)
            {
                // 1. 씬에 있는 모든 체크포인트를 찾아서 끕니다.
                Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                foreach (Checkpoint cp in allCheckpoints)
                {
                    cp.isActiveCheckpoint = false;
                }

                // 2. 이 체크포인트만 켭니다.
                isActiveCheckpoint = true;

                // Phase 11 (D-01): checkpoint activation is a save trigger. The checkpoint's
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
            else
            {
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