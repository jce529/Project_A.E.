using UnityEngine;
using UnityEngine.SceneManagement;

public class SignpostPortal : MonoBehaviour, IPlayerInteractable
{
    public string nextSceneName;
    public string spawnPointName;

    public bool CanInteract(PlayerInteraction player)
    {
        return player != null && !string.IsNullOrEmpty(nextSceneName) &&
            Application.CanStreamedLevelBeLoaded(nextSceneName);
    }

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        PlayerSpawner.targetSpawnPointName = spawnPointName;
        SceneManager.LoadScene(nextSceneName);
    }
}
