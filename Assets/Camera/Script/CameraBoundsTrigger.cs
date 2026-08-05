using UnityEngine;

// Put this on a GameObject with a BoxCollider2D (Is Trigger = on) that covers one room / zone
// (260805-m41). Entering the zone hands CameraController the X limits that room should clamp to,
// so a single scene can frame room by room instead of sharing one pair of scene-wide bounds.
// Wide easter egg areas are the same script with wider values - no extra component needed.
//
// A zone is a SCOPED OVERRIDE, not a permanent handoff. On exit the camera is always sent back to
// CameraController's own minX / maxX - the fixed stage base bounds - instead of to whatever was
// active right before entering (Q2-06, supersedes MX-05). Nothing is remembered per trigger, so
// overlapping or out-of-order zones can no longer hand back a logically stale pair, and a player
// who SPAWNS inside a zone and walks out simply lands on the base bounds with no guard needed.
//
// The swap itself is not instant: CameraController eases its live bounds toward this pair at
// boundsSmoothing, so crossing a zone edge slides the clamp instead of snapping it (Q2-02).
//
// Because a zone only ever hands off to another zone or falls back to the base bounds, rooms are
// meant to be TILED - put a trigger on every stretch that has its own walls. Level design guide:
// Assets/Camera/Check.md, section "quick task 260805-q2u".
//
// The trigger volumes themselves are placed manually per zone by the designer, exactly like
// BossZoomTrigger: this script never creates or edits scene objects.
public class CameraBoundsTrigger : MonoBehaviour
{
    [Header("Zone X Bounds")]
    // World-space X limits handed to the camera on enter. Tune per zone in the Inspector.
    // Defaults match CameraController's own wide defaults, so a zone left untuned is a no-op
    // rather than a camera that snaps to the origin.
    // Keep the zone at least as wide as the camera view, or the clamp collapses onto a single X.
    public float zoneMinX = -1000f;
    public float zoneMaxX = 1000f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetXBounds(zoneMinX, zoneMaxX);
    }

    // Leaving the zone always falls back to the controller's fixed base bounds, the same way
    // BossZoomTrigger reverts to normalZoom on exit (Q2-06).
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetXBounds(CameraController.Instance.minX, CameraController.Instance.maxX);
    }
}
