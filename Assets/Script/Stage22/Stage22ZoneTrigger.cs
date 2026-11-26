// Phase 26 - put on a trigger collider covering one 2-2 zone.
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Stage22ZoneTrigger : MonoBehaviour
{
    [SerializeField] Stage22Zone zone = Stage22Zone.Z1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Stage22ProgressMachine.Instance != null) Stage22ProgressMachine.Instance.TryEnter(zone);
    }
}
