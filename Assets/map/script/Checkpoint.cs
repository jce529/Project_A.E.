using UnityEngine;

public class Checkpoint : MonoBehaviour, IPlayerInteractable
{
    public bool isActiveCheckpoint = false;

    public bool CanInteract(PlayerInteraction player)
    {
        return player != null && player.Respawn != null && !isActiveCheckpoint;
    }

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
            checkpoint.isActiveCheckpoint = false;
        isActiveCheckpoint = true;
        player.Respawn.UpdateCheckpoint(transform);
    }
}
