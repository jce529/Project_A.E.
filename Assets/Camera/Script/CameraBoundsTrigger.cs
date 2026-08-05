using UnityEngine;

// Put this on a GameObject with a BoxCollider2D (Is Trigger = on) that covers one room / zone
// (260805-m41). Entering the zone hands CameraController the X limits that room should clamp to,
// so a single scene can frame room by room instead of sharing one pair of scene-wide bounds.
// Wide easter egg areas are the same script with wider values - no extra component needed.
//
// A zone is a SCOPED OVERRIDE, not a permanent handoff: the bounds that were active right before
// entering are cached and restored on exit, mirroring how BossZoomTrigger reverts its zoom (MX-05).
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

    // Single slot holding the bounds that were active immediately before this trigger fired.
    // One slot per trigger instance on purpose: no stack, no history, no zone manager (MX-05).
    // Known limitation, documented in Check.md and NOT fixed here: if two zones overlap or are
    // traversed out of order, this slot can hold the neighbouring zone's values instead of the
    // original base bounds, so the restore is frame-correct but logically stale. Level design
    // avoids it by not overlapping trigger volumes.
    private float _prevMinX;
    private float _prevMaxX;
    // False until this trigger has actually applied its own bounds. Guards the case where the
    // player SPAWNS inside the zone: enter never fires, so restoring would push 0 / 0 into the
    // camera and freeze it, since the clamp would then have its min above its max.
    private bool _hasCachedPrev;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // Cache BEFORE overwriting - reading after the call would just cache this zone's own values.
        _prevMinX = CameraController.Instance.minX;
        _prevMaxX = CameraController.Instance.maxX;
        _hasCachedPrev = true;
        CameraController.Instance.SetXBounds(zoneMinX, zoneMaxX);
    }

    // Leaving the zone restores the previous bounds immediately, the same way BossZoomTrigger
    // reverts its zoom on exit (MX-05).
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_hasCachedPrev) return;
        CameraController.Instance.SetXBounds(_prevMinX, _prevMaxX);
    }
}
