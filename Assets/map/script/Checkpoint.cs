using UnityEngine;

public class Checkpoint : MonoBehaviour, IPlayerInteractable
{
    public bool CanInteract(PlayerInteraction player) => player != null;

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        var playerRespawn = player.GetComponent<PlayerRespawn>();
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
