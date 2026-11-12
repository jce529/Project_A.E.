// Phase 25 - place on a trigger collider per 2-1 area (1..6). Player entry advances the flow.
// Also removes fog on entry when fogArea is set (permanent once the lantern puzzle is solved).
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Stage21AreaTrigger : MonoBehaviour
{
    [Range(1, 6)] public int area = 1;
    [Tooltip("Entering this area removes the fog (if the lantern puzzle is solved).")]
    public bool removesFog;
    public UnityEvent onEntered;
    public UnityEvent onBlocked;   // entered out of order

    void Awake() { Stage21Progress.EnsureSubscribed(); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Stage21Progress.EnterArea(area)) onEntered.Invoke(); else onBlocked.Invoke();
        if (removesFog) Stage21Progress.TryRemoveFog();
    }
}
