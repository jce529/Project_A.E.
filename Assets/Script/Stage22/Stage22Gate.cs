// Phase 26 - physical blocker for a zone edge (e.g. 5->8 until waterway changed, 8->9 until tablet read,
// 7->8 always). Blocker is active while the move from -> to is not allowed.
using UnityEngine;

public class Stage22Gate : MonoBehaviour
{
    [SerializeField] Stage22Zone from = Stage22Zone.Z5;
    [SerializeField] Stage22Zone to = Stage22Zone.Z8;
    [SerializeField] GameObject blocker;
    [SerializeField] float pollInterval = 0.25f;

    float _next;

    void OnEnable() { Refresh(); }

    void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + pollInterval;
        Refresh();
    }

    void Refresh()
    {
        if (blocker != null) blocker.SetActive(!Stage22Progress.CanMove(from, to));
    }
}
