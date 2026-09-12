using UnityEngine;
using UnityEngine.SceneManagement;

public class SignpostPortal : MonoBehaviour, IPlayerInteractable
{
    [Header("이동 설정")]
    public string nextSceneName;
    public string spawnPointName;

    public bool CanInteract(PlayerInteraction player)
    {
        return !string.IsNullOrWhiteSpace(nextSceneName)
            && Application.CanStreamedLevelBeLoaded(nextSceneName);
    }

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        PlayerSpawner.targetSpawnPointName = spawnPointName;
        SceneManager.LoadScene(nextSceneName);
    }
}
