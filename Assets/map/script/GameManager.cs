using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string NextSpawnPointName { get => PlayerSpawner.targetSpawnPointName; set => PlayerSpawner.targetSpawnPointName = value; }

    private void Awake()
    {
        // Legacy scene-local singleton. Global services are owned by PersistentManagers.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
