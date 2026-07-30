using UnityEngine;

// Put this on a GameObject with a BoxCollider2D (Is Trigger = on) that covers a boss arena (D-01).
// The trigger volumes themselves are placed manually per boss by the designer (D-02) and are
// intentionally NOT created by this phase (D-08).
//
// Zoom sizes and transition speed live on CameraController (normalZoom / bossZoom / zoomSmoothing),
// so this component has no fields and can be dropped on any boss zone as-is.
public class BossZoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetBossZoom(true);
    }

    // Leaving the zone reverts to the normal stage zoom immediately, no boss-death event needed (D-03).
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetBossZoom(false);
    }
}
