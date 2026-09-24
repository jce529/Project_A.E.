using UnityEngine;

// Phase 20 (D-13): one parentless root owns scene-independent managers.
// Add one Register<T>() call in Bootstrap to register another manager later.
// Only AudioManager is registered here; BUG-007 remains separate work.
public static class PersistentManagers
{
    private const string RootName = "PersistentManagers";
    private static GameObject root;

    public static Transform Root => root != null ? root.transform : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureRoot();
        Register<AudioManager>();
    }

    private static void EnsureRoot()
    {
        if (root != null) return;
        root = new GameObject(RootName);
        Object.DontDestroyOnLoad(root);
    }

    public static T Register<T>() where T : Component
    {
        EnsureRoot();
        T existing = root.GetComponent<T>();
        if (existing != null) return existing;
        return root.AddComponent<T>();
    }
}
