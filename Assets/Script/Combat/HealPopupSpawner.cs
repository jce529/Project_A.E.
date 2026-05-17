// Phase 1 — HealPopupSpawner
// Static entry point. Loads HealPopup prefab from Resources and spawns it.
// Prefab creation is a Plan 05 manual step — this file degrades gracefully
// if the prefab does not exist.
using UnityEngine;

public static class HealPopupSpawner
{
    private static GameObject _cachedPrefab;
    private static bool _warned;

    public static void SpawnHealPopup(Vector3 worldPos, float amount)
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>("HealPopup");
        }
        if (_cachedPrefab == null)
        {
            if (!_warned)
            {
                Debug.LogWarning("[HealPopupSpawner] Resources/HealPopup.prefab not found. Create it in Plan 05.");
                _warned = true;
            }
            return;
        }
        var go = Object.Instantiate(_cachedPrefab, worldPos, Quaternion.identity);
        var popup = go.GetComponent<HealPopup>();
        if (popup != null) popup.Initialize(amount);
    }
}
