// Phase 25 - area 6 exit gate. Puzzle solved -> onAllowed (wire to the 2-2 scene transition);
// otherwise onBlocked (show message / keep a barrier). Gate state is re-evaluated each time.
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Stage21Stage22Gate : MonoBehaviour
{
    public UnityEvent onAllowed;
    public UnityEvent onBlocked;

    void Awake() { Stage21Progress.EnsureSubscribed(); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Stage21Progress.CanEnterStage22()) onAllowed.Invoke(); else onBlocked.Invoke();
    }
}
