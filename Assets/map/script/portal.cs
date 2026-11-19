using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerInteractable
{
    public string sceneToLoad;
    public string targetSpawnName;

    public bool CanInteract(PlayerInteraction player)
    {
        return player != null && !string.IsNullOrEmpty(sceneToLoad) &&
            Application.CanStreamedLevelBeLoaded(sceneToLoad);
    }

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        // The current PlayerSpawner consumes this value; preserve the legacy manager too.
        PlayerSpawner.targetSpawnPointName = targetSpawnName;
        if (GameManager.Instance != null) GameManager.Instance.NextSpawnPointName = targetSpawnName;
        SceneManager.LoadScene(sceneToLoad);
    }
}
