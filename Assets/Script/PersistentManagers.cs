using UnityEngine;

/// <summary>
/// Owns the lifetime of global gameplay services. The prefab is loaded before the
/// first scene so entering Play Mode from any scene produces the same service graph.
/// </summary>
public sealed class PersistentManagers : MonoBehaviour
{
    private const string ResourcePath = "PersistentManagers";
    private static PersistentManagers instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        PersistentManagers existing = FindAnyObjectByType<PersistentManagers>();
        if (existing != null)
        {
            instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogError(
                "PersistentManagers: Resources/PersistentManagers prefab could not be loaded.");
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.name = "PersistentManagers";
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
