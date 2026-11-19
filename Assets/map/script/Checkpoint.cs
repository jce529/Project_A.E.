using UnityEngine;

public class Checkpoint : MonoBehaviour, IPlayerInteractable
{
    public bool isActiveCheckpoint = false;
    public bool CanInteract(PlayerInteraction player) => player != null && player.Respawn != null;

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        SaveSlotDialog.Open(() => SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name),
            () => UpdateRespawn(player));
    }

    private void UpdateRespawn(PlayerInteraction player)
    {
        if (player == null) return;
        var playerRespawn = player.GetComponent<PlayerRespawn>();
        // Phase 11 (D-01): checkpoint interaction is a save trigger. The checkpoint's
        // own GameObject name is reused as the PlayerSpawner spawn point name (D-05).
        if (playerRespawn != null)
        {
            foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None)) checkpoint.isActiveCheckpoint = false;
            isActiveCheckpoint = true;
            playerRespawn.UpdateCheckpoint(this.transform);
        }
        else
        {
            Debug.LogError(" 실패: PlayerRespawn 스크립트를 찾지 못했습니다.");
        }
    }
}
